using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Motiv.RulesEngine;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter<SpecKind>))]
public enum SpecKind
{
    [EnumMember(Value = "proposition")]
    Proposition = 0,
    
    [EnumMember(Value = "and")]
    And = 1,
    
    [EnumMember(Value = "andAlso")]
    AndAlso = 2,
    
    [EnumMember(Value = "or")]
    Or = 3,
    
    [EnumMember(Value = "orElse")]
    OrElse = 4,
    
    [EnumMember(Value = "xor")]
    XOr = 5,
    
    [EnumMember(Value = "not")]
    Not = 6
}