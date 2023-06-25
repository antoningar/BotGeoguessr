using BotGeoGuessr.GeoGuessr.Models;
using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.GeoGuessr.States;
using BotGeoGuessr.GeoGuessr.Status;
using Serilog;

namespace BotGeoGuessr.GeoGuessr
{
    public class GeoGuessrContext : IGeoGuessrContext
    {
        private readonly ILogger _logger;
        private readonly ISeleniumService _seleniumService;

        private State _state;
        private string _joinCode = string.Empty;

        public GeoGuessrContext(ISeleniumService seleniumService, ILogger logger)
        {
            _logger = logger;
            _seleniumService = seleniumService;

            _state = new InitState(this, seleniumService, logger);
            _state.Execute();
        }

        public async Task ExecuteState()
        {
            await _state.Execute();
        }

        public State GetCurrentState()
        {
            return _state;
        }

        public void SetCurrentState(State state)
        {
            _state = state;
            _logger.Information("{Class}.{Function} : Context state change to {StateName}", nameof(GeoGuessrContext), nameof(SetCurrentState), state.Status);
        }

        public string GetJoinCode()
        {
            return _joinCode;
        }

        public void SetJoinCode(string joinCode)
        {
            _joinCode = joinCode;
        }

        public async Task AbortGame()
        {
            if (_state.Status == StatusLabels.CREATE_STATUS)
                await Task.Run(() => _seleniumService.DisbandParty());
        }

        public async Task UpdateSettings(GameSettings settings)
        {
            await _seleniumService.UpdateSettings(settings);
        }

        public async Task<List<string>> GetMaps()
        {
            return await _seleniumService.GetMaps();
        }
    }
}
