using Clip.Models;
using Clip.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Clip.Executors;

internal sealed class PermittedTypesFilteringExecutor : IPermittedTypesFilteringExecutor
{
    private readonly IConfigurationService _configService;
    private readonly IEntityService _entityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermittedTypesFilteringExecutor"/> class.
    /// </summary>
    /// <param name="configService"></param>
    public PermittedTypesFilteringExecutor(IConfigurationService configService, IEntityService entityService)
    {
        _configService = configService;
        _entityService = entityService;
    }

    public IEnumerable<ContentItemDisplay> Execute(IEnumerable<ContentItemDisplay> items, bool isMediaRequest)
    {
        Guid? GetGuid(Udi? udi)
        {
            if (udi is null)
            {
                return null;
            }

            Attempt<int> idResult = _entityService.GetId(udi);
            if (idResult.Success == false)
            {
                return null;
            }

            Attempt<Guid> guidResult = _entityService.GetKey(idResult.Result, UmbracoObjectTypes.DocumentType);

            return guidResult.Result;
        }

        IEnumerable<Udi> itemUdis = items.Select(x => Udi.Create(UdiEntityType.DocumentType, x.ContentTypeKey));
        IEnumerable<Udi?>? permittedUdis = Filter(itemUdis, isMediaRequest, config => config?.AllowedElements);

        if (permittedUdis is null)
        {
            return items;
        }

        return items.Where(x => permittedUdis.Any(y => GetGuid(y) == x.ContentTypeKey));
    }

    public IEnumerable<ContentTypeBasic> Execute(IEnumerable<ContentTypeBasic> items, bool isMediaRequest)
    {
        IEnumerable<Udi?>? permittedUdis = Filter(items.Select(x => x.Udi), isMediaRequest, config => config?.AllowedChildren);

        if (permittedUdis is null)
        {
            return items;
        }

        return items.Where(x => permittedUdis.Contains(x.Udi));
    }

    /// <summary>
    /// Do the filtering against the given UDIs.
    /// Returns null if no config, which indicates the normal creation rules should apply
    /// An empty enumerable response has been filtered
    /// </summary>
    /// <param name="udis"></param>
    /// <param name="isMediaRequest"></param>
    /// <param name="collectionAccessor"></param>
    /// <returns></returns>
    internal IEnumerable<Udi?>? Filter(IEnumerable<Udi?> udis, bool isMediaRequest, Func<ClipConfigurationModel?, IEnumerable<string>?> collectionAccessor)
    {
        ClipConfigurationModel? config = _configService.GetConfigurationModel();

        IEnumerable<string>? allowedChildren = collectionAccessor(config);

        // if nothing set, allow the normal content creation
        if (config is null || allowedChildren is null)
        {
            return null;
        }

        // remove any types not permitted for this user
        IEnumerable<Udi?>? filteredUdis = allowedChildren.Any()
            ? udis.Where(c => IsValidChild(c, allowedChildren, isMediaRequest))
            : null;

        if (filteredUdis is null)
        {
            return null;
        }

        if (!filteredUdis.Any())
        {
            return Enumerable.Empty<Udi>();
        }

        // next remove any types where the current count has no capacity for more
        return filteredUdis.Where(x => HasCapacity(x, config));
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="udi"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    internal static bool HasCapacity(Udi? udi, ClipConfigurationModel config)
    {
        int limitFromConfig = config.ContentTypeCounts.FirstOrDefault(y => y.Udi == udi)?.Max ?? 0;
        string? udiString = udi?.ToString();
        int existingInstancesCount = udiString is not null && config.ExistingItemCounts.ContainsKey(udiString) ? config.ExistingItemCounts[udiString] : 0;

        return limitFromConfig == 0 || limitFromConfig > existingInstancesCount;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="udi"></param>
    /// <param name="allowedChildren"></param>
    /// <param name="isMediaRequest"></param>
    /// <returns></returns>
    internal static bool IsValidChild(Udi? udi, IEnumerable<string>? allowedChildren, bool isMediaRequest)
    {
        // AllowedChildren is assumed to be non-null, as this function is only called after a null-check
        if (udi is null)
        {
            return false;
        }

        // if child is included in allowed children
        return allowedChildren!.Contains(udi.ToString()) && udi.EntityType == (isMediaRequest ? UdiEntityType.MediaType : UdiEntityType.DocumentType);
    }
}
