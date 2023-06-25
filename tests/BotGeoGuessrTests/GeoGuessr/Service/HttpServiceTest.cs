using Moq;
using Serilog;

namespace BotGeoGuessrTests.GeoGuessr.Service
{
    public class HttpServiceTest
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler = new();
        private readonly Mock<ILogger> _mockLogger = new();
    }
}
