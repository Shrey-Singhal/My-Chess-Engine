using System;                                  // for TimeSpan
using Microsoft.AspNetCore.Http;               // for SameSiteMode, CookieSecurePolicy
using ChessEngineAPI.Engine;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// default CORS policy
builder.Services.AddCors(o => o.AddDefaultPolicy(policy =>
    policy
      .WithOrigins(
        "https://my-chess-engine-ui.vercel.app",
        "http://localhost:5173"
      )
      .AllowAnyMethod()
      .AllowAnyHeader()
      .AllowCredentials()
));


// Session + in‑memory games per session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout     = TimeSpan.FromHours(1);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;

    // allow cross‑site cookie for fetch()
    opts.Cookie.SameSite     = SameSiteMode.None;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddSingleton<ConcurrentDictionary<string, ChessEngineState>>();

// Controllers
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── MIDDLEWARE ORDER ────────────────────────────────────────────────────────────



// Must be before UseCors & MapControllers when using endpoint routing
app.UseRouting();

// Apply CORS
app.UseCors();

// Now sessions can read/write the cookie that CORS just allowed
app.UseSession();

// Redirect HTTP→HTTPS (still allows CORS headers)
app.UseHttpsRedirection();

app.MapGet("/api/ping", () => Results.Ok("pong"))
   .RequireCors("AllowAll");


// Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map your controllers
app.MapControllers();

// Root health‑check
app.MapGet("/", () => Results.Ok("Welcome to my Chess Engine API"))
   .WithOpenApi();

app.Run();
