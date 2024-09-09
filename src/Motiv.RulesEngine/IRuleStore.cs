namespace Motiv.RulesEngine;

public interface IRuleStore
{
    void SaveRule(string ruleName, string ruleSource);
    string? LoadRule(string ruleName);
}