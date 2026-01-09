using FluentValidation;
using project.Models.DTO;

namespace project.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(l => l.Username)
            .NotEmpty().WithMessage("Имя пользователя не может быть пустым.");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("Пароль не может быть пустым.");
    }
}
