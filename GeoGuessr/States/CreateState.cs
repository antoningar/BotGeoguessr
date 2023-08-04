using BotGeoGuessr.GeoGuessr.Services;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.States
{
    public class CreateState : State
    {
        public CreateState(IGeoGuessrContext context, ISeleniumService seleniumService, ILogger logger) : base(context, seleniumService, logger)
        {
            Status = "CREATE";
            Logger.Debug("{Class}.{Function} : state set to {Status}", nameof(CreateState), nameof(CreateState), Status);
        }

        public override async Task Execute()
        {
            string code = await Task.Run(() => SeleniumService.GetJoinCode());
            Context.SetJoinCode(code);
            Context.SetCurrentState(new StartState(Context, SeleniumService, Logger));
        }
    }
}
