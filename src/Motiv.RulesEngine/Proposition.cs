using System.Text.RegularExpressions;

namespace Motiv.RulesEngine;

public class Proposition : IEquatable<Proposition>
{
    private static readonly Regex ParameterRegex = PropositionRegexFactory.FindParametersRegex();

    public Proposition(string template)
    {
        Template = template;
        ParameterNames = FindTemplateParameters(template);
        Id = PropositionRegexFactory.FindParametersIncludingBracesRegex().Replace(template, "");
    }

    public string Template { get; set; }
    public IEnumerable<string> ParameterNames { get; set; }

    public string Id { get; }

//    private static void ThrowIfParameterMismatch(string parameterName, string[] templateParameters, IDictionary<string, ParameterType> parameters)
//    {
//        var extraParameters = templateParameters.Except(parameters.Keys).ToArray();
//        if (extraParameters.Length != 0)
//            throw new ArgumentException(
//                $"""
//                 Template contains unrecognized parameter(s).
//                     Unable to bind the following parameters:
//                         '{string.Join("', '", extraParameters)}' 
//                 """,
//                parameterName);
//    }



    private static string[] FindTemplateParameters(string template) =>
        ParameterRegex
            .Matches(template)
            .Select(match => match.Groups["parameter"].Value)
            .ToArray();


    

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
}

public enum ParameterType
{
    String,
    Number,
    Date
}