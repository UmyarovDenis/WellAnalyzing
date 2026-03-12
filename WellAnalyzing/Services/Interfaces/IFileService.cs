using System.Diagnostics.CodeAnalysis;
using WellAnalyzing.Application.Dto;

namespace WellAnalyzing.Services.Interfaces;

/// <summary>
/// Предоставляет методы для работы с файловой системой.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Пробует получить путь файла .csv для загрузки исходных данных по скважинам.
    /// </summary>
    /// <param name="filePath">Путь до файла с исходными данными.</param>
    /// <returns>Если возвращается <see langword="true"/>, то <paramref name="filePath"/> присваивается значение.</returns>
    bool TryGetCsvFilePath([NotNullWhen(returnValue: true)] out string? filePath);

    /// <summary>
    /// Пробует получить путь файла .json для экспорта сводок по скважинам.
    /// </summary>
    /// <param name="filePath">Путь до файла экспорта сводок по скважинам.</param>
    /// <returns>Если возвращается <see langword="true"/>, то <paramref name="filePath"/> присваивается значение.</returns>
    bool TryGetJsonSavePath([NotNullWhen(returnValue: true)] out string? filePath);

    /// <summary>
    /// Асинхронно перечисляет .csv данные.
    /// </summary>
    /// <param name="filePath">Путь до файла с исходными данными.</param>
    /// <param name="token"><see cref="CancellationToken"/>.</param>
    IAsyncEnumerable<RawWellDataDto> EnumerateCsvRawDataAsync(string filePath, CancellationToken token = default);

    /// <summary>
    /// Экспортирует сводки по скважинам в файл Json.
    /// </summary>
    /// <param name="wellSummaries">Список <see cref="WellSummaryDto"/>.</param>
    /// <param name="filePath">Путь до файла экспорта сводок по скважинам.</param>
    /// <param name="token"><see cref="CancellationToken"/>.</param>
    Task ExportJsonDataAsync(IReadOnlyList<WellSummaryDto> wellSummaries, string filePath, CancellationToken token = default);
}
