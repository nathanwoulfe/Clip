using Clip.Models;
using Clip.Repositories;
using Clip.Services;
using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Clip.UnitTests;

public class ConfigurationServiceTests
{
    private readonly IConfigurationService _sut;

    private readonly Mock<IBackOfficeSecurityAccessor> _backOfficeSecurityAccessor = new();
    private readonly Mock<IEventMessagesFactory> _eventMessagesFactory = new();
    private readonly Mock<IContentTypeService> _contentTypeService = new();
    private readonly Mock<IEntityService> _entityService = new();
    private readonly Mock<IConfigurationRepository> _configurationRepository = new();

    public ConfigurationServiceTests()
    {
        _sut = new ConfigurationService(
            _backOfficeSecurityAccessor.Object,
            _eventMessagesFactory.Object,
            _contentTypeService.Object,
            _entityService.Object,
            _configurationRepository.Object);
    }

    // returns (null, null) as user is not a member of any groups with rules configured
    [MemberData(
        nameof(AllowedChildrenParameterGenerator.Get),
        parameters: new object[]
        {
            10,
            new int[] { 1 },
            new int[] { 2 },
            new string[] { "umb://document-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74" },
            -1,
            -1,
        },
        MemberType = typeof(AllowedChildrenParameterGenerator))]

    // returns (1, 0) as user is not a member of one group with a document type rule
    // and 0 rules exist for element-types for the group
    [MemberData(
        nameof(AllowedChildrenParameterGenerator.Get),
        parameters: new object[]
        {
            10,
            new int[] { 1 },
            new int[] { 1 },
            new string[] { "umb://document-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74" },
            1,
            0,
        },
        MemberType = typeof(AllowedChildrenParameterGenerator))]

    // returns (1, 1) as user is a member of two groups, with one item each, where the second will
    // internally be set as an element type as part of the test setup
    // each UDI string is split and assigned to a group
    [MemberData(
        nameof(AllowedChildrenParameterGenerator.Get),
        parameters: new object[]
        {
            10,
            new int[] { 1, 2 },
            new int[] { 1, 2 },
            new string[]
            {
                "umb://document-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74",
                "umb://document-type/724318e6-1134-4fdd-b8e7-52ee7b0c3c74"
            },
            1,
            1,
        },
        MemberType = typeof(AllowedChildrenParameterGenerator))]

    // returns (2, null) as user is a member of two groups, one which has two type rules assigned
    // since both supplied types are media, neither can be elements, so two allowed document types
    // elements is null as no document type children exist, only media
    [MemberData(
        nameof(AllowedChildrenParameterGenerator.Get),
        parameters: new object[]
        {
            10,
            new int[] { 1, 2 },
            new int[] { 1 },
            new string[]
            {
                "umb://media-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://media-type/724318e6-1134-4fdd-b8e7-52ee7b0c3c74"
            },
            2,
            -1,
        },
        MemberType = typeof(AllowedChildrenParameterGenerator))]


    // returns (3, null) as we remove the duplicates and are only looking at media types so no elements exist
    [MemberData(
        nameof(AllowedChildrenParameterGenerator.Get),
        parameters: new object[]
        {
            10,
            new int[] { 1, 2 },
            new int[] { 1, 2, 3 },
            new string[]
            {
                "umb://media-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://media-type/724318e6-1134-4fdd-b8e7-52ee7b0c3c74",
                "umb://media-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://media-type/aa4318e6-1134-4fdd-b8e7-52ee7b0c3c74",
                "umb://media-type/cc0818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://media-type/bb4318e6-1134-4fdd-b8e7-52ee7b0c3c74",
            },
            3,
            -1,
        },
        MemberType = typeof(AllowedChildrenParameterGenerator))]

    // returns (1, 2) as we remove the duplicates and user is not a member of group 3
    [MemberData(
        nameof(AllowedChildrenParameterGenerator.Get),
        parameters: new object[]
        {
            10,
            new int[] { 1, 2 },
            new int[] { 1, 2, 3 },
            new string[]
            {
                "umb://document-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://document-type/724318e6-1134-4fdd-b8e7-52ee7b0c3c74",
                "umb://document-type/720818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://document-type/aa4318e6-1134-4fdd-b8e7-52ee7b0c3c74",
                "umb://document-type/cc0818e6-1134-4fdd-b8e7-52ee7b0c3c74,umb://document-type/bb4318e6-1134-4fdd-b8e7-52ee7b0c3c74",
            },
            1,
            2,
        },
        MemberType = typeof(AllowedChildrenParameterGenerator))]

    [Theory]
    public void Can_Get_ConfigurationModel(
        IUser currentUser,
        IEnumerable<GroupConfigurationModel> groups,
        IEnumerable<IContentType> contentTypes,
        int expectedDocumentTypeCount,
        int expectedElementTypeCount)
    {
        Mock<IBackOfficeSecurity> backOfficeSecurity = new();
        backOfficeSecurity.Setup(x => x.CurrentUser).Returns(currentUser);
        _backOfficeSecurityAccessor.Setup(x => x.BackOfficeSecurity).Returns(backOfficeSecurity.Object);

        _configurationRepository.Setup(x => x.Get()).Returns(new ClipConfigurationModel()
        {
            Groups = groups,
        });

        _entityService.Setup(x => x.GetId(It.IsAny<Udi>())).Returns((Udi udi) =>
        {
            IContentType? contentType = contentTypes.FirstOrDefault(y => y.GetUdi().ToString() == udi.ToString());
            if (contentType is null)
            {
                return Attempt<int>.Fail();
            }

            return Attempt.Succeed(contentType.Id);
        });

        _contentTypeService.Setup(x => x.GetAll(It.IsAny<int[]>()))
            .Returns((int[] ids) => contentTypes.Where(y => ids.Contains(y.Id)));

        ClipConfigurationModel? result = _sut.GetConfigurationModel();

        int? expectedDocCount = expectedDocumentTypeCount < 0 ? null : expectedDocumentTypeCount;
        int? expectedElementCount = expectedElementTypeCount < 0 ? null : expectedElementTypeCount;

        Assert.Equal(expectedDocCount, result?.AllowedChildren?.Count());
        Assert.Equal(expectedElementCount, result?.AllowedElements?.Count());
    }
}
