using ChessEngine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Defs.InitFilesRanksBoard();
Defs.InitSq120To64();

Gameboard board = new Gameboard();
board.InitHashKeys();
board.posKey = board.GeneratePosKey();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/newgame", () =>
{
    return Results.Ok("New Chess game created and board initialized");
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
