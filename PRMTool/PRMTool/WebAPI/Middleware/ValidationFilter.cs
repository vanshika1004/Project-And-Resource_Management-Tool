using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.Middleware;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value == null)
                continue;

            var argumentType = argument.Value.GetType();

            var validatorType =
                typeof(IValidator<>).MakeGenericType(argumentType);

            var validator = _serviceProvider
                .GetService(validatorType) as IValidator;

            if (validator == null)
                continue;

            var validationContext =
                new ValidationContext<object>(argument.Value);

            var result =
                await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var response = new
                {
                    success = false,
                    message = "Validation failed",
                    errors
                };

                context.Result =
                    new BadRequestObjectResult(response);

                return;
            }
        }

        await next();
    }
}
