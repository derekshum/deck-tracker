using DeckTracker.Api.Data;
using DeckTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Endpoints;

public static class AnalysisEndpoints
{
    public static void MapAnalysisEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/games/{gameId:int}/analysis", GetGameAnalysis);
    }

    private static async Task<IResult> GetGameAnalysis(int gameId, Models.DeckResult? result, DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var query = db.Decks.Where(d => d.GameId == gameId);
        if (result is not null)
        {
            query = query.Where(d => d.Result == result);
        }

        var decks = await query
            .Include(d => d.Cards).ThenInclude(dc => dc.Card).ThenInclude(c => c!.Type)
            .ToListAsync();

        return Results.Ok(AnalysisService.AnalyzeGame(gameId, decks));
    }
}
