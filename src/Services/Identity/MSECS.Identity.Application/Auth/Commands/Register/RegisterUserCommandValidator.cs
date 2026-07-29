using FluentValidation;
using MSECS.Identity.Domain.Enums;

namespace MSECS.Identity.Application.Auth.Commands.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.OrganizationName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.OrganizationType)
            .NotEmpty()
            .Must(t => Enum.TryParse<OrganizationType>(t, true, out _))
            .WithMessage("OrganizationType must be one of: Installer, AssetOwner, Utility, Platform.");

        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(10)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
