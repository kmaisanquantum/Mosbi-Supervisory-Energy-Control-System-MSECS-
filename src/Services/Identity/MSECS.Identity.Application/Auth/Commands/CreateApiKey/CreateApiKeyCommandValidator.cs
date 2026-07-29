using FluentValidation;

namespace MSECS.Identity.Application.Auth.Commands.CreateApiKey;

public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    public CreateApiKeyCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ExpiresInDays).GreaterThan(0).When(x => x.ExpiresInDays.HasValue);
    }
}
