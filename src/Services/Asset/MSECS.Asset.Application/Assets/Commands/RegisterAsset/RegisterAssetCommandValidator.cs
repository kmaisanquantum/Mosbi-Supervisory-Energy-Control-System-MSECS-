using FluentValidation;
using MSECS.Asset.Domain.Enums;

namespace MSECS.Asset.Application.Assets.Commands.RegisterAsset;

public class RegisterAssetCommandValidator : AbstractValidator<RegisterAssetCommand>
{
    public RegisterAssetCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Type).NotEmpty().Must(t => Enum.TryParse<AssetType>(t, true, out _))
            .WithMessage("Type must be one of: SolarArray, Panel, Inverter, Battery, Meter, WeatherStation, Controller.");
        RuleFor(x => x.Manufacturer).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(150);
        RuleFor(x => x.RatedCapacityKw).GreaterThanOrEqualTo(0).When(x => x.RatedCapacityKw.HasValue);
    }
}
