namespace DeckTracker.Api.Models;

public class CardType
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public HashSet<Card> Cards { get; set; } = [];
}
