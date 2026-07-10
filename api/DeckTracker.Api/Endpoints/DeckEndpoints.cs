using DeckTracker.Api.Contracts;
using DeckTracker.Api.Data;
using DeckTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Endpoints;

public static class DeckEndpoints
{
    public static void MapDeckEndpoints(this IEndpointRouteBuilder app)
    {
        var gameGroup = app.MapGroup("/games/{gameId:int}/decks");
        gameGroup.MapGet("/", GetDecks);
        gameGroup.MapPost("/", CreateDeck);

        var deckGroup = app.MapGroup("/decks");
        deckGroup.MapGet("/{deckId:int}", GetDeck);
        deckGroup.MapPut("/{deckId:int}", UpdateDeck);
        deckGroup.MapDelete("/{deckId:int}", DeleteDeck);
        deckGroup.MapGet("/{deckId:int}/analysis", GetDeckAnalysis);
    }

    private static async Task<IResult> GetDecks(
        int gameId,
        Models.DeckResult? result,
        int? page,
        int? pageSize,
        DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var effectivePage = page is > 0 ? page.Value : 1;
        var effectivePageSize = pageSize switch
        {
            null or <= 0 => 20,
            > 100 => 100,
            _ => pageSize.Value,
        };

        var query = db.Decks.Where(d => d.GameId == gameId);
        if (result is not null)
        {
            query = query.Where(d => d.Result == result);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.Date)
            .Skip((effectivePage - 1) * effectivePageSize)
            .Take(effectivePageSize)
            .Select(d => new DeckSummary(d.Id, d.GameId, d.Game!.Name, d.Result, d.Character, d.Date, d.Cards.Sum(c => c.Quantity)))
            .ToListAsync();

        return Results.Ok(new PagedDecks(items, total, effectivePage, effectivePageSize));
    }

    private static async Task<IResult> CreateDeck(int gameId, DeckRequest request, DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var errors = await ValidateDeckRequest(request, gameId, db);
        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var deck = new Models.Deck
        {
            GameId = gameId,
            Result = request.Result!.Value,
            Character = request.Character,
            Notes = request.Notes,
            Cards = MergeCardEntries(request.Cards),
        };
        db.Decks.Add(deck);
        await db.SaveChangesAsync();

        var dto = await LoadDeckDetail(db, deck.Id);
        return Results.Created($"/decks/{deck.Id}", dto);
    }

    private static async Task<IResult> GetDeck(int deckId, DeckTrackerDbContext db)
    {
        var dto = await LoadDeckDetail(db, deckId);
        if (dto is null)
        {
            return Problems.NotFound($"Deck {deckId} not found.");
        }

        return Results.Ok(dto);
    }

    private static async Task<IResult> UpdateDeck(int deckId, DeckRequest request, DeckTrackerDbContext db)
    {
        var deck = await db.Decks.Include(d => d.Cards).FirstOrDefaultAsync(d => d.Id == deckId);
        if (deck is null)
        {
            return Problems.NotFound($"Deck {deckId} not found.");
        }

        var errors = await ValidateDeckRequest(request, deck.GameId, db);
        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        deck.Result = request.Result!.Value;
        deck.Character = request.Character;
        deck.Notes = request.Notes;
        deck.Cards.Clear();
        deck.Cards.UnionWith(MergeCardEntries(request.Cards));
        await db.SaveChangesAsync();

        var dto = await LoadDeckDetail(db, deckId);
        return Results.Ok(dto);
    }

    private static async Task<IResult> DeleteDeck(int deckId, DeckTrackerDbContext db)
    {
        var deck = await db.Decks.FindAsync(deckId);
        if (deck is null)
        {
            return Problems.NotFound($"Deck {deckId} not found.");
        }

        db.Decks.Remove(deck);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetDeckAnalysis(int deckId, DeckTrackerDbContext db)
    {
        var deck = await db.Decks
            .Include(d => d.Cards).ThenInclude(dc => dc.Card).ThenInclude(c => c!.Type)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck is null)
        {
            return Problems.NotFound($"Deck {deckId} not found.");
        }

        return Results.Ok(AnalysisService.AnalyzeDeck(deck));
    }

    private static async Task<List<string>> ValidateDeckRequest(DeckRequest request, int gameId, DeckTrackerDbContext db)
    {
        var errors = Validation.Validate(request);
        foreach (var entry in request.Cards)
        {
            errors.AddRange(Validation.Validate(entry));
        }

        var cardIds = request.Cards.Select(c => c.CardId).Distinct().ToList();
        if (cardIds.Count > 0)
        {
            var validCount = await db.Cards.CountAsync(c => c.GameId == gameId && cardIds.Contains(c.Id));
            if (validCount != cardIds.Count)
            {
                errors.Add("cards must reference cards that belong to this game.");
            }
        }

        return errors;
    }

    private static HashSet<Models.DeckCard> MergeCardEntries(List<DeckCardEntry> entries) => entries
        .GroupBy(e => e.CardId)
        .Select(g => new Models.DeckCard { CardId = g.Key, Quantity = g.Sum(e => e.Quantity) })
        .ToHashSet();

    private static async Task<DeckDetail?> LoadDeckDetail(DeckTrackerDbContext db, int deckId)
    {
        var deck = await db.Decks.Include(d => d.Game).Include(d => d.Cards).FirstOrDefaultAsync(d => d.Id == deckId);
        if (deck is null)
        {
            return null;
        }

        return new DeckDetail(
            deck.Id,
            deck.GameId,
            deck.Game!.Name,
            deck.Result,
            deck.Character,
            deck.Date,
            deck.Cards.Sum(c => c.Quantity),
            deck.Notes,
            deck.Cards.Select(c => new DeckCardEntry(c.CardId, c.Quantity)).ToList());
    }
}
