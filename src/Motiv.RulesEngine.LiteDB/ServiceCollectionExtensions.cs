using Microsoft.Extensions.DependencyInjection;

namespace Motiv.RulesEngine.LiteDB;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMotivRulesEngineDefaultStore(this IServiceCollection services, string dbPath = Defaults.DbPath)
    {
        services.AddSingleton<IRuleStore>(_ => new LiteDbRuleStore(dbPath));
        
        return services;
    }
}