using ChessEngineAPI.Engine;
using System.Collections.Concurrent;    // ADD: for ConcurrentDictionary

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// // register ChessEngineState class with dependency injection.
// builder.Services.AddSingleton<ChessEngineState>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<ConcurrentDictionary<string, ChessEngineState>>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins(
                "https://my-chess-engine-ui.vercel.app",   // production UI
                "http://localhost:5173"                    //local dev server
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();        // necessary to send the session cookie
    });
});

var app = builder.Build();

// // Force ChessEngineState initialization
// app.Services.GetRequiredService<ChessEngineState>();
app.UseSession();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Ok("Welcome to my Chess Engine API"))
.WithOpenApi();

app.Run();
