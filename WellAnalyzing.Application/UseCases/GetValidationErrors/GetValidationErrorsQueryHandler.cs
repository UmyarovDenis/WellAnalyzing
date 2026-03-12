using FluentValidation;
using MediatR;
using WellAnalyzing.Application.Validators;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.UseCases.GetValidationErrors;

/// <summary>
/// Обработчик запроса <see cref="GetValidationErrorsQuery"/>.
/// </summary>
public sealed class GetValidationErrorsQueryHandler : IRequestHandler<GetValidationErrorsQuery, string[]?>
{
    private readonly IValidator<Well> _validator;

    public GetValidationErrorsQueryHandler(IValidator<Well> validator)
    {
        _validator = validator;
    }

    /// <inheritdoc />
    public async Task<string[]?> Handle(GetValidationErrorsQuery request, CancellationToken cancellationToken)
    {
        int startLine = 2;
        var errors = new List<string[]>();

        foreach (var well in request.Wells)
        {
            var validationContext = CreateValidationContext(well, ref startLine);
            var validationResult = await _validator.ValidateAsync(validationContext, cancellationToken);

            var failures =
                validationResult
                    .Errors
                    .Where(e => e != null && e.CustomState != null)
                    .Select(e => $"{e.CustomState}: {e.ErrorMessage}");

            if (failures.Any())
            {
                errors.Add([.. failures]);
            }
        }

        if (errors.Count > 0)
        {
            return [.. errors.SelectMany(e => e)];
        }

        return null;
    }

    private static ValidationContext<Well> CreateValidationContext(Well well, ref int startLine)
    {
        var validationContext = new ValidationContext<Well>(well);
        var intervalData = new Dictionary<Interval, string>();

        for (var i = 0; i < well.Intervals.Count; i++, startLine++)
        {
            intervalData[well.Intervals[i]] = $"[{startLine}] [{well.Id}]";
        }

        validationContext.RootContextData[ValidationContextDataKeys.RawDataLine] = intervalData;

        return validationContext;
    }
}
