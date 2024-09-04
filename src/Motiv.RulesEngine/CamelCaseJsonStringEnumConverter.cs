using System.Text.Json;
using System.Text.Json.Serialization;

namespace Motiv.RulesEngine;

public class CamelCaseJsonStringEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.CamelCase) 
    where T : struct, Enum;