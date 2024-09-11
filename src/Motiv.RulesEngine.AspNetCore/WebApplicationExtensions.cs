using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Motiv.RulesEngine.AspNetCore;

public static class WebApplicationExtensions
{
    public static WebApplication MapMotivRulesEngineEndpoints(this WebApplication app)
    {
        app.AddGetRuleEndpoint();
        app.AddPutRuleEndpoint();
        app.AddGetAllRulesEndpoint();
        
        return app;
    }

    private static void AddGetRuleEndpoint(this WebApplication app)
    {
        app.MapGet("/motiv/rule/{ruleName}", 
            (string ruleName, IRuleService ruleService) =>
            {
                var specDefinitions = ruleService
                    .GetPropositionExports(ruleName)
                    .Select(descriptor => descriptor.ToPropositionResource());
            
                var rule = ruleService.GetRule(ruleName);

                return Results.Ok(new GetRuleResource(specDefinitions, rule));
            })
            .Produces<GetRuleResource>();
    }

    private static void AddPutRuleEndpoint(this WebApplication app)
    {
        app.MapPut("/motiv/rule/{ruleName}",
            (string ruleName, PutRuleResource ruleResource, IRuleService ruleService) =>
            {
                ruleService.SaveRule(ruleName, ruleResource.Rule);
                
                return Results.Ok();
            });
    }
    
    private static void AddGetAllRulesEndpoint(this WebApplication app)
    {
        app.MapGet("/motiv/rules",
            (IRuleService ruleService) =>
            {
                var rules = ruleService.GetAllRules()
                    .Select(rule => new GetAllRuleResource(rule.Name, rule.Rule));
                
                return Results.Ok(rules);
            })
            .Produces<IEnumerable<GetAllRuleResource>>();
    }
}