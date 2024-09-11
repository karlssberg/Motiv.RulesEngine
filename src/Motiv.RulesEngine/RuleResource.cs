namespace Motiv.RulesEngine;

public record GetRuleResource(IEnumerable<PropositionResource> CompatiblePropositions, string Rule);
public record GetAllRulesResource(IEnumerable<GetAllRuleResource> Rules);
public record PutRuleResource(string Rule);

public record GetAllRuleResource(string Name, string Rule);