namespace DeckTracker.Api.Models;

public class Game
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public HashSet<CardType> CardTypes { get; set; } = [];
    public HashSet<Card> Cards { get; set; } = [];
    public HashSet<Deck> Decks { get; set; } = [];
}
