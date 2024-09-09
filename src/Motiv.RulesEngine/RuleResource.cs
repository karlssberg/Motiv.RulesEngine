namespace Motiv.RulesEngine;

public record GetRuleResource(IEnumerable<PropositionResource> CompatiblePropositions, string Source);
public record PutRuleResource(string Source);