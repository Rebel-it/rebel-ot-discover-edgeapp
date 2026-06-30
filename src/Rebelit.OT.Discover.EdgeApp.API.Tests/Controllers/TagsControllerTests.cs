using Microsoft.AspNetCore.Mvc;
using Rebelit.OT.Discover.EdgeApp.API.Controllers;
using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Controllers;

[TestFixture]
public class TagsControllerTests
{
    [Test]
    public async Task GetTagsAsync_WhenCalled_ReturnsOkWithTags()
    {
        // Arrange
        IReadOnlyList<Tag> tags = [CreateTag("tag-a"), CreateTag("tag-b")];
        var service = new FakeTagService { GetTagsResult = tags };
        var sut = new TagsController(service);

        // Act
        var result = await sut.GetTagsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.GetTagsCallCount, Is.EqualTo(1));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(tags));
        });
    }

    [Test]
    public async Task GetPrefilledTagsAsync_WhenCalled_ReturnsOkWithTags()
    {
        // Arrange
        IReadOnlyList<Tag> tags = [CreateTag("tag-prefilled")];
        var service = new FakeTagService { GetPrefilledTagsResult = tags };
        var sut = new TagsController(service);

        // Act
        var result = await sut.GetPrefilledTagsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.GetPrefilledTagsCallCount, Is.EqualTo(1));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(tags));
        });
    }

    [Test]
    public async Task CreateTagAsync_WhenNameIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var service = new FakeTagService();
        var sut = new TagsController(service);
        var model = CreateTag("   ");

        // Act
        var result = await sut.CreateTagAsync(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CreateTagCallCount, Is.EqualTo(0));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo("Tag name is required."));
        });
    }

    [Test]
    public async Task CreateTagAsync_WhenNameIsValid_ReturnsOkWithCreatedTag()
    {
        // Arrange
        var createdTag = CreateTag("created-tag");
        var service = new FakeTagService { CreateTagResult = createdTag };
        var sut = new TagsController(service);
        var model = CreateTag("request-tag");

        // Act
        var result = await sut.CreateTagAsync(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CreateTagCallCount, Is.EqualTo(1));
            Assert.That(service.LastCreateTagRequest, Is.SameAs(model));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(createdTag));
        });
    }

    [Test]
    public async Task CreateTagsAsync_WhenModelIsNull_ReturnsBadRequest()
    {
        // Arrange
        var service = new FakeTagService();
        var sut = new TagsController(service);

        // Act
        var result = await sut.CreateTagsAsync(null!);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CreateTagsCallCount, Is.EqualTo(0));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo("Tag list cannot be empty."));
        });
    }

    [Test]
    public async Task CreateTagsAsync_WhenModelIsValid_ReturnsOk()
    {
        // Arrange
        var service = new FakeTagService();
        var sut = new TagsController(service);
        IEnumerable<Tag> model = [CreateTag("tag-1"), CreateTag("tag-2")];

        // Act
        var result = await sut.CreateTagsAsync(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.CreateTagsCallCount, Is.EqualTo(1));
            Assert.That(service.LastCreateTagsRequests, Is.Not.Null);
            Assert.That(service.LastCreateTagsRequests!.Count, Is.EqualTo(2));

            Assert.That(result, Is.TypeOf<OkResult>());
        });
    }

    [Test]
    public async Task UpdateTagAsync_WhenPublicIdIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var service = new FakeTagService();
        var sut = new TagsController(service);
        var model = CreateUpdateTagRequest("   ");

        // Act
        var result = await sut.UpdateTagAsync(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.UpdateTagCallCount, Is.EqualTo(0));

            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.Value, Is.EqualTo("Tag public ID is required."));
        });
    }

    [Test]
    public async Task UpdateTagAsync_WhenPublicIdIsValid_ReturnsOkWithUpdatedTag()
    {
        // Arrange
        var updatedTag = CreateTag("updated-tag");
        var service = new FakeTagService { UpdateTagResult = updatedTag };
        var sut = new TagsController(service);
        var model = CreateUpdateTagRequest("pub-1");

        // Act
        var result = await sut.UpdateTagAsync(model);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(service.UpdateTagCallCount, Is.EqualTo(1));
            Assert.That(service.LastUpdateTagRequest, Is.SameAs(model));

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.SameAs(updatedTag));
        });
    }

    private static Tag CreateTag(string name)
    {
        return new Tag
        {
            LogEvent = "interval",
            LoggingInterval = "30s",
            Name = name,
            RetentionPolicy = "104w",
            Slug = "slug-" + Guid.NewGuid().ToString("N"),
            Variable = new TagVariable { PublicId = "var-public-id" },
            OnChangeExpiry = null,
            EdgeAggregator = null,
        };
    }

    private static UpdateTagRequest CreateUpdateTagRequest(string publicId)
    {
        return new UpdateTagRequest
        {
            PublicId = publicId,
            LogEvent = "interval",
            LoggingInterval = "30s",
            Name = "updated-name",
            RetentionPolicy = "104w",
            Slug = "updated-slug",
            Variable = "var-public-id",
            OnChangeExpiry = null,
            EdgeAggregator = null,
        };
    }

    private sealed class FakeTagService : ITagService
    {
        public IReadOnlyList<Tag> GetTagsResult { get; init; } = [];
        public IReadOnlyList<Tag> GetPrefilledTagsResult { get; init; } = [];
        public Tag? CreateTagResult { get; init; }
        public IReadOnlyList<Tag> CreateTagsResult { get; init; } = [];
        public Tag? UpdateTagResult { get; init; }

        public int GetTagsCallCount { get; private set; }
        public int GetPrefilledTagsCallCount { get; private set; }
        public int CreateTagCallCount { get; private set; }
        public int CreateTagsCallCount { get; private set; }
        public int UpdateTagCallCount { get; private set; }

        public Tag? LastCreateTagRequest { get; private set; }
        public List<Tag>? LastCreateTagsRequests { get; private set; }
        public UpdateTagRequest? LastUpdateTagRequest { get; private set; }

        public Task<IReadOnlyList<Tag>> GetTagsAsync()
        {
            GetTagsCallCount++;
            return Task.FromResult(GetTagsResult);
        }

        public Task<IReadOnlyList<Tag>> GetPrefilledTagsAsync()
        {
            GetPrefilledTagsCallCount++;
            return Task.FromResult(GetPrefilledTagsResult);
        }

        public Task<Tag?> CreateTagAsync(Tag request)
        {
            CreateTagCallCount++;
            LastCreateTagRequest = request;
            return Task.FromResult(CreateTagResult);
        }

        public Task<IReadOnlyList<Tag>> CreateTagsAsync(IEnumerable<Tag> requests)
        {
            CreateTagsCallCount++;
            LastCreateTagsRequests = requests.ToList();
            return Task.FromResult(CreateTagsResult);
        }

        public Task<Tag?> UpdateTagAsync(UpdateTagRequest request)
        {
            UpdateTagCallCount++;
            LastUpdateTagRequest = request;
            return Task.FromResult(UpdateTagResult);
        }
    }
}
