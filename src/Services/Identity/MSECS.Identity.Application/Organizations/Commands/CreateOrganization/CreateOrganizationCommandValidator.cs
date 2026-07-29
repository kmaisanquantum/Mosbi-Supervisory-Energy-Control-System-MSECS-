using FluentValidation;
using MSECS.Identity.Domain.Enums;

namespace MSECS.Identity.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<OrganizationType>(t, true, out _))
            .WithMessage("Type must be one of: Installer, AssetOwner, Utility, Platform.");
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
