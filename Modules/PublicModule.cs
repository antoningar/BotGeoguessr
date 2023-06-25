using BotGeoGuessr.GeoGuessr;
using BotGeoGuessr.GeoGuessr.Models;
using Discord.Commands;
using FluentValidation;
using FluentValidation.Results;
using Serilog;

namespace BotGeoGuessr.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>, IPublicModule
    {
        private readonly IGeoGuessrContext _geoGuessrContext;
        private readonly ILogger _logger;
        private readonly IValidator<GameSettings> _gameSettingsValidator;

        private const string CREATE_STATE_NAME = "CREATE";
        private const string START_STATE_NAME = "START";

        private const string CREATE_COMMAND = "creategame";
        private const string START_COMMAND = "startgame";
        private const string ABORT_COMMAND = "abort";
        private const string SETTINGS_OPTION_COMMAND = "settings";
        private const string MAPS_COMMAND = "maps";

        public PublicModule(IGeoGuessrContext geoGuessrContext, ILogger logger, IValidator<GameSettings> gameSettingsValidator)
        {
            _geoGuessrContext = geoGuessrContext;
            _logger = logger;
            _gameSettingsValidator = gameSettingsValidator;
        }

        [Command(MAPS_COMMAND)]
        public async Task GetMaps()
        {
            _logger.Information("{Class}.{Function} : Command {CommandName} triggered", nameof(PublicModule), nameof(GetMaps), SETTINGS_OPTION_COMMAND);

            List<string> maps = await _geoGuessrContext.GetMaps();
            if (maps.Count == 0)
                await ReplyAsync("Erreur de recuperartion des maps");
            else
                await ReplyAsync("Les maps possible sont :\n" + string.Join('\n', maps));
        }

        [Command(SETTINGS_OPTION_COMMAND)]
        public async Task UpdateSettings(string map, int duration)
        {
            _logger.Information("{Class}.{Function} : Command {CommandName} triggered", nameof(PublicModule), nameof(UpdateSettings), SETTINGS_OPTION_COMMAND);

            if (_geoGuessrContext.GetCurrentState().Status != "START")
            {
                await ReplyAsync("Créez une partie ou attendez la fin de la partie en cours pour modifier ses parametres");
                return;
            }
            
            GameSettings settings = new(map, duration);
            ValidationResult? validate = await _gameSettingsValidator.ValidateAsync(settings);
            
            if (!validate.IsValid)
                await ReplyAsync(validate.Errors.Last().ToString());
            else
                await _geoGuessrContext.UpdateSettings(settings);
        }

        [Command(ABORT_COMMAND)]
        public async Task AbortGameAsync()
        {
            _logger.Information("{Class}.{Function} : Command {CommandName} triggered", nameof(PublicModule), nameof(AbortGameAsync), ABORT_COMMAND);
            await _geoGuessrContext.AbortGame();
        }

        [Command(CREATE_COMMAND)]
        public async Task CreateGameAsync()
        {
            _logger.Information("{Class}.{Function} : Command {CommandName} triggered", nameof(PublicModule), nameof(CreateGameAsync), CREATE_COMMAND);
            if (CREATE_STATE_NAME == _geoGuessrContext.GetCurrentState().Status)
            {
                await _geoGuessrContext.ExecuteState();
                await ReplyAsync(_geoGuessrContext.GetJoinCode());
            }
            else
                await ReplyAsync($"Une game est deja en cours, rejoignez la ici : {_geoGuessrContext.GetJoinCode()}");
        }

        [Command(START_COMMAND)]
        public async Task StartGameAsync()
        {
            _logger.Information("{Class}.{Function} : Command {CommandName} triggered", nameof(PublicModule), nameof(StartGameAsync), START_COMMAND);
            if (START_STATE_NAME == _geoGuessrContext.GetCurrentState().Status)
                try
                {
                    await _geoGuessrContext.ExecuteState();
                }
                catch (GeoGuessrException ex)
                {
                    await ReplyAsync(ex.Message);
                }
            else
            {
                string url = _geoGuessrContext.GetJoinCode();
                if (string.IsNullOrWhiteSpace(url))
                    await ReplyAsync($"Pas de game en cours");
                else
                    await ReplyAsync($"Une game est deja en cours, rejoignez la ici : {url}");
            }
        }
    }
}
