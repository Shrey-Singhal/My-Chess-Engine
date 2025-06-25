using ChessEngine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

Defs.InitFilesRanksBoard();
Defs.InitSq120To64();

Gameboard board = new();
board.InitHashKeys(); // set up random hash keys
board.ParseFEN(Defs.START_FEN); // load initial position
board.posKey = board.GeneratePosKey(); // gen hash for that position

board.PrintBoard();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/api/newgame", () =>
{
    return Results.Ok("New Chess game created and board initialized");
})
.WithName("GetNewGame")
.WithOpenApi();

app.Run();
