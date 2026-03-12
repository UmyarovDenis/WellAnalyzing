using FluentValidation;
using FluentValidation.Results;
using MoreLinq;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.Validators;

/// <summary>
/// Валидатор <see cref="Well"/>.
/// </summary>
public sealed class WellValidator : AbstractValidator<Well>
{
    private const int WINDOW_SIZE = 2;

    /// <inheritdoc />
    public WellValidator()
    {
        RuleFor(x => x.Intervals)
            .Must((_, intervals, ctx) => NotIntersects(intervals, ctx));

        RuleForEach(x => x.Intervals)
            .SetValidator(new IntervalValidator());
    }

    private static bool NotIntersects(IReadOnlyList<Interval> intervals, ValidationContext<Well> ctx)
    {
        var notIntersects = true;

        foreach (var window in intervals.Window(WINDOW_SIZE))
        {
            if (window.Count == WINDOW_SIZE && window[0].DepthTo > window[1].DepthFrom)
            {
                var failure = new ValidationFailure(nameof(ctx.InstanceToValidate.Intervals), "Интервалы не должны пересекаться")
                {
                    CustomState = SetState(window[0], ctx),
                };

                ctx.AddFailure(failure);
                notIntersects = false;
            }
        }

        return notIntersects;
    }

    private static string SetState(Interval interval, ValidationContext<Well> ctx)
    {
        var stateMap = (Dictionary<Interval, string>)ctx.RootContextData[ValidationContextDataKeys.RawDataLine];
        return stateMap[interval];
    }
}
