using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Motiv.RulesEngine;

public record PropositionParameter(string Name, MotivPrimitive Kind)
{
    public bool TryParse(string value, out object result)
    {
        switch (Kind) 
        {
            case MotivPrimitive.Decimal when decimal.TryParse(value, out var decimalValue):
                result = decimalValue;
                return true;
            case MotivPrimitive.Integer when int.TryParse(value, out var intValue):
                result = intValue;
                return true;
            case MotivPrimitive.DateTime when DateTime.TryParse(value, out var dateTimeValue):
                result = dateTimeValue;
                return true;
            case MotivPrimitive.String when value is ['"', .., '"']:
                result = value[1..^1];
                return true;
            case MotivPrimitive.Unknown:
                throw new InvalidOperationException($"The proposition '{Name}' has an 'Unknown' Kind");
            default:
                result = null!;
                return false;
        };
    }
};

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter<MotivPrimitive>))]
public enum MotivPrimitive
{
    [EnumMember(Value = "unknown")]
    Unknown,
    [EnumMember(Value = "decimal")]
    Decimal,
    [EnumMember(Value = "string")]
    String,
    [EnumMember(Value = "datetime")]
    DateTime,
    [EnumMember(Value = "integer")]
    Integer,
}

public static class TypeExtensions
{
    public static MotivPrimitive ToMotivPrimitive(this Type type)
    {
        return type switch
        {
            not null when IsMotivString(type) => MotivPrimitive.String,
            not null when IsMotivDecimal(type) => MotivPrimitive.Decimal,
            not null when IsMotivDateTime(type) => MotivPrimitive.DateTime,
            not null when IsMotivInteger(type) => MotivPrimitive.Integer,
            _ => MotivPrimitive.Unknown
        };
        
        bool IsMotivString(Type t) => t == typeof(string);
        bool IsMotivDecimal(Type t) => t == typeof(decimal) || t == typeof(float) || t == typeof(double);
        bool IsMotivDateTime(Type t) => t == typeof(DateTime) || t == typeof(DateTimeOffset);
        bool IsMotivInteger(Type t) => t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte);
    }
}