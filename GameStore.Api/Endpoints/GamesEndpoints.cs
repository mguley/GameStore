using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Entities;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string getGameEndpointName = "GetGame";

    public static RouteGroupBuilder MapGamesEndpoints(this WebApplication application)
    {
        var group = application.MapGroup("games")
            .WithParameterValidation();
        
        // GET /games
        group.MapGet(pattern: "/", handler: (GameStoreContext dbContext) => 
            dbContext.Games
                .Include(game => game.Genre)
                .Select(game => game.ToGameSummaryDto())
                .AsNoTracking());

        // GET /games/1
        group.MapGet(pattern: "/{id}", handler: (int id, GameStoreContext dbContext) =>
            {
                Game? game = dbContext.Games.Find(id);

                return game is null ? 
                    Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
            })
            .WithName(endpointName: getGameEndpointName);

        // POST /games
        group.MapPost(pattern: "/", handler: (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            dbContext.SaveChanges();
            
            return Results.CreatedAtRoute(routeName: getGameEndpointName, routeValues: new { id = game.Id },
                value: game.ToGameDetailsDto());
        }).WithParameterValidation();

        // PUT /games
        group.MapPut(pattern: "/{id}", handler: (int id, UpdateGameDto updateGameDto, GameStoreContext dbContext) =>
        {
            var existingGame = dbContext.Games.Find(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }
            
            dbContext.Entry(existingGame)
                .CurrentValues
                .SetValues(updateGameDto.ToEntity(id: id));

            dbContext.SaveChanges();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete(pattern: "/{id}", handler: (int id, GameStoreContext dbContext) =>
        {
            dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDelete();

            return Results.NoContent();
        });

        return group;
    }
}