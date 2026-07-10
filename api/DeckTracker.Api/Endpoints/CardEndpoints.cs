using DeckTracker.Api.Contracts;
using DeckTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Endpoints;

public static class CardEndpoints
{
    public static void MapCardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/games/{gameId:int}/cards");

        group.MapGet("/", GetCards);
        group.MapPost("/", CreateCard);
        group.MapGet("/{cardId:int}", GetCard);
        group.MapPut("/{cardId:int}", UpdateCard);
        group.MapDelete("/{cardId:int}", DeleteCard);
    }

    private static async Task<IResult> GetCards(int gameId, DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var cards = await db.Cards
            .Where(c => c.GameId == gameId)
            .Include(c => c.Type)
            .ToListAsync();

        return Results.Ok(cards.Select(GameEndpoints.ToCardDto).ToList());
    }

    private static async Task<IResult> CreateCard(int gameId, CardRequest request, DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var errors = Validation.Validate(request);
        if (request.TypeId is not null && !await db.CardTypes.AnyAsync(ct => ct.Id == request.TypeId && ct.GameId == gameId))
        {
            errors.Add("typeId must reference a card type belonging to this game.");
        }

        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var card = new Models.Card
        {
            GameId = gameId,
            Name = request.Name.Trim(),
            Description = request.Description,
            TypeId = request.TypeId,
            Cost = request.Cost,
        };
        db.Cards.Add(card);
        await db.SaveChangesAsync();

        var dto = await LoadCardDto(db, gameId, card.Id);
        return Results.Created($"/games/{gameId}/cards/{card.Id}", dto);
    }

    private static async Task<IResult> GetCard(int gameId, int cardId, DeckTrackerDbContext db)
    {
        var dto = await LoadCardDto(db, gameId, cardId);
        if (dto is null)
        {
            return Problems.NotFound($"Card {cardId} not found in game {gameId}.");
        }

        return Results.Ok(dto);
    }

    private static async Task<IResult> UpdateCard(int gameId, int cardId, CardRequest request, DeckTrackerDbContext db)
    {
        var errors = Validation.Validate(request);
        if (request.TypeId is not null && !await db.CardTypes.AnyAsync(ct => ct.Id == request.TypeId && ct.GameId == gameId))
        {
            errors.Add("typeId must reference a card type belonging to this game.");
        }

        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId && c.GameId == gameId);
        if (card is null)
        {
            return Problems.NotFound($"Card {cardId} not found in game {gameId}.");
        }

        card.Name = request.Name.Trim();
        card.Description = request.Description;
        card.TypeId = request.TypeId;
        card.Cost = request.Cost;
        await db.SaveChangesAsync();

        var dto = await LoadCardDto(db, gameId, cardId);
        return Results.Ok(dto);
    }

    private static async Task<IResult> DeleteCard(int gameId, int cardId, DeckTrackerDbContext db)
    {
        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId && c.GameId == gameId);
        if (card is null)
        {
            return Problems.NotFound($"Card {cardId} not found in game {gameId}.");
        }

        var inUse = await db.DeckCards.AnyAsync(dc => dc.CardId == cardId);
        if (inUse)
        {
            return Problems.Conflict("Cannot delete a card that appears in a logged deck.");
        }

        db.Cards.Remove(card);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<Card?> LoadCardDto(DeckTrackerDbContext db, int gameId, int cardId)
    {
        var card = await db.Cards.Include(c => c.Type).FirstOrDefaultAsync(c => c.Id == cardId && c.GameId == gameId);
        return card is null ? null : GameEndpoints.ToCardDto(card);
    }
}
