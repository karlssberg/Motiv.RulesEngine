using System.Reflection;
using System.Text.RegularExpressions;

namespace Motiv.RulesEngine;

public static class MotivSourceGeneratorExtensions
{
    public static string GenerateSource<TModel, TMetadata>(this SpecBase<TModel, TMetadata> rootSpec) => GenerateSourceInternal(rootSpec);
    
    private static string GenerateSourceInternal<TModel, TMetadata>(this SpecBase<TModel, TMetadata> rootSpec) => string.Concat(VisitSpec(rootSpec));

    private static IEnumerable<string> VisitSpec<TModel, TMetadata>(SpecBase<TModel, TMetadata> spec)
    {
        return spec switch
        {
            IBinaryOperationSpec<TModel, TMetadata> binarySpec => CreateBinaryExpression(binarySpec),
            IUnaryOperationSpec<TModel, TMetadata> unarySpec when unarySpec.Operation == Operator.Not =>
                CreateNotExpression(unarySpec.Operand),
            SpecWithCustomParameterValues<TModel, TMetadata> customSpec =>
                GenerateIdentifierWithParameters(customSpec),
            _ => [spec.GetExportIdentifier() ?? spec.Statement.ToExternalName()]
        };
    }

    private static IEnumerable<string> GenerateIdentifierWithParameters<TModel, TMetadata>(SpecWithCustomParameterValues<TModel, TMetadata> customSpec)
    {
        var exportName = customSpec.Underlying.GetExportIdentifier();
        var splitText = Regex.Split(exportName, "(\\{[^}]+\\})");
        foreach (var text in splitText)
        {
            if (text is ['{', .., '}'])
            {
                var parameterName = text[1..^1];
                yield return "{";
                yield return SerializeParameterValue(customSpec.ParametersValues[parameterName]);
                yield return "}";
            }
            else
            {
                yield return text;
            }
        }
    }

    private static string SerializeParameterValue(object parameterValue)
    {
        if (parameterValue is string)
            return $"\"{parameterValue}\"";
        
        return parameterValue.ToString() ?? "null";
    }

    private static IEnumerable<string> CreateNotExpression<TModel, TMetadata>(SpecBase<TModel, TMetadata> spec)
    {
        yield return "!";
        var shouldWrap = spec is IBinaryOperationSpec<TModel>;

        if (shouldWrap)
            yield return "(";

        foreach (var underlying in VisitSpec(spec))
            yield return underlying;

        if (shouldWrap)
            yield return ")";
    }

    private static IEnumerable<string> CreateBinaryExpression<TModel, TMetadata>(IBinaryOperationSpec<TModel, TMetadata> binarySpec)
    {
        var firstIteration = true;

        foreach (var operand in binarySpec.GetOperands())
        {
            if (!firstIteration)
            {
                yield return " ";
                yield return GetBinaryOperatorLiteral(binarySpec.Operation);
                yield return " ";
            }

            var shouldWrap = operand switch
            {
                IBinaryOperationSpec<TModel> opBinarySpec => opBinarySpec.Operation != binarySpec.Operation,
                _ => false
            };
                
            if (shouldWrap)
                yield return "(";

            foreach (var underlying in VisitSpec(operand))
                yield return underlying;

            if (shouldWrap)
                yield return ")";

            firstIteration = false;
        }
    }
    private static string GetBinaryOperatorLiteral(string op) =>
        op switch
        {
            _ when op == Operator.And => "&",
            _ when op == Operator.AndAlso => "&&",
            _ when op == Operator.Or => "|",
            _ when op == Operator.OrElse => "||",
            _ when op == Operator.XOr => "^",
            _ => throw new InvalidOperationException($"Invalid binary operator '{op}'")
        };
}