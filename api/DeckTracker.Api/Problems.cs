namespace DeckTracker.Api;

public static class Problems
{
    public static IResult BadRequest(string detail) =>
        Results.Problem(title: "Validation failed", detail: detail, statusCode: StatusCodes.Status400BadRequest);

    public static IResult BadRequest(IEnumerable<string> errors) =>
        BadRequest(string.Join(" ", errors));

    public static IResult NotFound(string detail) =>
        Results.Problem(title: "Not found", detail: detail, statusCode: StatusCodes.Status404NotFound);

    public static IResult Conflict(string detail) =>
        Results.Problem(title: "Conflict", detail: detail, statusCode: StatusCodes.Status409Conflict);
}
