namespace WellAnalyzing.Application.Dto;

/// <summary>
/// Отчёт по скважинам.
/// </summary>
/// <param name="WellSummaries">Сводки по каждой скважине.</param>
/// <param name="ValidationErrors">Ошибки валидации.</param>
public sealed record WellReportDto(
    IReadOnlyList<WellSummaryDto> WellSummaries,
    string[]? ValidationErrors);
