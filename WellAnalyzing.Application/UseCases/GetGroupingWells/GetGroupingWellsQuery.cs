using MediatR;
using WellAnalyzing.Application.Dto;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.UseCases.GetGroupingWells;

/// <summary>
/// Запрос для группировки <see cref="RawWellDataDto"/> в <see cref="Well"/>.
/// </summary>
/// <param name="RawWells">Список <see cref="RawWellDataDto"/>.</param>
public sealed record GetGroupingWellsQuery(IReadOnlyList<RawWellDataDto> RawWells) : IRequest<Well[]>;
