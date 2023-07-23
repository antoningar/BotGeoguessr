namespace BotGeoGuessr.GeoGuessr.Options;

public class BrowserlessOptions
{
    public string? Token { get; set; }
    public int Timeout { get; set; }
    public bool Stealth { get; set; }
    public bool BlockAds { get; set; }
    public string? TrackingId { get; set; }
    public string? Url { get; set; }
}