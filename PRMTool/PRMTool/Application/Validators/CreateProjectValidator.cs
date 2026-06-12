using Application.DTOs.Project;
using FluentValidation;

namespace Application.Validators;

public class CreateProjectValidator
    : AbstractValidator<CreateProjectRequestDto>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty()
            .WithMessage("Project name is required.")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0)
            .WithMessage("ManagerId must be greater than 0.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
    }
}
