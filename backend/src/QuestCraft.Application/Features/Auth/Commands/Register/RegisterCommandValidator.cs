using FluentValidation;

namespace QuestCraft.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("İstifadəçi adı boş ola bilməz.")
            .Length(3, 50).WithMessage("İstifadəçi adı 3-50 simvol aralığında olmalıdır.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("İstifadəçi adı yalnız hərf, rəqəm və alt xətdən ibarət ola bilər.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Ad 100 simvoldan uzun ola bilməz.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad boş ola bilməz.")
            .MaximumLength(100).WithMessage("Soyad 100 simvoldan uzun ola bilməz.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş ola bilməz.")
            .EmailAddress().WithMessage("Email formatı düzgün deyil.")
            .MaximumLength(256).WithMessage("Email 256 simvoldan uzun ola bilməz.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifrə boş ola bilməz.")
            .MinimumLength(8).WithMessage("Şifrə ən azı 8 simvol olmalıdır.")
            .Matches("[A-Z]").WithMessage("Şifrə ən azı bir böyük hərf ehtiva etməlidir.")
            .Matches("[a-z]").WithMessage("Şifrə ən azı bir kiçik hərf ehtiva etməlidir.")
            .Matches("[0-9]").WithMessage("Şifrə ən azı bir rəqəm ehtiva etməlidir.");
    }
}
