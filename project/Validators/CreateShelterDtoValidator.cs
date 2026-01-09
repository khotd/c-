using FluentValidation;
using project.Models.DTO;

namespace project.Validators;

public class CreateShelterDtoValidator : AbstractValidator<CreateShelterDto>
{
    public CreateShelterDtoValidator()
    {
        RuleFor(s => s.Name)
            .NotEmpty().WithMessage("Название приюта не может быть пустым.")
            .MaximumLength(200).WithMessage("Название не может быть длиннее 200 символов.");

        RuleFor(s => s.Address)
            .NotEmpty().WithMessage("Адрес не может быть пустым.")
            .MaximumLength(500).WithMessage("Адрес не может быть длиннее 500 символов.");

        RuleFor(s => s.Capacity)
            .GreaterThan(0).WithMessage("Вместимость должна быть больше 0.");

        RuleFor(s => s.Email)
            .EmailAddress().WithMessage("Email некорректен.")
            .When(s => !string.IsNullOrEmpty(s.Email));

        RuleFor(s => s.Phone)
            .MaximumLength(20).WithMessage("Телефон не может быть длиннее 20 символов.")
            .When(s => !string.IsNullOrEmpty(s.Phone));
    }
}
