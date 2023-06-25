using BotGeoGuessr.GeoGuessr.Models;
using BotGeoGuessr.GeoGuessr.States;

namespace BotGeoGuessr.GeoGuessr
{
    public interface IGeoGuessrContext
    {
        public State GetCurrentState();
        public void SetCurrentState(State state);
        public Task ExecuteState();
        public string GetJoinCode();
        public void SetJoinCode(string joinCode);
        public Task AbortGame();
        public Task UpdateSettings(GameSettings settings);
        public Task<List<string>> GetMaps();
    }
}
