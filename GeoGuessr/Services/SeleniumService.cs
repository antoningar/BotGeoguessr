using BotGeoGuessr.GeoGuessr.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.Services;

public class SeleniumService : ISeleniumService
{
    private const int ROUND_DELAY = 10000;
    
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpService _httpService;
    
    private readonly IWebDriver _webDriver;

    private readonly WebDriverWait _wait;

    public SeleniumService(ILogger logger, IHttpService httpService, IConfiguration configuration)
    {
        _logger = logger;
        _httpService = httpService;
        _configuration = configuration;

        ChromeOptions options = new();
        options.AddArgument("--headless");

        _webDriver = new ChromeDriver(options);
        _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(5));
    }

    public void Login()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(Login));
        GotToLoginPage();
        AcceptCookies();
        LoginForm();
    }

    public string GetJoinCode()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(GetJoinCode));
        GoToPartyPage();
        string url =  GetGameUrl();
        new Actions(_webDriver).SendKeys(Keys.Escape).Perform();
        return url;
    }

    public void AbortGame()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(AbortGame));
        try
        {
            IWebElement settingsBtn = _webDriver.FindElement(By.XPath("//button[@data-qa='in-game-settings-button']"));
            settingsBtn.Click();
            IWebElement abortBtn = _webDriver.FindElement(By.XPath("//button[@data-qa='abort-game-button']"));
            abortBtn.Click();
            IWebElement confirmBtn = _webDriver.FindElement(By.XPath("//button[@data-qa='confirmation-dialog-continue']"));
            confirmBtn.Click();
        }
        catch (Exception)
        {
            IWebElement settingsImg = _webDriver.FindElement(By.XPath("//img[@alt='Options']"));
            settingsImg.Click();
        }
    }

    public void DisbandParty()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(DisbandParty));
        
        IWebElement btn = _webDriver.FindElement(By.XPath("//button[@data-qa='disband-party-button']"));
        btn.Click();
        IWebElement confBtn = _webDriver.FindElement(By.XPath("//button[@data-qa='confirmation-dialog-continue']"));
        confBtn.Click();
    }

    public async Task UpdateSettings(GameSettings settings)
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(UpdateSettings));
        string ncfaCookie = GetNcfaCookie();
        await _httpService.UpdateSettings(ncfaCookie, settings);
    }

    public async Task<List<string>> GetMaps()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(GetMaps));
        string ncfaCookie = GetNcfaCookie();
        return await _httpService.GetMaps(ncfaCookie);
    }

    public async Task StartGame()
    {
        _logger.Information("{Class}.{Method} : Starting game", nameof(SeleniumService), nameof(StartGame));
        int userPresent = await GetPresentUser();

        if (userPresent > 1)
        {
            ClickStartGame();
            ManageRounds(userPresent);
            EndGame();
        }
        else
            throw new GeoGuessrException("missing user in game");
    }

    private void EndGame()
    {
        IWebElement spanFinish = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Finish game']")));
        IWebElement btnFinish = spanFinish.FindElement(By.XPath("./.."));
        btnFinish.Click();
        
        IWebElement spanContinue = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Continue']")));
        IWebElement btnContinue = spanContinue.FindElement(By.XPath("./.."));
        btnContinue.Click();
    }

    private void ManageRounds(int userPresent)
    {
        const int MAX_ROUND = 5;
        int round = 1;
        do
        {
            Thread.Sleep(3000);
            try
            {
                WaitForAllGuesses(userPresent - 1);
                Guess();
                ValidGuess();
            }
            catch (Exception ex) when (ex is NoSuchElementException or WebDriverTimeoutException
                                           or StaleElementReferenceException)
            {
                _logger.Information("{Class}.{Method} : game ended by timeout", nameof(SeleniumService),
                    nameof(ManageRounds));
            }
            finally
            {
                Thread.Sleep(ROUND_DELAY);
            }
            
            if (round == MAX_ROUND)
                return;
            
            StartNextRound();
            round++;
            
        } while (round <=  MAX_ROUND);
    }

    private void ValidGuess()
    {
        IWebElement span = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Guess']")));
        IWebElement btn = span.FindElement(By.XPath("./.."));
        btn.Click();
    }

    private void Guess()
    {
        IWebElement map = _webDriver.FindElement(By.XPath("//div[@data-qa='guess-map-canvas']"));
        new Actions(_webDriver)
            .MoveToElement(map)
            .Click()
            .Perform();
    }

    private void StartNextRound()
    {
        IWebElement span = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Start next round']")));
        IWebElement btn = span.FindElement(By.XPath("./.."));
        btn.Click();
    }

    private void WaitForAllGuesses(int guessRequired)
    {
        int guesses;
        do
        {
            IWebElement guessesDom =  _wait.Until(e => e.FindElement(By.XPath("//div[text()='Guesses Made']")));
            IWebElement guessParents = guessesDom.FindElement(By.XPath("./.."));
            IWebElement guessNumbers = guessParents.FindElement(By.CssSelector("div"));
            guesses = int.Parse(guessNumbers.Text.Split('/')[0].Trim());
        } while (guesses < guessRequired);
    }

    private async Task<int> GetPresentUser()
    {
        string ncfaCookie = GetNcfaCookie();
        string buildId = GetBuildId();
        
        return await _httpService.GetPresentUser(ncfaCookie, buildId);
    }

    private string GetNcfaCookie()
    {
        return _webDriver.Manage().Cookies.GetCookieNamed("_ncfa").ToString();
    }

    private string GetBuildId()
    {
        IWebElement script = _webDriver.FindElement(By.XPath("//script[@id='__NEXT_DATA__']"));
        string value = script.GetAttribute("innerText");
        dynamic jsonContent = JsonConvert.DeserializeObject<dynamic>(value)!;
        
        return jsonContent.buildId;
    }

    private string GetGameUrl()
    {
        Thread.Sleep(2000);
        IWebElement span;
        try
        {
            span = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Invite players']")));
        }
        catch (Exception)
        {
            span = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Invite']")));
        }
        
        IWebElement div = span.FindElement(By.XPath("./.."));
        div.Click();
        IWebElement link = _webDriver.FindElement(By.XPath("//input[@data-qa='copy-party-link']"));
        return link.GetAttribute("value");
    }


    private void LoginForm()
    {
        const string EMAIL_KEY = "GEOGUESSR_EMAIL";
        const string PASSWORD_KEY = "GEOGUESSR_PASSWORD";
        
        string email = _configuration.GetSection(EMAIL_KEY).Value!;
        string password = _configuration.GetSection(PASSWORD_KEY).Value!;
        
        IWebElement emailField = _webDriver.FindElement(By.XPath("//input[@data-qa='email-field']"));
        IWebElement passwordField = _webDriver.FindElement(By.XPath("//input[@data-qa='password-field']"));

        emailField.SendKeys(email);
        passwordField.SendKeys(password);
        passwordField.Submit();
    }

    private void AcceptCookies()
    {
        _logger.Information("{Class}.{Method} : Accepting cookies", nameof(SeleniumService), nameof(AcceptCookies));
        IWebElement btn = _webDriver.FindElement(By.Id("accept-choices"));
        btn.Click();
    }

    private void GotToLoginPage()
    {
        _logger.Information("{Class}.{Method} : Go to home page", nameof(SeleniumService), nameof(GotToLoginPage));
        const string HOME_URL = "https://www.geoguessr.com/signin";
        _webDriver.Navigate().GoToUrl(HOME_URL);
    }

    private void ClickStartGame()
    {
        IWebElement btn = _webDriver.FindElement(By.XPath("//button[@data-qa='party-start-game-button']"));
        btn?.Click();
    }

    private void GoToPartyPage()
    {
        _logger.Information("{Class}.{Method} : Go to party page", nameof(SeleniumService), nameof(GoToPartyPage));
        const string PARTY_URL = "https://www.geoguessr.com/party";
        _webDriver.Navigate().GoToUrl(PARTY_URL);
    }
}