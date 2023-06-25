using BotGeoGuessr.GeoGuessr.Services;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.States
{
    public abstract class State
    {
        protected readonly IGeoGuessrContext Context;
        protected readonly ISeleniumService SeleniumService;
        protected readonly ILogger Logger;

        public string? Status;

        protected State(IGeoGuessrContext context, ISeleniumService seleniumService, ILogger logger)
        {
            Context = context;
            SeleniumService = seleniumService;
            Logger = logger;
        }

        public abstract Task Execute();
    }
}
