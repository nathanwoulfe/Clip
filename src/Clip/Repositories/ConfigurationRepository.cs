using Clip.Models;
using Newtonsoft.Json;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Clip.Repositories;

internal sealed class ConfigurationRepository : IConfigurationRepository
{
    private const string ExistingItemCountQuery = @"
        SELECT Count(*) as Count, N.uniqueId, N.nodeObjectType
        FROM umbracoContent C
        INNER JOIN cmsContentType CT ON CT.nodeId = C.contentTypeId
        INNER JOIN umbracoContentVersion V ON V.nodeId = C.nodeId
        INNER JOIN umbracoNode N on N.id = CT.nodeId
        WHERE V.[current] = 1
        GROUP BY N.uniqueID, CT.alias, N.nodeObjectType
        ORDER BY N.uniqueID";

    private readonly IScopeProvider _scopeProvider;

    public ConfigurationRepository(IScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

    public ClipConfigurationModel? Get()
    {
        using IScope scope = _scopeProvider.CreateScope();
        string? settingsStr = scope.Database.Fetch<ContentCreationRulesSchema>()?.FirstOrDefault()?.Value;
        _ = scope.Complete();

        if (settingsStr is null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<ClipConfigurationModel>(settingsStr) ?? null;
    }

    public void Save(ClipConfigurationModel model)
    {
        using IScope scope = _scopeProvider.CreateScope();
        ContentCreationRulesSchema poco = scope.Database.Fetch<ContentCreationRulesSchema>()?.FirstOrDefault() ?? new();
        poco.Value = JsonConvert.SerializeObject(model);
        scope.Database.Save(poco);
        _ = scope.Complete();
    }


    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, int> GetItemCounts()
    {
        using IScope scope = _scopeProvider.CreateScope();
        IEnumerable<ContentTypeCount> results = scope.Database.Query<ContentTypeCount>(ExistingItemCountQuery);
        _ = scope.Complete();

        return results.ToDictionary(x => x.Udi.ToString(), x => x.Count);
    }
}
