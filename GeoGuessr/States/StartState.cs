﻿using BotGeoGuessr.GeoGuessr.Services;
using BotGeoGuessr.GeoGuessr.Status;
using Serilog;

namespace BotGeoGuessr.GeoGuessr.States
{
    public class StartState : State
    {
        public StartState(IGeoGuessrContext context, ISeleniumService seleniumService, ILogger logger) : base(context, seleniumService, logger)
        {
            Status = StatusLabels.START_STATUS;
        }

        public override async Task Execute()
        {
            Logger.Information("{Class}.{Function} : state {StateName} execute", nameof(StartState), nameof(Execute), Status);
            Status = StatusLabels.PLAYING_STATUS;
            try
            {
                await SeleniumService.StartGame();
                Status = StatusLabels.START_STATUS;
            }
            catch (GeoGuessrException)
            {
                Logger.Information("{Class}.{Function} : not enough players", nameof(StartState), nameof(Execute));                Status = "START";
                Status = StatusLabels.START_STATUS;
                throw;
            }
        }
    }
}
