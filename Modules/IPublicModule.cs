namespace BotGeoGuessr.Modules
{
    internal interface IPublicModule
    {
        public Task AbortGameAsync();
        public Task StartGameAsync();
        public Task CreateGameAsync();
        public Task UpdateSettings(string map, int duration);
    }
}
