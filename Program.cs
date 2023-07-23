using BotGeoGuessr.GeoGuessr;
using BotGeoGuessr.GeoGuessr.Models;
using BotGeoGuessr.GeoGuessr.Options;
using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.Services;
using BotGeoGuessr.Validators;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace BotGeoGuessr
{
    internal static class Program
    {
        private const string TOKEN_KEY = "DISCORD_TOKEN";

        private static void Main(string[] args)
            => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            Logger logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            await using ServiceProvider services = ConfigureServices(configuration, logger);

            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            client.Log += LogAsync;
            services.GetRequiredService<CommandService>().Log += LogAsync;

            await client.LoginAsync(TokenType.Bot, configuration.GetRequiredSection(TOKEN_KEY).Value);
            await client.StartAsync();
            await services.GetRequiredService<BotService>().InitializeAsync();
            
            logger.Information("Program started");
            await Task.Delay(Timeout.Infinite);
        }
        private static ServiceProvider ConfigureServices(IConfiguration configuration, ILogger log)
        {
            const string BROWSWERLESS_KEY = "BROWSERLESS_OPTIONS";
            const string GEOGUESSR_KEY = "GEOGUESSR_OPTIONS";
            return new ServiceCollection()
                .Configure<BrowserlessOptions>(configuration.GetSection(BROWSWERLESS_KEY))
                .Configure<GeoguessrOptions>(configuration.GetSection(GEOGUESSR_KEY))
                .AddSingleton(new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
                })
                .AddSingleton<ISeleniumService, SeleniumService>()
                .AddSingleton<IGeoGuessrContext, GeoGuessrContext>()
                .AddSingleton<IHttpService, HttpService>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<IValidator<GameSettings>, GameSettingsValidator>()
                .AddSingleton<BotService>()
                .AddSingleton<HttpClient>()
                .AddSingleton(_ => configuration)
                .AddSingleton(_ => log)
                .BuildServiceProvider();
        }

        private static Task LogAsync(LogMessage log)
        {
            Log.Information("{Log}", log.ToString());
            return Task.CompletedTask;
        }
    }
}