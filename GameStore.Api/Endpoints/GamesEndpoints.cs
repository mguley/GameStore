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
        group.MapGet(pattern: "/", handler: async (GameStoreContext dbContext) => 
            await dbContext.Games
                .Include(game => game.Genre)
                .Select(game => game.ToGameSummaryDto())
                .AsNoTracking()
                .ToListAsync());

        // GET /games/1
        group.MapGet(pattern: "/{id}", handler: async (int id, GameStoreContext dbContext) =>
            {
                Game? game = await dbContext.Games.FindAsync(id);

                return game is null ? 
                    Results.NotFound() : Results.Ok(game.ToGameDetailsDto());
            })
            .WithName(endpointName: getGameEndpointName);

        // POST /games
        group.MapPost(pattern: "/", handler: async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = newGame.ToEntity();

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();
            
            return Results.CreatedAtRoute(routeName: getGameEndpointName, routeValues: new { id = game.Id },
                value: game.ToGameDetailsDto());
        }).WithParameterValidation();

        // PUT /games
        group.MapPut(pattern: "/{id}", handler: async (int id, UpdateGameDto updateGameDto, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }
            
            dbContext.Entry(existingGame)
                .CurrentValues
                .SetValues(updateGameDto.ToEntity(id: id));

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete(pattern: "/{id}", handler: async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                .Where(game => game.Id == id)
                .ExecuteDeleteAsync();

            return Results.NoContent();
        });

        return group;
    }
}