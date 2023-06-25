using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BotGeoGuessr.Services
{
    public class BotService : IBotService
    {
        private const char COMMAND_PREFIX = '!';
        private const string CHANNEL_NAME_KEY = "CHANNEL_NAME";

        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _discord;

        private readonly IServiceProvider _services;

        private readonly IConfiguration _config;

        public BotService(IServiceProvider services, IConfiguration configuration)
        {
            _services = services;
            _config = configuration;
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();

            _discord.MessageReceived += MessageReceivedAsync;
        }
        
        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message)
                return;
            if (message.Source != MessageSource.User)
                return;
            if (message.Channel.Name != _config.GetSection(CHANNEL_NAME_KEY).Value)
                return;

            int argPos = 0;
            if (!message.HasCharPrefix(COMMAND_PREFIX, ref argPos))
                return;

            SocketCommandContext context = new(_discord, message);
            await _commandService.ExecuteAsync(context, argPos, _services);
        }
    }
}
