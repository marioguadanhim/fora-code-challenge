using ErrorOr;
using FluentValidation;
using MediatR;
using System.Reflection;


namespace Fora.Application.Handlers.Base;

public class ValidateRequestPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(result => result.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .Select(failure => Error.Validation(failure.PropertyName, failure.ErrorMessage))
            .ToList();

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];
            MethodInfo? fromMethod = typeof(ErrorOr<>)
                .MakeGenericType(resultType)
                .GetMethod("From", new[] { typeof(List<Error>) });

            if (fromMethod != null)
            {
                return (TResponse)fromMethod.Invoke(null, new object[] { errors })!;
            }
        }

        throw new ValidationException(failures);
    }
}