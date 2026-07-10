using System.Linq.Expressions;
using DeckTracker.Api.Contracts;
using DeckTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Endpoints;

public static class GameEndpoints
{
    private static readonly Expression<Func<Models.Game, GameSummary>> ToSummary = g => new GameSummary(
        g.Id,
        g.Name,
        g.Description,
        g.Decks.Count,
        g.Decks.Count == 0 ? 0.0 : (double)g.Decks.Count(d => d.Result == Models.DeckResult.Win) / g.Decks.Count);

    public static void MapGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games");

        group.MapGet("/", GetGames);
        group.MapPost("/", CreateGame);
        group.MapGet("/{gameId:int}", GetGame);
        group.MapPut("/{gameId:int}", UpdateGame);
        group.MapDelete("/{gameId:int}", DeleteGame);
    }

    private static async Task<IResult> GetGames(DeckTrackerDbContext db)
    {
        var games = await db.Games.Select(ToSummary).ToListAsync();
        return Results.Ok(games);
    }

    private static async Task<IResult> CreateGame(GameRequest request, DeckTrackerDbContext db)
    {
        var errors = Validation.Validate(request);
        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var game = new Models.Game { Name = request.Name.Trim(), Description = request.Description };
        db.Games.Add(game);
        await db.SaveChangesAsync();

        var summary = await db.Games.Where(g => g.Id == game.Id).Select(ToSummary).FirstAsync();
        return Results.Created($"/games/{game.Id}", summary);
    }

    private static async Task<IResult> GetGame(int gameId, DeckTrackerDbContext db)
    {
        var game = await db.Games
            .Include(g => g.CardTypes)
            .Include(g => g.Cards).ThenInclude(c => c.Type)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null)
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var totalDecks = await db.Decks.CountAsync(d => d.GameId == gameId);
        var wins = await db.Decks.CountAsync(d => d.GameId == gameId && d.Result == Models.DeckResult.Win);
        var winRate = totalDecks == 0 ? 0.0 : (double)wins / totalDecks;

        var detail = new GameDetail(
            game.Id,
            game.Name,
            game.Description,
            totalDecks,
            winRate,
            game.CardTypes.Select(ct => new CardType(ct.Id, ct.Name, ct.Description)).ToList(),
            game.Cards.Select(ToCardDto).ToList());

        return Results.Ok(detail);
    }

    private static async Task<IResult> UpdateGame(int gameId, GameRequest request, DeckTrackerDbContext db)
    {
        var errors = Validation.Validate(request);
        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var game = await db.Games.FindAsync(gameId);
        if (game is null)
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        game.Name = request.Name.Trim();
        game.Description = request.Description;
        await db.SaveChangesAsync();

        var summary = await db.Games.Where(g => g.Id == gameId).Select(ToSummary).FirstAsync();
        return Results.Ok(summary);
    }

    private static async Task<IResult> DeleteGame(int gameId, DeckTrackerDbContext db)
    {
        var game = await db.Games.FindAsync(gameId);
        if (game is null)
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var hasDecks = await db.Decks.AnyAsync(d => d.GameId == gameId);
        if (hasDecks)
        {
            return Problems.Conflict("Cannot delete a game that has logged decks. Delete its decks first.");
        }

        db.Games.Remove(game);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    internal static Card ToCardDto(Models.Card card) => new(
        card.Id,
        card.Name,
        card.Description,
        card.Type is null ? null : new CardType(card.Type.Id, card.Type.Name, card.Type.Description),
        card.Cost);
}
