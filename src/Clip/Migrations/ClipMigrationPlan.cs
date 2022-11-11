using Umbraco.Cms.Infrastructure.Migrations;

namespace Clip.Migrations;

public class ClipMigrationPlan : MigrationPlan
{
    public ClipMigrationPlan() : base(Constants.Name)
    {
        DefinePlan();
    }

    public override string InitialState => "75d9e3a8-65d6-42fb-9414-9292a0628823";

    protected void DefinePlan()
    {
        From(InitialState)
            .To<Clip_AddConfigTable>(Clip_AddConfigTable.Key);
    }
}
