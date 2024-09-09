using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine.AspNetCore;

public static class WebApplicationExtensions
{
    public static WebApplication MapMotivRulesEngineEndpoints(this WebApplication app)
    {
        app.AddGetRuleEndpoint();
        app.AddPutRuleEndpoint();
        
        return app;
    }

    private static void AddGetRuleEndpoint(this WebApplication app)
    {
        app.MapGet("/motiv/rule/{ruleName}", (string ruleName) =>
            {
                var rule = app.Services.GetRequiredKeyedService<IRuleDescriptor>(ruleName);
                var specs = app.Services.GetPropositionExports(rule);
                var specDefinitions = specs
                    .Select(descriptor => descriptor.ToPropositionResource());
            
                var source = rule.GetSource(app.Services);

                return Results.Ok(new GetRuleResource(specDefinitions, source));
            })
            .Produces<GetRuleResource>();
    }

    private static void AddPutRuleEndpoint(this WebApplication app)
    {
        app.MapPut("/motiv/rule/{ruleName}", (string ruleName, PutRuleResource ruleResource) =>
        {
            var rule = app.Services.GetRequiredKeyedService<IRuleDescriptor>(ruleName);
            rule.SaveSource(ruleResource.Source);
            
            return Results.Ok();
        });
    }
}