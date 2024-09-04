using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine.AspNetCore;

public static class ApplicationExtensions
{
    public static WebApplication UseMotivRulesEngine(this WebApplication app)
    {
        app.MapGet("/motiv/rule/{ruleName}", (string ruleName) =>
        {
            var rule = app.Services.GetRequiredKeyedService<IRule>(ruleName);
            var specs = (IEnumerable<ISpecExport>) app.Services.GetServices(
                typeof(ISpecExport<,>).MakeGenericType(rule.ModelType, rule.MetadataType));

            var specDefinitions = specs
                .Select(descriptor => descriptor.ToPropositionResource());
            
            var source = rule.GetSpec(app.Services).GenerateSource();

            return Results.Ok(new RuleResource(specDefinitions, source));
        })
        .Produces<RuleResource>();
        
        app.MapPut("/motiv/rule/{ruleName}", (string ruleName, RuleResource ruleResource) =>
        {
            var rule = app.Services.GetRequiredKeyedService<IRule>(ruleName);
            var ruleDescriptor = app.Services.GetRequiredKeyedService<IRuleDescriptor>(ruleName);
            
            //ruleDescriptor.PredicateOverride = ruleResource.Source.ToSpec(rule.ModelType, app.Services);
            
            return Results.Ok();
        });
        return app;
    }
}