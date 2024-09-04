using System.Text.Json;

namespace Motiv.RulesEngine;

public static class BinaryOperationSpecExtensions
{
    public static IEnumerable<SpecBase> GetOperands(this IBinaryOperationSpec binaryOperationSpec) =>
        binaryOperationSpec.Left.ToEnumerable().Append(binaryOperationSpec.Right);
    
    public static IEnumerable<SpecBase<TModel>> GetOperands<TModel>(this IBinaryOperationSpec<TModel> binaryOperationSpec) =>
        binaryOperationSpec.Left.ToEnumerable().Append(binaryOperationSpec.Right);
    
    public static IEnumerable<SpecBase<TModel, TMetadata>> GetOperands<TModel, TMetadata>(this IBinaryOperationSpec<TModel, TMetadata> binaryOperationSpec) =>
        binaryOperationSpec.Left.ToEnumerable().Append(binaryOperationSpec.Right);
    
    
}