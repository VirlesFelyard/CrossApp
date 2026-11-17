using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveBookValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<SaveBookRequest>().FirstOrDefault();

        if (request is null)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["body"] = ["Тіло запиту обов'язкове."]
            });

        var validator = context.HttpContext.RequestServices
            .GetRequiredService<IValidator<SaveBookRequest>>();
        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}
