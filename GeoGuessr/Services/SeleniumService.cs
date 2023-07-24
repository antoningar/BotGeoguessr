using BotGeoGuessr.GeoGuessr.Models;
using BotGeoGuessr.GeoGuessr.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.Services;

public sealed class SeleniumService : ISeleniumService
{
    private const int ROUND_DELAY = 10000;
    
    private readonly ILogger _logger;
    private readonly IOptions<GeoguessrOptions> _geoguessrOptions;
    private readonly IHttpService _httpService;
    
    private readonly IWebDriver _webDriver;

    private readonly WebDriverWait _wait;

    public SeleniumService(ILogger logger, IHttpService httpService, IOptions<GeoguessrOptions> geoguessrOptions, IOptions<SeleniumServerOptions> seleniumServerOptions)
    {
        _logger = logger;
        _httpService = httpService;
        _geoguessrOptions = geoguessrOptions;

        ChromeOptions options = new();
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--disable-breakpad");
        options.AddArgument("--disable-component-extensions-with-background-pages");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-features=TranslateUI,BlinkGenPropertyTrees");
        options.AddArgument("--disable-ipc-flooding-protection");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--enable-features=NetworkService,NetworkServiceInProcess");
        options.AddArgument("--force-color-profile=srgb");
        options.AddArgument("--hide-scrollbars");
        options.AddArgument("--metrics-recording-only");
        options.AddArgument("--mute-audio");
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--window-size=1440,980");

        _webDriver = new RemoteWebDriver(
            new Uri(seleniumServerOptions.Value.Url!), options.ToCapabilities());
        
        _wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(5));
    }

    public void Login()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(Login));
        GotToLoginPage();
        try
        {
            AcceptCookies();
        }
        catch (NoSuchElementException)
        {
            _logger.Information("{Class}.{Method}: No cookies popup found", nameof(SeleniumService), nameof(Login));
        }
        LoginForm();
    }

    public string GetJoinCode()
    {
        _logger.Information("{Class}.{Method}", nameof(SeleniumService), nameof(GetJoinCode));
        GoToPartyPage();
        try
        {
            AcceptCookies();
        }
        catch (NoSuchElementException)
        {
            _logger.Information("{Class}.{Method}: No cookies popup found", nameof(SeleniumService), nameof(Login));
        }
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
        _logger.Information("{Class}.{Method} : Ending game", nameof(SeleniumService), nameof(EndGame));
        IWebElement spanFinish = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Finish game']")));
        IWebElement btnFinish = spanFinish.FindElement(By.XPath("./.."));
        btnFinish.Click();
        
        IWebElement spanContinue = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Continue']")));
        IWebElement btnContinue = spanContinue.FindElement(By.XPath("./.."));
        btnContinue.Click();
    }

    private void ManageRounds(int userPresent)
    {
        _logger.Information("{Class}.{Method} : Starting rounds management", nameof(SeleniumService), nameof(ManageRounds));
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
        _logger.Information("{Class}.{Method} : Validating guess", nameof(SeleniumService), nameof(ValidGuess));
        IWebElement span = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Guess']")));
        IWebElement btn = span.FindElement(By.XPath("./.."));
        btn.Click();
    }

    private void Guess()
    {
        _logger.Information("{Class}.{Method} : Guessing", nameof(SeleniumService), nameof(Guess));
        IWebElement map = _webDriver.FindElement(By.XPath("//div[@data-qa='guess-map-canvas']"));
        new Actions(_webDriver)
            .MoveToElement(map)
            .Click()
            .Perform();
    }

    private void StartNextRound()
    {
        _logger.Information("{Class}.{Method} : Starting next round", nameof(SeleniumService), nameof(StartNextRound));
        IWebElement span = _wait.Until(e => e.FindElement(By.XPath("//span[text()='Start next round']")));
        IWebElement btn = span.FindElement(By.XPath("./.."));
        btn.Click();
    }

    private void WaitForAllGuesses(int guessRequired)
    {
        _logger.Information("{Class}.{Method} : Waiting for guesses", nameof(SeleniumService), nameof(WaitForAllGuesses));
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
        _logger.Debug("{Class}.{Method} : Looking for ncfa cookie in cookies {Cookies}",
            nameof(SeleniumService), nameof(GotToLoginPage), _webDriver.Manage().Cookies);
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
        new Actions(_webDriver).MoveToElement(div).Click(div).Perform();

        IWebElement link = _webDriver.FindElement(By.XPath("//input[@data-qa='copy-party-link']"));
        return link.GetAttribute("value");
    }


    private void LoginForm()
    {
        IWebElement emailField = _webDriver.FindElement(By.XPath("//input[@data-qa='email-field']"));
        IWebElement passwordField = _webDriver.FindElement(By.XPath("//input[@data-qa='password-field']"));

        emailField.SendKeys(_geoguessrOptions.Value.Email!);
        passwordField.SendKeys(_geoguessrOptions.Value.Password!);
        passwordField.Submit();
    }

    private void AcceptCookies()
    {
        _logger.Information("{Class}.{Method} : Accepting cookies", nameof(SeleniumService), nameof(AcceptCookies));
        IWebElement btn = _webDriver.FindElement(By.Id("onetrust-accept-btn-handler"));
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
        _logger.Information("{Class}.{Method} : Click on starting game", nameof(SeleniumService), nameof(ClickStartGame));
        IWebElement btn = _webDriver.FindElement(By.XPath("//button[@data-qa='party-start-game-button']"));
        btn?.Click();
    }

    private void GoToPartyPage()
    {
        _logger.Information("{Class}.{Method} : Go to party page", nameof(SeleniumService), nameof(GoToPartyPage));
        const string PARTY_URL = "https://www.geoguessr.com/party";
        _webDriver.Navigate().GoToUrl(PARTY_URL);
    }

    ~SeleniumService()
    {
        _logger.Information("{Class}.Destructor", nameof(SeleniumService));
        _webDriver.Dispose();
    }
}