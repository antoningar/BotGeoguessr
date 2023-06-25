using BotGeoGuessr.GeoGuessr;
using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.GeoGuessr.States;
using Moq;
using Serilog;

namespace BotGeoGuessrTests.GeoGuessr.States
{
    public class InitStateTest
    {
        private readonly Mock<IGeoGuessrContext> _mockContext = new();
        private readonly Mock<ISeleniumService> _mockSeleniumService = new();
        private readonly Mock<ILogger> _mockLogger = new();

        [Fact]
        public async Task Execute_ShouldCallGetoken()
        {
            InitState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();

            //_mockGeoGuessrService.Verify(s => s.GetTokenAsync(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldSetNewState()
        {
            InitState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();

            _mockContext.Verify(s => s.SetCurrentState(It.IsAny<CreateState>()), Times.Once);
        }
    }
}
