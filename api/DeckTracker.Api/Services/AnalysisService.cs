using DeckTracker.Api.Contracts;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Services;

public static class AnalysisService
{
    public static DeckAnalysis AnalyzeDeck(Models.Deck deck)
    {
        var totalCardCount = deck.Cards.Sum(dc => dc.Quantity);
        var uniqueCardCount = deck.Cards.Count;

        var typeBreakdown = deck.Cards
            .GroupBy(dc => dc.Card?.Type?.Name ?? "Untyped")
            .ToDictionary(g => g.Key, g => g.Sum(dc => dc.Quantity));

        var costed = deck.Cards.Where(dc => dc.Card?.Cost is not null).ToList();
        Dictionary<string, int>? costCurve = null;
        double? averageCost = null;
        if (costed.Count > 0)
        {
            costCurve = costed
                .GroupBy(dc => CostBucket(dc.Card!.Cost!.Value))
                .ToDictionary(g => g.Key, g => g.Sum(dc => dc.Quantity));

            var costedQuantity = costed.Sum(dc => dc.Quantity);
            averageCost = costed.Sum(dc => dc.Card!.Cost!.Value * dc.Quantity) / (double)costedQuantity;
        }

        var mostDuplicated = deck.Cards
            .OrderByDescending(dc => dc.Quantity)
            .Select(dc => new DeckCardEntry(dc.CardId, dc.Quantity))
            .FirstOrDefault();

        return new DeckAnalysis(deck.Id, totalCardCount, uniqueCardCount, typeBreakdown, costCurve, averageCost, mostDuplicated);
    }

    public static AggregateAnalysis AnalyzeGame(int gameId, List<Models.Deck> decks)
    {
        var winRate = WinRate(decks);

        var topCards = decks
            .SelectMany(d => d.Cards.Select(dc => (Deck: d, DeckCard: dc)))
            .GroupBy(x => x.DeckCard.CardId)
            .Select(g => new
            {
                Card = g.First().DeckCard.Card!,
                Count = g.Sum(x => x.DeckCard.Quantity),
                WinRate = WinRate(g.Select(x => x.Deck).ToList()),
            })
            .OrderByDescending(x => x.WinRate)
            .ThenByDescending(x => x.Count)
            .Take(10)
            .Select(x => new CardFrequency(ToCardDto(x.Card), x.Count, x.WinRate))
            .ToList();

        var topPairs = decks
            .SelectMany(d => Pairs(d.Cards.Select(dc => dc.Card!).OrderBy(c => c.Id).ToList())
                .Select(pair => (Deck: d, pair.A, pair.B)))
            .GroupBy(x => (x.A.Id, x.B.Id))
            .Select(g => new
            {
                A = g.First().A,
                B = g.First().B,
                CoOccurrenceCount = g.Count(),
                WinRate = WinRate(g.Select(x => x.Deck).ToList()),
            })
            .OrderByDescending(x => x.CoOccurrenceCount)
            .Take(10)
            .Select(x => new CardPair(ToCardDto(x.A), ToCardDto(x.B), x.CoOccurrenceCount, x.WinRate))
            .ToList();

        return new AggregateAnalysis(gameId, decks.Count, winRate, topCards, topPairs);
    }

    private static string CostBucket(int cost) => cost switch
    {
        <= 0 => "0",
        1 => "1",
        2 => "2",
        _ => "3+",
    };

    private static double WinRate(ICollection<Models.Deck> decks) =>
        decks.Count == 0 ? 0.0 : (double)decks.Count(d => d.Result == Models.DeckResult.Win) / decks.Count;

    private static IEnumerable<(Models.Card A, Models.Card B)> Pairs(List<Models.Card> cards)
    {
        for (var i = 0; i < cards.Count; i++)
        {
            for (var j = i + 1; j < cards.Count; j++)
            {
                yield return (cards[i], cards[j]);
            }
        }
    }

    private static Card ToCardDto(Models.Card card) => new(
        card.Id,
        card.Name,
        card.Description,
        card.Type is null ? null : new CardType(card.Type.Id, card.Type.Name, card.Type.Description),
        card.Cost);
}
