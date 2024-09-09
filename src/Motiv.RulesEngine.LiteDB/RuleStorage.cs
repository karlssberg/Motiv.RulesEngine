using LiteDB;

namespace Motiv.RulesEngine.LiteDB;

public class LiteDbRuleStore(string dbPath) : IRuleStore, IDisposable
{
    private readonly LiteDatabase _db = new (dbPath);
    private const string RuleNameKey = "RuleName";
    private const string RuleSourceKey = "RuleSource";
    private const string CollectionName = "rules";
    
    public void SaveRule(string ruleName, string source)
    {
        var collection = _db.GetCollection<BsonDocument>(CollectionName);
        
        var document = new BsonDocument
        {
            [RuleNameKey] = ruleName,
            [RuleSourceKey] = source
        };

        collection.Upsert(ruleName, document);
    }

    public string? LoadRule(string ruleName)
    {
        var collection = _db.GetCollection<BsonDocument>(CollectionName);
        var result = collection.FindById(ruleName);
        if (result is null) return null;
        
        return result?[RuleSourceKey];
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}