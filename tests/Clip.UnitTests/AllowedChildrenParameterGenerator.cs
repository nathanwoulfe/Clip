using Clip.Models;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Clip.UnitTests;

public class AllowedChildrenParameterGenerator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="userId">The current user id.</param>
    /// <param name="userGroupIds">The groups the current user belongs to.</param>
    /// <param name="configGroupIds">The group ids to add to configuration.</param>
    /// <param name="udis">The UDIs assigned to each config group.</param>
    /// <param name="expectedDocumentTypeCount">The expected count for document types. -1 is used in place of null.</param>
    /// <param name="expectedElementTypeCount">The expected count for element types. -1 is used in place of null.</param>
    /// <returns></returns>
    public static IEnumerable<object[]> Get(
        int userId,
        IEnumerable<int> userGroupIds,
        IEnumerable<int> configGroupIds,
        IEnumerable<string> udis,
        int expectedDocumentTypeCount,
        int expectedElementTypeCount)
    {
        List<object[]> data = [];
        Mock<IUser> currentUser = new();

        currentUser.Setup(x => x.Id).Returns(userId);
        currentUser.Setup(x => x.Groups).Returns(GetUserGroups(userGroupIds));

        IEnumerable<GroupConfigurationModel> groups = configGroupIds.Select((x, idx) => new GroupConfigurationModel()
        {
            GroupId = x,
            Udis = udis.ElementAt(idx).Split(",").Select(UdiParser.Parse),
        });

        // to provide mocked content types, take all known UDIs and generate mocks for each
        IEnumerable<Udi> allUdis = groups.SelectMany(x => x.Udis);
        IEnumerable<IContentType> contentTypes = allUdis.Select((udi, idx) =>
        {
            Guid guid = ((GuidUdi)udi).Guid;
            int id = idx + 1;
            Mock<IContentType> contentType = new();

            contentType.Setup(x => x.Key).Returns(guid);
            contentType.Setup(x => x.Id).Returns(id);
            contentType.Setup(x => x.IsElement).Returns(id % 2 == 0);

            return contentType.Object;
        });

        data.Add([currentUser.Object, groups, contentTypes, expectedDocumentTypeCount, expectedElementTypeCount]);

        return data;
    }

    private static IEnumerable<ReadOnlyUserGroup> GetUserGroups(IEnumerable<int> groupIds) =>
        groupIds.Select(x => new ReadOnlyUserGroup(
            x,
            Guid.NewGuid(),
            null,
            null,
            null,
            null,
            "group" + x.ToString(),
            Enumerable.Empty<int>(),
            Enumerable.Empty<string>(),
            null,
            true));
}
