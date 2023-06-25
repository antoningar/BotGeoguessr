using BotGeoGuessr.GeoGuessr.Models;
using FluentValidation;

namespace BotGeoGuessr.Validators
{
    public class GameSettingsValidator : AbstractValidator<GameSettings>
    {
        public GameSettingsValidator()
        {
            RuleFor(x => x.Duration)
                .InclusiveBetween(10, 600)
                .Must(d => d % 10 == 0)
                .WithMessage("La durée d'un round doit etre entre 10 et 600 secondes et etre un multiple de 10.");
        }
    }
}

