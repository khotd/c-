using FluentValidation;
using project.Models.DTO;

namespace project.Validators;

public class CreateAdoptionDtoValidator : AbstractValidator<CreateAdoptionDto>
{
    public CreateAdoptionDtoValidator()
    {
        RuleFor(a => a.AnimalId)
            .GreaterThan(0).WithMessage("ID животного должен быть больше 0.");

        RuleFor(a => a.UserId)
            .GreaterThan(0).WithMessage("ID пользователя должен быть больше 0.");

        RuleFor(a => a.AdoptionDate)
            .NotEmpty().WithMessage("Дата усыновления обязательна.");
    }
}
