using System.Text.Json.Serialization;
using TicTacToe.API.Services;

const string AngularClientPolicy = "AngularClient";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Without this, enums like GameMode/Player deserialize from numbers only,
        // so a request body like { "mode": "TwoPlayer" } would fail model binding.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Registered as singletons because game state lives in memory and must persist across requests.
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<ComputerPlayerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularClientPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors(AngularClientPolicy);

app.MapControllers();

app.Run();
