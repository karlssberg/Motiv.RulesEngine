using System.Text.RegularExpressions;

namespace Motiv.RulesEngine;

public static partial class PropositionRegexFactory
{

    [GeneratedRegex(@"(?:\{)(?<parameter>[^}]+)(?:\})", RegexOptions.Compiled)]
    public static partial Regex FindParametersRegex();
    
    [GeneratedRegex(@"\{[^}]+\}", RegexOptions.Compiled)]
    public static partial Regex FindParametersIncludingBracesRegex();
}