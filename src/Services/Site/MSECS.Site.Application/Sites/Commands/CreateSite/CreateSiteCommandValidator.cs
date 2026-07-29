using FluentValidation;

namespace MSECS.Site.Application.Sites.Commands.CreateSite;

public class CreateSiteCommandValidator : AbstractValidator<CreateSiteCommand>
{
    public CreateSiteCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.WeatherZone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Timezone).NotEmpty().MaximumLength(60);
        RuleFor(x => x.InstalledCapacityKw).GreaterThanOrEqualTo(0);
    }
}
