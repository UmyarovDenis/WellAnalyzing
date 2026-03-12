using FluentValidation;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.Validators;

/// <summary>
/// Валидатор <see cref="Interval"/>.
/// </summary>
public sealed class IntervalValidator : AbstractValidator<Interval>
{
    /// <inheritdoc />
    public IntervalValidator()
    {
        RuleFor(x => x.DepthFrom)
            .GreaterThanOrEqualTo(0.0f)
            .WithMessage("Начальная глубина должна быть не менее нуля")
            .WithState((i, _, ctx) => SetState(i, ctx))
            .Must((interval, val) => val < interval.DepthTo)
            .WithMessage("Начальная глубина должна быть меньше конечной")
            .WithState((i, _, ctx) => SetState(i, ctx));

        RuleFor(x => x.Porosity)
            .InclusiveBetween(0, 1)
            .WithMessage("Пористость должна находиться в диапазоне от 0 до 1")
            .WithState((i, _, ctx) => SetState(i, ctx));

        RuleFor(x => x.RockType)
            .NotNull()
            .WithMessage("Требуется указать тип породы")
            .WithState((i, _, ctx) => SetState(i, ctx));
    }

    private static string SetState(Interval interval, ValidationContext<Interval> ctx)
    {
        var stateMap = (Dictionary<Interval, string>)ctx.RootContextData[ValidationContextDataKeys.RawDataLine];
        return stateMap[interval];
    }
}
