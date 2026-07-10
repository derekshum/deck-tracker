namespace DeckTracker.Api.Models;

public class Deck
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game? Game { get; set; }
    public DeckResult Result { get; set; }
    public string? Character { get; set; }
    public string? Notes { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public HashSet<DeckCard> Cards { get; set; } = [];
}
