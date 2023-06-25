using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.GeoGuessr.Status;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.States
{
    public class InitState : State
    {
        public InitState(IGeoGuessrContext context, ISeleniumService seleniumService, ILogger logger) : base(context, seleniumService, logger)
        {
            Status = StatusLabels.INIT_STATUS;
        }

        public override async Task Execute()
        {
            Logger.Information("{Class}.{Function} : state {StateName} execute", nameof(StartState), nameof(Exception), Status);
            await Task.Run(() => SeleniumService.Login());
            Context.SetCurrentState(new CreateState(Context, SeleniumService, Logger));
        }
    }
}
