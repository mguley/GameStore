using GameStore.Api.Data;
using GameStore.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connString = builder.Configuration.GetConnectionString(name: "GameStore");
builder.Services.AddSqlite<GameStoreContext>(connectionString: connString);

var app = builder.Build();

app.MapGamesEndpoints();

app.MigrateDb();

app.Run();