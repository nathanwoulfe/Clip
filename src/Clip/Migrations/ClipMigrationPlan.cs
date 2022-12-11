using Umbraco.Cms.Infrastructure.Migrations;

namespace Clip.Migrations;

/// <summary>
/// Migration plan.
/// </summary>
public class ClipMigrationPlan : MigrationPlan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClipMigrationPlan"/> class.
    /// </summary>
    public ClipMigrationPlan()
        : base(Constants.Name) => DefinePlan();

    /// <inheritdoc/>
    public override string InitialState => "75d9e3a8-65d6-42fb-9414-9292a0628823";

    /// <summary>
    /// Add migration steps here.
    /// </summary>
    protected void DefinePlan() =>
        _ = From(InitialState)
            .To<Clip_AddConfigTable>(Clip_AddConfigTable.Key);
}
