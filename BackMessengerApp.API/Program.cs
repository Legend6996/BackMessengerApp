using BackMessengerApp.API;
using BackMessengerApp.Application;
using BackMessengerApp.Core.Settings;
using BackMessengerApp.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuthSettings"));
builder.Services.Configure<YandexOAuthSettings>(builder.Configuration.GetSection("YandexOAuthSettings"));

builder.Services.AddServiceApplication();
builder.Services.AddServicePersistence();
builder.Services.AddServicesGeneral(builder);


var app = builder.Build();

app.AddConfigurationGeneral();

app.Run();
