using System.Text.Json.Serialization;

namespace Motiv.RulesEngine;

public record PropositionResource(string Template)
{
    public string Id { get; } = PropositionRegexFactory
        .FindParametersIncludingBracesRegex()
        .Replace(Template, "");
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, MotivTypeResource>? Parameters { get; init; }

    public string Template { get; } = Template;
}