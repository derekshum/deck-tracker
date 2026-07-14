using DeckTracker.Api.Contracts;
using DeckTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using Models = DeckTracker.Api.Models;

namespace DeckTracker.Api.Endpoints;

public static class CardTypeEndpoints
{
    public static void MapCardTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var gameGroup = app.MapGroup("/games/{gameId:int}/card-types");
        gameGroup.MapGet("/", GetCardTypes);
        gameGroup.MapPost("/", CreateCardType);

        var typeGroup = app.MapGroup("/card-types");
        typeGroup.MapGet("/{cardTypeId:int}", GetCardType);
        typeGroup.MapPut("/{cardTypeId:int}", UpdateCardType);
        typeGroup.MapDelete("/{cardTypeId:int}", DeleteCardType);
    }

    private static async Task<IResult> GetCardTypes(int gameId, DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var types = await db.CardTypes
            .Where(ct => ct.GameId == gameId)
            .Select(ct => new CardType(ct.Id, ct.Name, ct.Description))
            .ToListAsync();

        return Results.Ok(types);
    }

    private static async Task<IResult> CreateCardType(int gameId, CardTypeRequest request, DeckTrackerDbContext db)
    {
        if (!await db.Games.AnyAsync(g => g.Id == gameId))
        {
            return Problems.NotFound($"Game {gameId} not found.");
        }

        var errors = Validation.Validate(request);
        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var cardType = new Models.CardType { GameId = gameId, Name = request.Name.Trim(), Description = request.Description };
        db.CardTypes.Add(cardType);
        await db.SaveChangesAsync();

        var dto = new CardType(cardType.Id, cardType.Name, cardType.Description);
        return Results.Created($"/card-types/{cardType.Id}", dto);
    }

    private static async Task<IResult> GetCardType(int cardTypeId, DeckTrackerDbContext db)
    {
        var cardType = await db.CardTypes.FirstOrDefaultAsync(ct => ct.Id == cardTypeId);
        if (cardType is null)
        {
            return Problems.NotFound($"Card type {cardTypeId} not found.");
        }

        return Results.Ok(new CardType(cardType.Id, cardType.Name, cardType.Description));
    }

    private static async Task<IResult> UpdateCardType(int cardTypeId, CardTypeRequest request, DeckTrackerDbContext db)
    {
        var errors = Validation.Validate(request);
        if (errors.Count > 0)
        {
            return Problems.BadRequest(errors);
        }

        var cardType = await db.CardTypes.FirstOrDefaultAsync(ct => ct.Id == cardTypeId);
        if (cardType is null)
        {
            return Problems.NotFound($"Card type {cardTypeId} not found.");
        }

        cardType.Name = request.Name.Trim();
        cardType.Description = request.Description;
        await db.SaveChangesAsync();

        return Results.Ok(new CardType(cardType.Id, cardType.Name, cardType.Description));
    }

    private static async Task<IResult> DeleteCardType(int cardTypeId, DeckTrackerDbContext db)
    {
        var cardType = await db.CardTypes.FirstOrDefaultAsync(ct => ct.Id == cardTypeId);
        if (cardType is null)
        {
            return Problems.NotFound($"Card type {cardTypeId} not found.");
        }

        var inUse = await db.Cards.AnyAsync(c => c.TypeId == cardTypeId);
        if (inUse)
        {
            return Problems.Conflict("Cannot delete a card type that is still assigned to cards.");
        }

        db.CardTypes.Remove(cardType);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
