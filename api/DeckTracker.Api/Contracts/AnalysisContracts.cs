namespace DeckTracker.Api.Contracts;

public record CardFrequency(Card Card, int Count, double WinRate);

public record CardPair(Card CardA, Card CardB, int CoOccurrenceCount, double WinRate);

public record DeckAnalysis(
    int DeckId,
    int TotalCardCount,
    int UniqueCardCount,
    Dictionary<string, int> TypeBreakdown,
    Dictionary<string, int>? CostCurve,
    double? AverageCost,    // string key to do make groupings possible 
    DeckCardEntry? MostDuplicated);

public record AggregateAnalysis(
    int GameId,
    int TotalDecks,
    double WinRate,
    List<CardFrequency> TopCards,
    List<CardPair> TopPairs);
