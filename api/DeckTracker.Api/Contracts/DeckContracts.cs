using System.ComponentModel.DataAnnotations;
using DeckTracker.Api.Models;

namespace DeckTracker.Api.Contracts;

public record DeckCardEntry(int CardId, [property: Range(1, int.MaxValue)] int Quantity);

public record DeckSummary(
    int Id,
    int GameId,
    string GameName,
    DeckResult Result,
    string? Character,
    DateTime Date,
    int CardCount);

public record DeckRequest
{
    [Required]
    public DeckResult? Result { get; init; }

    public string? Character { get; init; }

    public string? Notes { get; init; }

    public List<DeckCardEntry> Cards { get; init; } = [];
}

public record DeckDetail(
    int Id,
    int GameId,
    string GameName,
    DeckResult Result,
    string? Character,
    DateTime Date,
    int CardCount,
    string? Notes,
    List<DeckCardEntry> Cards);

public record PagedDecks(List<DeckSummary> Items, int Total, int Page, int PageSize);
