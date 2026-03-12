using WellAnalyzing.Domain.Enums;

namespace WellAnalyzing.Domain.Entities;

/// <summary>
/// Сущность интервала бурения.
/// </summary>
/// <param name="DepthFrom">Начальная отметка, м.</param>
/// <param name="DepthTo">Конечная отметка, м.</param>
/// <param name="RockType">Тип породы.</param>
/// <param name="Porosity">Пористость породы.</param>
public sealed record Interval(
    float DepthFrom,
    float DepthTo,
    RockType? RockType,
    float Porosity)
{
    private readonly int _hashCode = Guid.NewGuid().GetHashCode();

    /// <inheritdoc />
    public override int GetHashCode() => _hashCode;
}
