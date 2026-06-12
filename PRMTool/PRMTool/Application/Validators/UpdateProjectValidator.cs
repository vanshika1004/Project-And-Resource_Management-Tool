using Application.DTOs.Project;
using FluentValidation;

namespace Application.Validators;

public class UpdateProjectValidator
    : AbstractValidator<UpdateProjectRequestDto>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.ProjectName)
            .NotEmpty()
            .WithMessage("Project name is required.")
            .MaximumLength(200)
            .WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid project status.");

        RuleFor(x => x.HealthStatus)
            .IsInEnum()
            .WithMessage("Invalid health status.");
    }
}
