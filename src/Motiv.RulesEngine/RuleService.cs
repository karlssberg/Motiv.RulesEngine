using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public interface IRuleService
{
    IEnumerable<IPropositionExport> GetPropositionExports(string ruleName);
    string GetRule(string ruleName);
    
    void SaveRule(string ruleName, string rule);
    
    IEnumerable<RuleMetadata> GetAllRules();
}

public record RuleMetadata(string Name, string ModelType, string MetadataType, string Rule);

public class RuleService(IServiceProvider provider) : IRuleService
{
    public IEnumerable<IPropositionExport> GetPropositionExports(string ruleName)
    {
        var rule = GetRuleDescriptor(ruleName);
        var modelType = rule.ModelType;
        var metadataType = rule.MetadataType;
        
        return (IEnumerable<IPropositionExport>)provider.GetServices(
            typeof(IPropositionExport<,>).MakeGenericType(modelType, metadataType));
    }
    
    public string GetRule(string ruleName) => GetRuleDescriptor(ruleName).GetSource(provider);
    
    public void SaveRule(string ruleName, string rule)
    {
        var motivParseErrors = provider.Validate(rule);
        if (!motivParseErrors.IsValid)
        {
            const string indent = "    ";
            throw new SyntaxErrorException(
                $"""
                 Syntax error with rule source {rule}. With the following errors: 
                 {indent}{string.Join($"{Environment.NewLine}{indent}", motivParseErrors.Errors)}
                 """);
        }

        var ruleDescriptor = provider.GetRequiredKeyedService<IRuleDescriptor>(ruleName);
        ruleDescriptor.SaveSource(rule);
    }

    public IEnumerable<RuleMetadata> GetAllRules() => provider.GetServices<IRuleDescriptor>().Select(rule => 
        new RuleMetadata(rule.Name, rule.ModelType.Name, rule.MetadataType.Name, rule.GetSource(provider)));

    public IRuleDescriptor GetRuleDescriptor(string ruleName) =>
        provider.GetRequiredKeyedService<IRuleDescriptor>(ruleName);
}