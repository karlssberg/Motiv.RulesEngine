namespace Motiv.RulesEngine;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ExternalParameterAttribute(string externalParameterName, string parameterName) : Attribute
{
    public string ParameterName { get; } = parameterName;
    public string ExternalParameterName { get; } = externalParameterName;
}