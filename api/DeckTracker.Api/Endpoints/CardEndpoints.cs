using DeckTracker.Api.Contracts;
using DeckTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Endpoints;

public static class CardEndpoints
{
    public static void MapCardEndpoints(this IEndpointRouteBuilder app)
    {
        var gameGroup = app.MapGroup("/games/{gameId:int}/cards");
        gameGroup.MapGet("/", GetCards);
        gameGroup.MapPost("/", CreateCard);

        var cardGroup = app.MapGroup("/cards");
        cardGroup.MapGet("/{cardId:int}", GetCard);
        cardGroup.MapPut("/{cardId:int}", UpdateCard);
        cardGroup.MapDelete("/{cardId:int}", DeleteCard);
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

        var dto = await LoadCardDto(db, card.Id);
        return Results.Created($"/cards/{card.Id}", dto);
    }

    private static async Task<IResult> GetCard(int cardId, DeckTrackerDbContext db)
    {
        var dto = await LoadCardDto(db, cardId);
        if (dto is null)
        {
            return Problems.NotFound($"Card {cardId} not found.");
        }

        return Results.Ok(dto);
    }

    private static async Task<IResult> UpdateCard(int cardId, CardRequest request, DeckTrackerDbContext db)
    {
        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
        if (card is null)
        {
            return Problems.NotFound($"Card {cardId} not found.");
        }

        var errors = Validation.Validate(request);
        if (request.TypeId is not null && !await db.CardTypes.AnyAsync(ct => ct.Id == request.TypeId && ct.GameId == card.GameId))
        {
            errors.Add("typeId must reference a card type belonging to this game.");
        }

        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        card.Name = request.Name.Trim();
        card.Description = request.Description;
        card.TypeId = request.TypeId;
        card.Cost = request.Cost;
        await db.SaveChangesAsync();

        var dto = await LoadCardDto(db, cardId);
        return Results.Ok(dto);
    }

    private static async Task<IResult> DeleteCard(int cardId, DeckTrackerDbContext db)
    {
        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
        if (card is null)
        {
            return Problems.NotFound($"Card {cardId} not found.");
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

    private static async Task<Card?> LoadCardDto(DeckTrackerDbContext db, int cardId)
    {
        var card = await db.Cards.Include(c => c.Type).FirstOrDefaultAsync(c => c.Id == cardId);
        return card is null ? null : GameEndpoints.ToCardDto(card);
    }
}
