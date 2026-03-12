using WellAnalyzing.Application.Dto;

namespace WellAnalyzing.Application.Services.Interfaces;

/// <summary>
/// Сервис, предоставляющий методы для составления отчёта по скважинам.
/// </summary>
public interface IWellReportSummaryService
{
    /// <summary>
    /// Составляет отчёт по скважинам.
    /// </summary>
    /// <param name="rawData">Исходные данные по скважинам IReadOnlyList(<see cref="RawWellDataDto"/>).</param>
    /// <returns></returns>
    Task<WellReportDto> CreateWellReportAsync(IReadOnlyList<RawWellDataDto> rawData);
}
