using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Motiv.RulesEngine;
using Motiv.RulesEngine.AspNetCore;
using Motiv.RulesEngine.LiteDB;
using Motiv.RulesEngine.WebApi.rules;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<MoneyNormalizer>();

builder.Services.AddRule<CustomRule>();
builder.Services.AddTransientProposition<IsEven>();
builder.Services.AddTransientProposition<IsPositive>();
builder.Services.AddTransientProposition<IsNegative>();
builder.Services.AddTransientProposition<IsZero>();
builder.Services.AddTransientProposition<IsGreaterThanCurrencyAmount>();
builder.Services.AddMotivRulesEngine();
builder.Services.AddMotivRulesEngineDefaultStore();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policyBuilder => policyBuilder.SetIsOriginAllowed(AllowLocalhost)
            .AllowAnyHeader()
            .AllowAnyMethod());
});
var app = builder.Build();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowLocalhost");
}
else
{
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            // using static System.Net.Mime.MediaTypeNames;
            context.Response.ContentType = MediaTypeNames.Text.Plain;

            await context.Response.WriteAsync("An exception was thrown.");

            var exceptionHandlerPathFeature =
                context.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
            {
                await context.Response.WriteAsync(" The file was not found.");
            }

            if (exceptionHandlerPathFeature?.Path == "/")
            {
                await context.Response.WriteAsync(" Page: Home.");
            }
        });
    });
}

app.UseHttpsRedirection();
app.MapMotivRulesEngineEndpoints();

app.Run();
return;

bool AllowLocalhost(string url)
{
    if(!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        return false;
    
    return uri.Host == "localhost";
}