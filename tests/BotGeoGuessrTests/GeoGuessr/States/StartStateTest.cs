using BotGeoGuessr.GeoGuessr;
using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.GeoGuessr.States;
using BotGeoGuessr.GeoGuessr.Status;
using Moq;
using Serilog;

namespace BotGeoGuessrTests.GeoGuessr.States
{
    public class StartStateTest
    {
        private readonly Mock<IGeoGuessrContext> _mockContext = new();
        private readonly Mock<ISeleniumService> _mockSeleniumService = new();
        private readonly Mock<ILogger> _mockLogger = new();

        [Fact]
        public async Task Execute_ShouldStartgame()
        {
            _mockSeleniumService
                .Setup(s => s.StartGame());

            StartState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            _mockSeleniumService.Verify(s => s.StartGame(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldSetStatus()
        {
            StartState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            Assert.Equal(StatusLabels.START_STATUS, state.Status);
        }

        [Fact]
        public async Task Execute_ShouldThrowGeoGuessrException()
        {
            _mockSeleniumService
                .Setup(s => s.StartGame())
                .ThrowsAsync(new GeoGuessrException(""));

            StartState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await Assert.ThrowsAsync<GeoGuessrException>(() => state.Execute());
        }
    }
}
