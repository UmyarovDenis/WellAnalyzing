using MediatR;
using WellAnalyzing.Domain.Entities;

namespace WellAnalyzing.Application.UseCases.GetValidationErrors;

/// <summary>
/// Запрос для получения сообщений ошибок валидации <see cref="Well"/>.
/// </summary>
/// <param name="Wells">Список <see cref="Well"/>.</param>
public sealed record GetValidationErrorsQuery(Well[] Wells) : IRequest<string[]?>;
