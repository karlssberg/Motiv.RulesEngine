using Antlr4.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine;

public static class ServiceProviderExtensions
{
    public static SpecBase<TModel, TMetadata> ComposeSpec<TModel, TMetadata>(this IServiceProvider provider, string source)
    {
        var input = CharStreams.fromString(source);
        var lexer = new PropositionalLogicLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new PropositionalLogicParser(tokens);
        var ruleDeserializer = new RuleDeserializer<TModel, TMetadata>(provider);
        var root = parser.formula();
        
        return ruleDeserializer.Visit(root);
    }

    public static SpecBase<TModel, TMetadata> GetSpecByName<TModel, TMetadata>(this IServiceProvider provider, string proposition)
    {
        var normalizedProposition = Proposition.Normalize(proposition);
        var exportedSpec = provider.GetRequiredKeyedService<IPropositionExport<TModel, TMetadata>>(normalizedProposition);
        return exportedSpec.CreateInstance(proposition);
    }
    
    public static IEnumerable<IPropositionExport> GetPropositionExports(this IServiceProvider provider, IRuleDescriptor rule) =>
        (IEnumerable<IPropositionExport>) provider.GetServices(
            typeof(IPropositionExport<,>).MakeGenericType(rule.ModelType, rule.MetadataType));
}