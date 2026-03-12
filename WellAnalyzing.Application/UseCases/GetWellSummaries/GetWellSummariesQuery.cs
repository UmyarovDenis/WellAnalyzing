using MediatR;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.UseCases.GetWellSummaries;

/// <summary>
/// Запрос для получения сводок по скважинам <see cref="Well"/>.
/// </summary>
/// <param name="WellSummaryDtos">Список <see cref="Well"/>.</param>
public sealed record GetWellSummariesQuery(Well[] WellSummaryDtos) : IRequest<WellSummaryDto[]>;
