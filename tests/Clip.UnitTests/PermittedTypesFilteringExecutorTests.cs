using Clip.Executors;
using Clip.Models;
using Umbraco.Cms.Core;
using static Umbraco.Cms.Core.Constants;

namespace Clip.UnitTests;

public class PermittedTypesFilteringExecutorTests
{
    [InlineData("ccd097ad-e92b-493f-9786-04e6108ac6be", 4, 10, true)]
    [InlineData("ccd097ad-e92b-493f-9786-04e6108ac6be", 40, 10, false)]
    [InlineData("ddd097a9-e92b-493f-9786-04e6108ac6be", 4, 0, true)]
    [Theory]
    public void HasCapacity_Returns_True_When_Configured_Limit_Is_Less_Than_Existing_Count(string guidString, int existingCount, int configuredLimit, bool expected)
    {
        var guid = Guid.Parse(guidString);
        Udi udi = new GuidUdi(UdiEntityType.DocumentType, guid);

        ClipConfigurationModel config = new()
        {
            ContentTypeCounts = new List<ContentTypeCount>()
            {
                new()
                {
                    UniqueId = guid,
                    NodeObjectType = ObjectTypes.DocumentType,
                    Max = configuredLimit,
                },
            },
            ExistingItemCounts = new()
            {
                { udi.ToString(), existingCount },
            },
        };

        Assert.Equal(expected, PermittedTypesFilteringExecutor.HasCapacity(udi, config));
    }

    [InlineData(
        "umb://document-type/ccd097ad-e92b-493f-9786-04e6108ac6be",
        "umb://document-type/ccd097ad-e92b-493f-9786-04e6108ac6be,umb://document-type/aad097ad-e92b-493f-9786-04e6108ac6be",
        false,
        true)]
    [InlineData(
        "umb://document-type/bbd097ad-e92b-493f-9786-04e6108ac6be",
        "umb://document-type/ccd097ad-e92b-493f-9786-04e6108ac6be,umb://document-type/aad097ad-e92b-493f-9786-04e6108ac6be",
        true,
        false)]
    [InlineData(
        "umb://media-type/ccd097ad-e92b-493f-9786-04e6108ac6be",
        "umb://media-type/ccd097ad-e92b-493f-9786-04e6108ac6be,umb://document-type/aad097ad-e92b-493f-9786-04e6108ac6be",
        true,
        true)]
    [InlineData(
        "umb://media-type/ccd097ad-e92b-493f-9786-04e6108ac6be",
        "umb://document-type/ccd097ad-e92b-493f-9786-04e6108ac6be,umb://document-type/aad097ad-e92b-493f-9786-04e6108ac6be",
        true,
        false)]
    [InlineData(
        "umb://document-type/ccd097ad-e92b-493f-9786-04e6108ac6be",
        "umb://media-type/ccd097ad-e92b-493f-9786-04e6108ac6be,umb://document-type/aad097ad-e92b-493f-9786-04e6108ac6be",
        false,
        false)]
    [Theory]
    public void IsValidChild_Correctly_Determines_Validity(string udiString, string? allowedChildrenString, bool isMediaRequest, bool expected)
    {
        Udi udi = UdiParser.Parse(udiString);
        IEnumerable<string>? allowedChildUdis = allowedChildrenString?.Split(",").Select(x => UdiParser.Parse(x).ToString());

        Assert.Equal(expected, PermittedTypesFilteringExecutor.IsValidChild(udi, allowedChildUdis, isMediaRequest));
    }
}
