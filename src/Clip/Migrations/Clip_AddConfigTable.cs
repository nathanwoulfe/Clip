using Clip.Models;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Clip.Migrations;

internal sealed class Clip_AddConfigTable : MigrationBase
{
    public const string Key = "469bae4d-c607-493c-a865-315772232849";

    /// <summary>
    /// Initializes a new instance of the <see cref="Clip_AddConfigTable"/> class.
    /// </summary>
    /// <param name="context"></param>
    public Clip_AddConfigTable(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override void Migrate()
    {
        if (!TableExists(Constants.ConfigTableName))
        {
            Create.Table<ContentCreationRulesSchema>().Do();
        }
    }
}
