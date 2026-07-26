using FluentValidation;

namespace HermesZoneTorba.Application.HermesManagement.Commands.InstallHermes;

public sealed class InstallHermesCommandValidator : AbstractValidator<InstallHermesCommand>
{
    public InstallHermesCommandValidator()
    {
        RuleFor(c => c.InstallPath)
            .NotEmpty()
            .WithMessage("An install path is required.");

        RuleFor(c => c.RequestedVersion)
            .Matches(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$")
            .When(c => !string.IsNullOrWhiteSpace(c.RequestedVersion))
            .WithMessage("RequestedVersion must be a valid semantic version, e.g. '1.4.0'.");
    }
}
