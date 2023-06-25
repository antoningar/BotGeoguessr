namespace BotGeoGuessr.GeoGuessr.Models;

public class GameSettings
{
    public string Map { get; set; }
    public int Duration { get; set; }

    public GameSettings(string map, int duration)
    {
        Map = map;
        Duration = duration;
    }
}