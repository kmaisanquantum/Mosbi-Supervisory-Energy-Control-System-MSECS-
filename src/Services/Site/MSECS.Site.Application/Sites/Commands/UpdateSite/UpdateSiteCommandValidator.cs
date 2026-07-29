using FluentValidation;

namespace MSECS.Site.Application.Sites.Commands.UpdateSite;

public class UpdateSiteCommandValidator : AbstractValidator<UpdateSiteCommand>
{
    public UpdateSiteCommandValidator()
    {
        RuleFor(x => x.SiteId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InstalledCapacityKw).GreaterThanOrEqualTo(0);
    }
}
