using BotGeoGuessr.GeoGuessr.Models;

namespace BotGeoGuessr.GeoGuessr.Services;

public interface ISeleniumService
{
    public Task StartGame();
    void Login();
    string GetJoinCode();
    void DisbandParty();
    Task UpdateSettings(GameSettings settings);
    Task<List<string>> GetMaps();
}