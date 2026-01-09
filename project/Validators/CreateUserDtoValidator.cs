using FluentValidation;
using project.Models.DTO;

namespace project.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Имя пользователя не может быть пустым.")
            .MaximumLength(100).WithMessage("Имя пользователя не может быть длиннее 100 символов.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email не может быть пустым.")
            .EmailAddress().WithMessage("Email некорректен.")
            .MaximumLength(255).WithMessage("Email не может быть длиннее 255 символов.");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Пароль не может быть пустым.")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов.");

        RuleFor(u => u.Role)
            .Must(r => r == "Admin" || r == "Manager" || r == "User")
            .WithMessage("Роль должна быть Admin, Manager или User.");
    }
}
