using System.Numerics;

namespace WellAnalyzing.Domain.Entities;

/// <summary>
/// Сущность скважины.
/// </summary>
/// <param name="Id">Идентификатор.</param>
/// <param name="Position">Позиционирование.</param>
/// <param name="Intervals">Интервалы.</param>
public sealed record Well(
    string Id,
    Vector2 Position,
    IReadOnlyList<Interval> Intervals);
