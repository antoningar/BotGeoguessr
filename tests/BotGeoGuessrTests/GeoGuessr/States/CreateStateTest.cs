using BotGeoGuessr.GeoGuessr;
using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.GeoGuessr.States;
using Moq;
using Serilog;

namespace BotGeoGuessrTests.GeoGuessr.States
{
    public class CreateStateTest
    {
        private const string CODE = "joincode";
        
        private readonly Mock<IGeoGuessrContext> _mockContext = new();
        private readonly Mock<ISeleniumService> _mockSeleniumService = new();
        private readonly Mock<ILogger> _mockLogger = new();

        [Fact]
        public async Task Execute_ShouldCallGetJoinCode()
        {
            _mockSeleniumService
                .Setup(s => s.GetJoinCode())
                .Returns(CODE);

            CreateState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            _mockSeleniumService.Verify(s => s.GetJoinCode(), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldCallSetJoinCode()
        {
            _mockSeleniumService
                .Setup(s => s.GetJoinCode())
                .Returns(CODE);
            _mockContext
                .Setup(s => s.SetJoinCode(CODE));

            CreateState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            _mockContext.Verify(s => s.SetJoinCode(CODE), Times.Once);
        }

        [Fact]
        public async Task Execute_ShoulSetNextState()
        {
            _mockContext
                .Setup(s => s.SetCurrentState(It.IsAny<StartState>()));

            CreateState state = new(_mockContext.Object, _mockSeleniumService.Object, _mockLogger.Object);
            await state.Execute();
            
            _mockContext.Verify(s => s.SetCurrentState(It.IsAny<StartState>()), Times.Once);
        }
    }
}
