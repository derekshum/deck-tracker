using System.ComponentModel.DataAnnotations;

namespace DeckTracker.Api.Contracts;

public record Card(int Id, string Name, string? Description, CardType? Type, int? Cost);

public record CardRequest
{
    [Required, MinLength(1), MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    public int? TypeId { get; init; }

    [Range(0, int.MaxValue)]
    public int? Cost { get; init; }
}
