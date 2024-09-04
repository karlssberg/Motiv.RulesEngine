using Motiv.RulesEngine;
using Motiv.RulesEngine.AspNetCore;
using Motiv.RulesEngine.WebApi.rules;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<MoneyNormalizer>();

builder.Services.AddTransientRule<CustomRule>();
builder.Services.AddSpec<IsEven>();
builder.Services.AddSpec<IsPositive>();
builder.Services.AddSpec<IsNegative>();
builder.Services.AddSpec<IsZero>();
builder.Services.AddSpec<IsGreaterThanCurrencyAmount>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        builder =>
        {
            builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowLocalhost");
}

app.UseHttpsRedirection();
app.UseMotivRulesEngine();

app.Run();