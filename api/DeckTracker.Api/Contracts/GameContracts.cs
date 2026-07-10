using System.ComponentModel.DataAnnotations;

namespace DeckTracker.Api.Contracts;

public record GameSummary(int Id, string Name, string? Description, int TotalDecks, double WinRate);

public record GameRequest
{
    [Required, MinLength(1), MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}

public record GameDetail(
    int Id,
    string Name,
    string? Description,
    int TotalDecks,
    double WinRate,
    List<CardType> CardTypes,
    List<Card> Cards);
