namespace WellAnalyzing.Application.Dto;

/// <summary>
/// Исходные данные по скважине.
/// </summary>
public sealed class RawWellDataDto
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public string WellId { get; set; } = null!;

    /// <summary>
    /// Позиционирование по X.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// Позиционирование по Y.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// Отметка начальной глубины.
    /// </summary>
    public float DepthFrom { get; set; }

    /// <summary>
    /// Отметка конечной глубины.
    /// </summary>
    public float DepthTo { get; set; }

    /// <summary>
    /// Тип породы.
    /// </summary>
    public string Rock { get; set; } = null!;

    /// <summary>
    /// Пористость.
    /// </summary>
    public float Porosity { get; set; }
}
