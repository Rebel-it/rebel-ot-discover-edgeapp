using Rebelit.OT.Discover.EdgeApp.API.Models;
using Rebelit.OT.Discover.EdgeApp.API.Services;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;
using System.Text.Json;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services;

[TestFixture]
public class TagServiceTests
{
    [Test]
    public async Task GetTagsAsync_WhenApiReturnsTags()
    {
        // Arrange
        TagVariable[] tagVariables = [
            new() { PublicId = "Var1" },
            new() { PublicId = "Var2" },
            ];
        Tag[] expected =
            [
            new() {LogEvent = "Event1" , LoggingInterval = "180ms", Name = "Tag1" , OnChangeExpiry = 500, RetentionPolicy = "Policy1", Slug = "Slug1", Variable = tagVariables[0], EdgeAggregator = "Min"},
            new() {LogEvent = "Event2" , LoggingInterval = "5m", Name = "Tag2" , OnChangeExpiry = 500, RetentionPolicy = "Policy1", Slug = "Slug1",Variable = tagVariables[1], EdgeAggregator = "Min"},
            ];

        var apiClient = new ApiClientSpy { Tags = expected };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService,logger);

        // Act
        var result = await sut.GetTagsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(result[i].Name, Is.EqualTo(expected[i].Name));
                Assert.That(result[i].LogEvent, Is.EqualTo(expected[i].LogEvent));
                Assert.That(result[i].LoggingInterval, Is.EqualTo(expected[i].LoggingInterval));
                Assert.That(NormalizeObjectValue(result[i].OnChangeExpiry), Is.EqualTo(NormalizeObjectValue(expected[i].OnChangeExpiry)));
                Assert.That(result[i].RetentionPolicy, Is.EqualTo(expected[i].RetentionPolicy));
                Assert.That(result[i].Slug, Is.EqualTo(expected[i].Slug));
                Assert.That(NormalizeObjectValue(result[i].EdgeAggregator), Is.EqualTo(NormalizeObjectValue(expected[i].EdgeAggregator)));
                Assert.That(result[i].Variable.PublicId, Is.EqualTo(expected[i].Variable.PublicId));
            }
        });
    }

    [Test]
    public async Task GetPrefilledTagsAsync_WhenApiReturnsTags_ReturnsPrefilledTags()
    {
        // Arrange
        Variable[] expected =
        [
            new() { PublicId = "v-1", Address = "ns=2;s=Temp", Name = "Temperature", Slug = "temperature", Type = "float", Source = new Source { PublicId = "src-1" } },
            new() { PublicId = "v-2", Address = "ns=2;s=Pressure", Name = "Pressure", Slug = "pressure", Type = "float", Source = new Source { PublicId = "src-1" } },
        ];

        var apiClient = new ApiClientSpy { DataVariables = expected };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = unitTestHelper.CreateAuthenticationContext("agent-42", sourceId: "src-1");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService, logger);

        // Act
        var result = await sut.GetPrefilledTagsAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Count.EqualTo(2));

            Assert.That(result[0].Name, Is.EqualTo("Temperature"));
            Assert.That(result[0].Slug, Is.EqualTo("temperature_stemp"));
            Assert.That(result[0].LogEvent, Is.EqualTo("interval"));
            Assert.That(result[0].LoggingInterval, Is.EqualTo("500ms"));
            Assert.That(result[0].RetentionPolicy, Is.EqualTo("260w"));
            Assert.That(result[0].EdgeAggregator, Is.EqualTo("last"));
            Assert.That(result[0].Variable.PublicId, Is.EqualTo("v-1"));

            Assert.That(result[1].Name, Is.EqualTo("Pressure"));
            Assert.That(result[1].Slug, Is.EqualTo("pressure_spressure"));
            Assert.That(result[1].LogEvent, Is.EqualTo("interval"));
            Assert.That(result[1].LoggingInterval, Is.EqualTo("500ms"));
            Assert.That(result[1].RetentionPolicy, Is.EqualTo("260w"));
            Assert.That(result[1].EdgeAggregator, Is.EqualTo("last"));
            Assert.That(result[1].Variable.PublicId, Is.EqualTo("v-2"));

            Assert.That(apiClient.GetDataVariablesCallCount, Is.EqualTo(1));
            Assert.That(apiClient.GetTagsCallCount, Is.EqualTo(1));
        });
    }

    private static readonly string[] expected = new[] { "Tag1", "Tag2" };

    [Test]
    public async Task CreateTagsAsync_WhenApiReturnsPostedTags_PostsRequestedTags()
    {
        // Arrange
        Tag[] requests =
        [
            new() { LogEvent = "interval", LoggingInterval = "500ms", Name = "Tag1", RetentionPolicy = "260w", Slug = "tag1", Variable = new TagVariable { PublicId = "v-1" }, EdgeAggregator = "last" },
            new() { LogEvent = "interval", LoggingInterval = "500ms", Name = "Tag2", RetentionPolicy = "260w", Slug = "tag2", Variable = new TagVariable { PublicId = "v-2" }, EdgeAggregator = "last" },
        ];
        var apiClient = new ApiClientSpy { PostedTagsResponse = requests };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService, logger);

        // Act
        await sut.CreateTagsAsync(requests);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(apiClient.PostTagsCallCount, Is.EqualTo(1));
            Assert.That(apiClient.PostedTags, Is.Not.Null);
            Assert.That(apiClient.PostedTags!, Has.Count.EqualTo(2));
            Assert.That(apiClient.PostedTags.Select(t => t.Name), Is.EqualTo(expected));

            var postInfoLog = logger.Entries.Single(e => e.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information && e.Message.Contains("Posting 2 tags"));
            var successInfoLog = logger.Entries.Single(e => e.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information && e.Message.Contains("Successfully posted 2 tags"));
            Assert.That(postInfoLog.Message, Is.EqualTo("Posting 2 tags for agent agent-42."));
            Assert.That(successInfoLog.Message, Is.EqualTo("Successfully posted 2 tags for agent agent-42."));
        });
    }

    [Test]
    public async Task CreateTagsAsync_WhenApiReturnsNull_LogsWarning()
    {
        // Arrange
        Tag[] requests =
        [
            new() { LogEvent = "interval", LoggingInterval = "500ms", Name = "Tag1", RetentionPolicy = "260w", Slug = "tag1", Variable = new TagVariable { PublicId = "v-1" }, EdgeAggregator = "last" },
        ];
        var apiClient = new ApiClientSpy { ReturnNullFromPostTags = true };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService, logger);

        // Act
        await sut.CreateTagsAsync(requests);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(apiClient.PostTagsCallCount, Is.EqualTo(1));
            var warningLog = logger.Entries.Single(e => e.LogLevel == Microsoft.Extensions.Logging.LogLevel.Warning);
            Assert.That(warningLog.Message, Is.EqualTo("Posting tags for agent agent-42 returned an unexpected empty response. Attempted to post 1 tags."));
        });
    }

    [Test]
    public async Task CreateTagAsync_WhenApiReturnsCreatedTag_ReturnsCreatedTagAndLogsInformation()
    {
        // Arrange
        var request = new Tag
        {
            Name = "Tag1",
            Slug = "tag1",
            LogEvent = "interval",
            LoggingInterval = "500ms",
            RetentionPolicy = "260w",
            EdgeAggregator = "last",
            Variable = new TagVariable { PublicId = "v-1" },
        };
        var created = new Tag
        {
            Name = "Tag1-created",
            Slug = "tag1",
            LogEvent = "interval",
            LoggingInterval = "500ms",
            RetentionPolicy = "260w",
            EdgeAggregator = "last",
            Variable = new TagVariable { PublicId = "v-1" },
        };

        var apiClient = new ApiClientSpy { PostedTagResponse = created };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService, logger);

        // Act
        var result = await sut.CreateTagAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(apiClient.PostTagCallCount, Is.EqualTo(1));
            Assert.That(apiClient.PostedTag, Is.Not.Null);
            Assert.That(apiClient.PostedTag!.Name, Is.EqualTo("Tag1"));
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Tag1-created"));

            var infoLog = logger.Entries.Single(e => e.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information);
            Assert.That(infoLog.Message, Is.EqualTo("Created tag Tag1-created for agent agent-42."));
        });
    }

    [Test]
    public async Task UpdateTagAsync_WhenApiReturnsUpdatedTag_MapsRequestAndReturnsUpdatedTag()
    {
        // Arrange
        var request = new UpdateTagRequest
        {
            PublicId = "tag-public-id",
            Name = "Tag-Updated",
            Slug = "tag-updated",
            LogEvent = "change",
            LoggingInterval = "5m",
            OnChangeExpiry = "1h",
            RetentionPolicy = "260w",
            Variable = "v-9",
            EdgeAggregator = "min",
        };

        var updated = new Tag
        {
            Name = "Tag-Updated-Result",
            Slug = "tag-updated",
            LogEvent = "change",
            LoggingInterval = "5m",
            OnChangeExpiry = "1h",
            RetentionPolicy = "260w",
            EdgeAggregator = "min",
            Variable = new TagVariable { PublicId = "v-9" },
        };

        var apiClient = new ApiClientSpy { UpdatedTagResponse = updated };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService, logger);

        // Act
        var result = await sut.UpdateTagAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(apiClient.UpdateTagCallCount, Is.EqualTo(1));
            Assert.That(apiClient.UpdatedTagPublicId, Is.EqualTo("tag-public-id"));
            Assert.That(apiClient.UpdatedTagRequest, Is.Not.Null);
            Assert.That(apiClient.UpdatedTagRequest!.Name, Is.EqualTo("Tag-Updated"));
            Assert.That(apiClient.UpdatedTagRequest.Slug, Is.EqualTo("tag-updated"));
            Assert.That(apiClient.UpdatedTagRequest.LogEvent, Is.EqualTo("change"));
            Assert.That(apiClient.UpdatedTagRequest.LoggingInterval, Is.EqualTo("5m"));
            Assert.That(NormalizeObjectValue(apiClient.UpdatedTagRequest.OnChangeExpiry), Is.EqualTo("1h"));
            Assert.That(apiClient.UpdatedTagRequest.RetentionPolicy, Is.EqualTo("260w"));
            Assert.That(NormalizeObjectValue(apiClient.UpdatedTagRequest.EdgeAggregator), Is.EqualTo("min"));
            Assert.That(apiClient.UpdatedTagRequest.Variable.PublicId, Is.EqualTo("v-9"));

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Tag-Updated-Result"));

            var infoLog = logger.Entries.Single(e => e.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information);
            Assert.That(infoLog.Message, Is.EqualTo("Updated tag Tag-Updated-Result for agent agent-42."));
        });
    }

    [Test]
    public async Task UpdateTagAsync_WhenApiReturnsNull_UsesRequestNameInLogAndReturnsNull()
    {
        // Arrange
        var request = new UpdateTagRequest
        {
            PublicId = "tag-public-id",
            Name = "Tag-Fallback-Name",
            Slug = "tag-fallback-name",
            LogEvent = "interval",
            LoggingInterval = "500ms",
            OnChangeExpiry = null,
            RetentionPolicy = "260w",
            Variable = "v-1",
            EdgeAggregator = "last",
        };

        var apiClient = new ApiClientSpy { ReturnNullFromUpdateTag = true };
        var loggerVariable = new TestLogger<VariableService>(true);
        var logger = new TestLogger<TagService>(true);
        var authContext = UnitTestHelpers.CreateAuthenticationContext("agent-42");

        var variableService = new VariableService(apiClient, authContext, loggerVariable);
        var sut = new TagService(apiClient, authContext, variableService, logger);

        // Act
        var result = await sut.UpdateTagAsync(request);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(apiClient.UpdateTagCallCount, Is.EqualTo(1));
            Assert.That(result, Is.Null);

            var infoLog = logger.Entries.Single(e => e.LogLevel == Microsoft.Extensions.Logging.LogLevel.Information);
            Assert.That(infoLog.Message, Is.EqualTo("Updated tag Tag-Fallback-Name for agent agent-42."));
        });
    }

    private static object? NormalizeObjectValue(object? value)
    {
        if (value is not JsonElement element)
        {
            return value;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.String => element.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString(),
        };
    }

}
