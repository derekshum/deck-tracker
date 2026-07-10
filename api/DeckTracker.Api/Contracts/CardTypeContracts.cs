using System.ComponentModel.DataAnnotations;

namespace DeckTracker.Api.Contracts;

public record CardType(int Id, string Name, string? Description);

public record CardTypeRequest
{
    [Required, MinLength(1), MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
