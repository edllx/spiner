using DotNetEnv;
using simpleapi;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Env.Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IDataContext, NpgsqlContext>();
builder.Services.AddScoped<IWheatherService, WheatherService>();
builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
