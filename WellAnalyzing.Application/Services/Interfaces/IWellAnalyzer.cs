using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.Services.Interfaces;

/// <summary>
/// Сервис, предоставляющий методы для анализа скважины.
/// </summary>
public interface IWellAnalyzer
{
    /// <summary>
    /// Выполняет расчёт общей глубины скважины.
    /// </summary>
    /// <param name="well"><see cref="Well"/>.</param>
    /// <returns>Общая глубина скважины, м.</returns>
    float CalculateTotalDepth(Well well);

    /// <summary>
    /// Выполняет расчёт колличества интервалов скважины.
    /// </summary>
    /// <param name="well"><see cref="Well"/>.</param>
    /// <returns>Количество интервалов скважины.</returns>
    int CalculateIntervalCount(Well well);

    /// <summary>
    /// Выполняет расчёт средней пористости по толщине (взвешенная по длине интервалов).
    /// </summary>
    /// <param name="well"><see cref="Well"/>.</param>
    /// <returns>Средняя пористость по толщине скважины.</returns>
    float CalculatePorosityAverage(Well well);

    /// <summary>
    /// Определяет самую распространённую породу по суммарной толщине.
    /// </summary>
    /// <param name="well"><see cref="Well"/>.</param>
    /// <returns>Самая распространённая порода по толщине.</returns>
    string DetermineRock(Well well);
}
