using BotGeoGuessr.GeoGuessr.Models;

namespace BotGeoGuessr.GeoGuessr.Services
{
    public interface IHttpService
    {
        public Task GuessAsync(int round);
        public Task<int> GetPresentUser(string ncfaCookie, string buildId);
        public Task UpdateSettings(string sessionCookie, GameSettings settings);
        Task<List<string>> GetMaps(string ncfaCookie);
    }
}
