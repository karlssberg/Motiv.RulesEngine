using System.Text.RegularExpressions;

namespace Motiv.RulesEngine;

public class Proposition : IEquatable<Proposition>
{
    private static readonly Regex ParameterRegex = PropositionRegexFactory.FindParametersRegex();

    private static readonly Regex ParametersWithBracesRegex =
        PropositionRegexFactory.FindParametersIncludingBracesRegex();

    public Proposition(string template)
    {
        Template = template;
        ParameterNames = FindParameterValues(template);
        Id = Normalize(template);
    }

    public string Template { get; set; }
    public IEnumerable<string> ParameterNames { get; set; }

    public string Id { get; }
    

    public bool Equals(Proposition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Proposition)obj);
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static string Normalize(string proposition) =>
        ParametersWithBracesRegex.Replace(proposition, "");

    public static string[] FindParameterValues(string proposition) =>
        ParameterRegex
            .Matches(proposition)
            .Select(match => match.Groups["parameter"].Value)
            .ToArray();
    
    public IDictionary<string, string> CreateSerializedParameterValueLookup(string candidate)
    {
        var parameterValues = FindParameterValues(candidate);
        return ParameterNames
            .Zip(parameterValues, (name, value) => (name, value))
            .ToDictionary(pair => pair.name, pair => pair.value);
    }
}

public enum ParameterType
{
    String,
    Number,
    Date
}