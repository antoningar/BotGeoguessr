using BotGeoGuessr.GeoGuessr;
using BotGeoGuessr.GeoGuessr.Services;
using Moq;
using Serilog;

namespace BotGeoGuessrTests.GeoGuessr.States
{
    public class StartStateTest
    {
        private readonly Mock<IGeoGuessrContext> _mockContext = new();
        private readonly Mock<ISeleniumService> _mockSeleniumService = new();
        private readonly Mock<ILogger> _mockLogger = new();
    }
}
