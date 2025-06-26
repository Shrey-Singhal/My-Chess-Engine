using ChessEngineAPI.Engine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// register ChessEngineState class with dependency injection.
builder.Services.AddSingleton<ChessEngineState>();

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

// Force ChessEngineState initialization
app.Services.GetRequiredService<ChessEngineState>();

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
