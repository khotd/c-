using FluentValidation;
using project.Models.DTO;

namespace project.Validators;

public class CreateAnimalDtoValidator : AbstractValidator<CreateAnimalDto>
{
    public CreateAnimalDtoValidator()
    {
        RuleFor(a => a.Name)
            .NotEmpty().WithMessage("Имя животного не может быть пустым.")
            .MaximumLength(100).WithMessage("Имя не может быть длиннее 100 символов.");

        RuleFor(a => a.Species)
            .NotEmpty().WithMessage("Вид животного не может быть пустым.")
            .MaximumLength(50).WithMessage("Вид не может быть длиннее 50 символов.");

        RuleFor(a => a.Breed)
            .NotEmpty().WithMessage("Порода не может быть пустой.")
            .MaximumLength(100).WithMessage("Порода не может быть длиннее 100 символов.");

        RuleFor(a => a.Age)
            .GreaterThanOrEqualTo(0).WithMessage("Возраст не может быть отрицательным.")
            .LessThanOrEqualTo(50).WithMessage("Возраст не может быть больше 50 лет.");

        RuleFor(a => a.Gender)
            .NotEmpty().WithMessage("Пол не может быть пустым.")
            .Must(g => g == "Male" || g == "Female")
            .WithMessage("Пол должен быть Male или Female.");

        RuleFor(a => a.Status)
            .Must(s => s == "Available" || s == "Adopted" || s == "InTreatment" || s == "Deceased")
            .WithMessage("Статус должен быть Available, Adopted, InTreatment или Deceased.");

        RuleFor(a => a.ShelterId)
            .GreaterThan(0).WithMessage("ID приюта должен быть больше 0.");
    }
}
