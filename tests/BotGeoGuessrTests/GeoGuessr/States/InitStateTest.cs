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
        public async Task Execute_ShouldLogin()
        {
            _mockSeleniumService
                .Setup(s => s.Login());

            InitState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            _mockSeleniumService.Verify(s => s.Login(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShoulSetNextState()
        {
            _mockContext
                .Setup(s => s.SetCurrentState(It.IsAny<CreateState>()));

            InitState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            _mockContext.Verify(s => s.SetCurrentState(It.IsAny<CreateState>()), Times.Once);
        }
    }
}
