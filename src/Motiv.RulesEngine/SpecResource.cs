using System.Text.Json.Serialization;

namespace Motiv.RulesEngine;

public record SpecResource(SpecKind Kind)
{
    public string Id { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, MotivTypeResource>? Parameters { get; init; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<SpecResource>? Operands { get; init; }
    
    [JsonPropertyName("_type")]
    public SpecKind Kind { get; init; } = Kind;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; init; } = Enum.GetName(Kind)!;
}