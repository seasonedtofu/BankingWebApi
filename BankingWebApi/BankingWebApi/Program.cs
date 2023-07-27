using Microsoft.EntityFrameworkCore;
using BankingWebApi.Clients;
using BankingWebApi.Repositories;
using BankingWebApi.Interfaces;
using System.Reflection;
using BankingWebApi.Formatters;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using BankingWebApi.Context;

var builder = WebApplication.CreateBuilder(args);
var currencyKey = builder.Configuration.GetValue<string>("CURRENCY_API_KEY");

// Database Context 
builder.Services.AddDbContext<AccountsDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("AccountsDbContext")));
//builder.Services.AddDbContext<AccountsContext>(opt => opt.UseInMemoryDatabase("Account"));

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
    options.OutputFormatters.Add(new CsvOutputFormatter());
}).AddXmlDataContractSerializerFormatters();
builder.Services.AddHttpClient<CurrencyClient>(client => 
        client.BaseAddress = new Uri("https://api.freecurrencyapi.com/v1/latest?apikey="));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
        options.IncludeXmlComments(xmlPath);
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Insert JWT Token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JTW",
            Scheme = "bearer"
        });
        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new String[]{ }
                }
            });
    });
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    setupAction.ReportApiVersions = true;
});
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

var app = builder.Build();

// Update database
using var scope = app.Services.CreateScope();
try
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AccountsDbContext>();
    if (dbContext.Database.IsSqlServer())
    {
        dbContext.Database.Migrate();
    }
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogError(ex, "An error occurred while migrating or seeding the database.");

    throw;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
