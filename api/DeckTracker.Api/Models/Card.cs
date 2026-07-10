namespace DeckTracker.Api.Models;

public class Card
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? TypeId { get; set; }
    public CardType? Type { get; set; }
    public int? Cost { get; set; }

    public HashSet<DeckCard> DeckCards { get; set; } = [];
}
