using Microsoft.EntityFrameworkCore;
using BankingWebApi.Models;
using BankingWebApi.Clients;

var builder = WebApplication.CreateBuilder(args);
var currencyKey = builder.Configuration.GetValue<string>("CURRENCY_API_KEY");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<AccountsContext>(opt => opt.UseInMemoryDatabase("Account"));
builder.Services.AddHttpClient<CurrencyClient>(client => 
        client.BaseAddress = new Uri("https://api.freecurrencyapi.com/v1/latest?apikey="));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
