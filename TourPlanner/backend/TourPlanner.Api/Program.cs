using System.Text.Json.Serialization;
using TourPlanner.Business.Interfaces;
using TourPlanner.Business.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSingleton<ITourService, TourService>();

var app = builder.Build();

app.MapControllers();

app.Run();
