using Microsoft.Extensions.Configuration;

namespace BotGeoGuessrTests
{
    public static class TestHelper
    {
        public static IConfiguration GetFakeInMemorySettings()
        {
            Dictionary<string, string>? settings = new() {
                {"TOKEN", "XXxxXX"},
                {"CHANNEL_NAME", "g�n�ral"},
                {"GEOGUESSR_EMAIL", "okletsgo@gmail.com"},
                {"GEOGUESSR_PASSWORD", "bahtiensokletsgo"},
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();
        }
    }
}