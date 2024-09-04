using System.Reflection;

namespace Motiv.RulesEngine;

public record RuleResource(IEnumerable<PropositionResource> CompatiblePropositions, string Source)
{
    public IEnumerable<PropositionResource> CompatiblePropositions { get; init; } = CompatiblePropositions;
    public string Source { get; init; } = Source;
}

public class RuleDescriptor<TRule> : IRuleDescriptor where TRule : IRule
{
    public RuleDescriptor()
    {
        var type = typeof(TRule);

        Name = type.GetCustomAttribute<ExportAttribute>()?.Identifier ?? type.Name;
    }

    public string Name { get; }
    
    public SpecBase? PredicateOverride { get; set; }
    
    public string? PredicateOverrideSource { get; set; }
}

public interface IRuleDescriptor
{
    SpecBase? PredicateOverride { get; set; }
}