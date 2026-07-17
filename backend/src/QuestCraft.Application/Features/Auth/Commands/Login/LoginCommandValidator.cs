using FluentValidation;

namespace QuestCraft.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty().WithMessage("Email və ya istifadəçi adı boş ola bilməz.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifrə boş ola bilməz.");
    }
}
