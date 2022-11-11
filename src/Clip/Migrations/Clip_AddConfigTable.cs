using Clip.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Clip.Migrations;

public sealed class Clip_AddConfigTable : MigrationBase
{
    public const string Key = "469bae4d-c607-493c-a865-315772232849";

    public Clip_AddConfigTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (!TableExists(Constants.ConfigTableName))
        {
            Create.Table<ContentCreationRulesSchema>().Do();
        }
    }
}
