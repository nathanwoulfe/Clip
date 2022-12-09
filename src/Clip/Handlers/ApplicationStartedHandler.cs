using Clip.Migrations;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Clip.Handlers;


internal class ApplicationStartedHandler : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IRuntimeState _runtimeState;
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly IScopeProvider _scopeProvider;
    private readonly IKeyValueService _keyValueService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationStartedHandler"/> class.
    /// </summary>
    /// <param name="runtimeState"></param>
    /// <param name="migrationPlanExecutor"></param>
    /// <param name="keyValueService"></param>
    /// <param name="scopeProvider"></param>
    public ApplicationStartedHandler(
        IRuntimeState runtimeState,
        IMigrationPlanExecutor migrationPlanExecutor,
        IKeyValueService keyValueService,
        IScopeProvider scopeProvider)
    {
        _runtimeState = runtimeState;
        _migrationPlanExecutor = migrationPlanExecutor;
        _keyValueService = keyValueService;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc/>
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        Upgrader? upgrader = new(new ClipMigrationPlan());
        _ = upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
    }
}
