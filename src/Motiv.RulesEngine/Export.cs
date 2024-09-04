using System.ComponentModel.DataAnnotations;

namespace Motiv.RulesEngine;

/// <summary>
/// Metadata necessary to make a spec or rule composable outside the system.
/// </summary>
/// <param name="identifier">
/// The non-whitespace name.
/// Typically written as <c>my-custom-proposition</c>,
/// but you can also add arguments <c>my-{parameterName}-proposition</c>.
/// Parameters are only supported on Specs.
/// They allow users to define simple
/// </param>
/// <param name="arguments">Default values for </param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ExportAttribute(string identifier, params object[] arguments) : Attribute
{
    [Required]
    public string Identifier { get; } = identifier;
    
    public object[] Arguments { get; } = arguments.All(IsValidTemplateParameterType)
        ? arguments
        : ThrowParameterValidationError(nameof(arguments));

    

    private static object[] ThrowParameterValidationError(string parameterName) =>
        throw new ArgumentException(
            "Only numbers and strings (including chars) are allowed as arguments.",
            parameterName);
    
    private static bool IsValidTemplateParameterType(object argument) =>
        argument is string or
            sbyte or byte or
            short or ushort or
            int or uint or
            long or ulong or
            float or double or
            decimal or
            nint or nuint or
            char;
}