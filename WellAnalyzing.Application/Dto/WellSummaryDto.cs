using System.Numerics;

namespace WellAnalyzing.Application.Dto;

/// <summary>
/// Сводка по скважине.
/// </summary>
/// <param name="WellId">Идентификатор.</param>
/// <param name="Position">Позиционирование.</param>
/// <param name="TotalDepth">Общая глубина, м.</param>
/// <param name="IntervalCount">Количество интервалов.</param>
/// <param name="PorosityAverage">Средняя пористость.</param>
/// <param name="Rock">Наиболее встречающаяся порода.</param>
public sealed record WellSummaryDto(
    string WellId,
    Vector2 Position,
    float TotalDepth,
    int IntervalCount,
    float PorosityAverage,
    string Rock);
