using Microsoft.EntityFrameworkCore;
using BankingWebApi.Models;
using BankingWebApi.Clients;
using BankingWebApi.Repositories;
using BankingWebApi.Interfaces;
using System.Reflection;
using BankingWebApi.Formatters;

var builder = WebApplication.CreateBuilder(args);
var currencyKey = builder.Configuration.GetValue<string>("CURRENCY_API_KEY");

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.OutputFormatters.Add(new CsvOutputFormatter());
}).AddXmlDataContractSerializerFormatters();
builder.Services.AddDbContext<AccountsContext>(opt => opt.UseInMemoryDatabase("Account"));
builder.Services.AddHttpClient<CurrencyClient>(client => 
        client.BaseAddress = new Uri("https://api.freecurrencyapi.com/v1/latest?apikey="));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        options.IncludeXmlComments(xmlPath);
    });
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
