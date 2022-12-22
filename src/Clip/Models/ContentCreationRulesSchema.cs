using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Clip.Models;

[TableName(Constants.ConfigTableName)]
[ExplicitColumns]
[PrimaryKey("Id", AutoIncrement = true)]
internal sealed class ContentCreationRulesSchema
{
    [Column("Id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("Value")]
    [Length(5000)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Value { get; set; }
}
