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

    public ApplicationStartedHandler(IRuntimeState runtimeState, 
        IMigrationPlanExecutor migrationPlanExecutor, 
        IKeyValueService keyValueService, 
        IScopeProvider scopeProvider)
    {
        _runtimeState = runtimeState;
        _migrationPlanExecutor = migrationPlanExecutor;
        _keyValueService = keyValueService;
        _scopeProvider = scopeProvider;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_runtimeState.Level != RuntimeLevel.Run) return;

        Upgrader? upgrader = new(new ClipMigrationPlan());
        upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
    }
}
