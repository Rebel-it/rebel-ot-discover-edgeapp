using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Agents;
using Rebelit.OT.Discover.EdgeApp.Connections.IXON.Models;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Tests.Agents;

public class ApiClientTests
{
    private static readonly Configuration DefaultConfig = new()
    {
        BaseUrl = "https://test.example.com",
    };

    private static Tag CreateTagRequest() => new()
    {
        LogEvent = "interval",
        LoggingInterval = "1s",
        Name = "n",
        RetentionPolicy = "30d",
        Slug = "s",
        Variable = new TagVariable { PublicId = "var-1" },
    };

    private static (FakeApiClient Client, MockHttpMessageHandler Handler) CreateSut(
        HttpStatusCode status,
        string responseJson
    )
    {
        var handler = new MockHttpMessageHandler(status, responseJson);
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var client = new FakeApiClient(
            optionsMonitor,
            NullLogger<ApiClient>.Instance,
            handler,
            TimeProvider.System
        );
        return (client, handler);
    }

    private static (TestBaseAgent Agent, MockHttpMessageHandler Handler) CreateBaseAgentSut(
        HttpStatusCode status,
        string responseJson = ""
    )
    {
        var handler = new MockHttpMessageHandler(status, responseJson);
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var agent = new TestBaseAgent(
            optionsMonitor,
            NullLogger<BaseAgent>.Instance,
            handler,
            TimeProvider.System
        );
        return (agent, handler);
    }

    [Test]
    public async Task GetDataVariablesAsync_WithOkResponse_ReturnsDeserializedVariables()
    {
        const string json =
            """{"data":[{"publicId":"v1","address":"40001","name":"Temp","slug":"t","type":"int16","width":"1"}]}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);

        var result = await client.GetDataVariablesAsync("agent-1");

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].PublicId, Is.EqualTo("v1"));
        });
    }

    [Test]
    public async Task GetDataVariablesAsync_WhenCalled_UsesCorrectUri()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":[]}""");

        await client.GetDataVariablesAsync("agent-42");

        Assert.That(
            handler.LastRequest!.RequestUri!.ToString(),
            Does.Contain("/api/agents/agent-42/data-variables")
        );
    }

    [Test]
    public async Task GetDataVariablesAsync_WhenCalled_SetsRequiredHeaders()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":[]}""");

        await client.GetDataVariablesAsync("agent-1");

        AssertCommonHeaders(handler.LastRequest!);
    }

    [Test]
    public void GetDataVariablesAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetDataVariablesAsync("agent-1"));
    }

    [Test]
    public void GetDataVariablesAsync_OnServerError_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.InternalServerError, "server error");

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetDataVariablesAsync("agent-1"));
    }

    [Test]
    public async Task GetTagsAsync_WithOkResponse_ReturnsDeserializedTags()
    {
        const string json =
            """{"data":[{"slug":"s","name":"n","logEvent":"interval","loggingInterval":"1s","retentionPolicy":"30d","variable":{"publicId":"var-1"},"edgeAggregator":"Last"}]}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);

        var result = await client.GetTagsAsync("agent-1");

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].Slug, Is.EqualTo("s"));
            Assert.That(result.Data[0].Variable.PublicId, Is.EqualTo("var-1"));
        });
    }

    [Test]
    public async Task GetTagsAsync_WhenCalled_UsesCorrectUri()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":[]}""");

        await client.GetTagsAsync("agent-99");

        Assert.That(
            handler.LastRequest!.RequestUri!.ToString(),
            Does.Contain("/api/agents/agent-99/data-tags")
        );
    }

    [Test]
    public void GetTagsAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetTagsAsync("agent-1"));
    }

    [Test]
    public async Task PostTagAsync_WithOkResponse_ReturnsDeserializedTag()
    {
        const string json =
            """{"data":{"slug":"s","name":"n","logEvent":"interval","loggingInterval":"1s","retentionPolicy":"30d","variable":{"publicId":"var-1"},"edgeAggregator":"Last"}}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);

        var result = await client.PostTagAsync("agent-1", CreateTagRequest());

        Assert.Multiple(() =>
        {
            Assert.That(result!.Data.Slug, Is.EqualTo("s"));
            Assert.That(result.Data.Variable.PublicId, Is.EqualTo("var-1"));
        });
    }

    [Test]
    public async Task PostTagAsync_WhenCalled_UsesCorrectUriAndMethod()
    {
        const string json =
            """{"data":{"slug":"s","name":"n","logEvent":"interval","loggingInterval":"1s","retentionPolicy":"30d","variable":{"publicId":"var-1"}}}""";
        var (client, handler) = CreateSut(HttpStatusCode.OK, json);

        await client.PostTagAsync("agent-5", CreateTagRequest());

        Assert.Multiple(() =>
        {
            Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(
                handler.LastRequest.RequestUri!.ToString(),
                Does.Contain("/api/agents/agent-5/data-tags")
            );
        });
    }

    [Test]
    public void PostTagAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");

        Assert.ThrowsAsync<HttpRequestException>(() => client.PostTagAsync("agent-1", CreateTagRequest()));
    }

    [Test]
    public async Task PostVariableAsync_WithOkResponse_ReturnsDeserializedVariable()
    {
        const string json =
            """{"data":{"publicId":"var-1","address":"40001","name":"Temp","slug":"temp","type":"int16","width":"1"}}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);
        var newVariable = new Variable
        {
            PublicId = "var-1",
            Address = "40001",
            Name = "Temp",
            Slug = "temp",
            Type = "int16",
            Width = "1",
        };

        var result = await client.PostVariableAsync("agent-1", newVariable);

        Assert.That(result!.Data.PublicId, Is.EqualTo("var-1"));
    }

    [Test]
    public async Task PostVariableAsync_WhenCalled_UsesCorrectUriAndMethod()
    {
        const string json =
            """{"data":{"publicId":"v","address":"a","name":"n","slug":"s","type":"t","width":"w"}}""";
        var (client, handler) = CreateSut(HttpStatusCode.OK, json);

        await client.PostVariableAsync(
            "agent-7",
            new Variable
            {
                PublicId = "v",
                Address = "a",
                Name = "n",
                Slug = "s",
                Type = "t",
                Width = "w",
            }
        );

        Assert.Multiple(() =>
        {
            Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(
                handler.LastRequest.RequestUri!.ToString(),
                Does.Contain("/api/agents/agent-7/data-variables")
            );
        });
    }

    [Test]
    public void PostVariableAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");

        Assert.ThrowsAsync<HttpRequestException>(() =>
            client.PostVariableAsync(
                "agent-1",
                new Variable
                {
                    PublicId = "v",
                    Address = "a",
                    Name = "n",
                    Slug = "s",
                    Type = "t",
                    Width = "w",
                }
            )
        );
    }

    [Test]
    public async Task GetAgentAsync_WithOkResponse_ReturnsDeserializedAgent()
    {
        const string json =
            """{"data":{"publicId":"9MuFepQeCJWL","name":"My Agent","deviceId":"dev-001"}}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);

        var result = await client.GetAgentAsync("9MuFepQeCJWL");

        Assert.Multiple(() =>
        {
            Assert.That(result.Data?.PublicId, Is.EqualTo("9MuFepQeCJWL"));
            Assert.That(result.Data?.Name, Is.EqualTo("My Agent"));
            Assert.That(result.Data?.DeviceId, Is.EqualTo("dev-001"));
        });
    }

    [Test]
    public async Task GetAgentAsync_WhenCalled_UsesCorrectUri()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":{"publicId":"abc"}}""");

        await client.GetAgentAsync("agent-42");

        Assert.That(
            handler.LastRequest!.RequestUri!.ToString(),
            Does.Contain("/api/agents/agent-42?fields=publicId,name,deviceId")
        );
    }

    [Test]
    public async Task GetAgentAsync_WhenCalled_SetsRequiredHeaders()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":{"publicId":"abc"}}""");

        await client.GetAgentAsync("agent-1");

        AssertCommonHeaders(handler.LastRequest!);
    }

    [Test]
    public void GetAgentAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetAgentAsync("agent-1"));
    }

    [Test]
    public async Task GetDataSourcesAsync_WithOkResponse_ReturnsDeserializedDataSources()
    {
        const string json =
            """{"data":[{"publicId":"ds-1","name":"My Source","slug":"my-source","device":{"publicId":"dev-1"},"protocol":{"publicId":"modbus"}}]}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);

        var result = await client.GetDataSourcesAsync("agent-1");

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(result.Data[0].PublicId, Is.EqualTo("ds-1"));
            Assert.That(result.Data[0].Protocol.PublicId, Is.EqualTo("modbus"));
        });
    }

    [Test]
    public async Task GetDataSourcesAsync_WhenCalled_UsesCorrectUri()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":[]}""");

        await client.GetDataSourcesAsync("agent-42");

        Assert.That(
            handler.LastRequest!.RequestUri!.ToString(),
            Does.Contain("/api/agents/agent-42/data-sources")
        );
    }

    [Test]
    public async Task GetDataSourcesAsync_WhenCalled_SetsRequiredHeaders()
    {
        var (client, handler) = CreateSut(HttpStatusCode.OK, """{"data":[]}""");

        await client.GetDataSourcesAsync("agent-1");

        AssertCommonHeaders(handler.LastRequest!);
    }

    [Test]
    public void GetDataSourcesAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetDataSourcesAsync("agent-1"));
    }

    [Test]
    public void GetDataSourcesAsync_OnServerError_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.InternalServerError, "server error");

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetDataSourcesAsync("agent-1"));
    }

    [Test]
    public async Task PostDataSourceAsync_WithOkResponse_ReturnsDeserializedDataSource()
    {
        const string json =
            """{"data":{"publicId":"ds-1","name":"My Source","slug":"my-source","device":{"publicId":"dev-1"},"protocol":{"publicId":"modbus"}}}""";
        var (client, _) = CreateSut(HttpStatusCode.OK, json);
        var newDataSource = new DataSource
        {
            Name = "My Source",
            Slug = "my-source",
            Device = new Source { PublicId = "dev-1" },
            Protocol = new DataSourceProtocol { PublicId = "modbus" },
        };

        var result = await client.PostDataSourceAsync("agent-1", newDataSource);

        Assert.That(result!.Data.PublicId, Is.EqualTo("ds-1"));
    }

    [Test]
    public async Task PostDataSourceAsync_WhenCalled_UsesCorrectUriAndMethod()
    {
        const string json =
            """{"data":{"publicId":"ds-1","name":"n","slug":"s","device":{"publicId":"d"},"protocol":{"publicId":"modbus"}}}""";
        var (client, handler) = CreateSut(HttpStatusCode.OK, json);
        var newDataSource = new DataSource
        {
            Name = "n",
            Slug = "s",
            Device = new Source { PublicId = "d" },
            Protocol = new DataSourceProtocol { PublicId = "modbus" },
        };

        await client.PostDataSourceAsync("agent-5", newDataSource);

        Assert.Multiple(() =>
        {
            Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Post));
            Assert.That(
                handler.LastRequest.RequestUri!.ToString(),
                Does.Contain("/api/agents/agent-5/data-sources")
            );
        });
    }

    [Test]
    public void PostDataSourceAsync_OnBadRequest_ThrowsHttpRequestException()
    {
        var (client, _) = CreateSut(HttpStatusCode.BadRequest, "bad request");
        var newDataSource = new DataSource
        {
            Name = "n",
            Slug = "s",
            Device = new Source { PublicId = "d" },
            Protocol = new DataSourceProtocol { PublicId = "modbus" },
        };

        Assert.ThrowsAsync<HttpRequestException>(() =>
            client.PostDataSourceAsync("agent-1", newDataSource)
        );
    }

    [Test]
    public async Task Post_WhenCalled_SendsPostRequest()
    {
        var (agent, handler) = CreateBaseAgentSut(HttpStatusCode.OK);

        await agent.PostAsync("/api/test", new { Value = 1 });

        Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Post));
    }

    [Test]
    public void Post_OnBadRequest_ThrowsHttpRequestException()
    {
        var (agent, _) = CreateBaseAgentSut(HttpStatusCode.BadRequest, "error");

        Assert.ThrowsAsync<HttpRequestException>(() => agent.PostAsync("/api/test", new { }));
    }

    [Test]
    public async Task GetDataVariablesAsync_WhenUnauthorized_DoesNotRetry()
    {
        var responses = new Queue<Func<HttpResponseMessage>>([
            () => new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("unauthorized"),
            },
        ]);
        var handler = new SequentialMockHttpMessageHandler(responses);
        var client = new FakeApiClient(
            new FakeOptionsMonitor(DefaultConfig),
            NullLogger<ApiClient>.Instance,
            handler,
            TimeProvider.System
        );

        Assert.ThrowsAsync<HttpRequestException>(() => client.GetDataVariablesAsync("agent-1"));
        Assert.That(handler.RequestCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetDataVariablesAsync_WhenNoConnectionOnFirstAttempt_RetriesAndReturnsResult()
    {
        const string json =
            """{"data":[{"publicId":"v1","address":"40001","name":"Temp","slug":"t","type":"int16","width":"1"}]}""";
        var responses = new Queue<Func<HttpResponseMessage>>([
            () => throw new HttpRequestException("Connection refused"),
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) },
        ]);
        var handler = new SequentialMockHttpMessageHandler(responses);
        var fakeTime = new FakeTimeProvider();
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var client = new FakeApiClient(
            optionsMonitor,
            NullLogger<ApiClient>.Instance,
            handler,
            fakeTime
        );

        using var cts = new CancellationTokenSource();
        var advanceTask = AdvanceTimeInBackgroundAsync(fakeTime, cts.Token);

        var result = await client.GetDataVariablesAsync("agent-1");

        await cts.CancelAsync();
        await advanceTask;

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Has.Length.EqualTo(1));
            Assert.That(handler.RequestCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetDataVariablesAsync_WhenRateLimited_WaitsTwoMinutesAndRetries()
    {
        const string json = """{"data":[]}""";
        var responses = new Queue<Func<HttpResponseMessage>>([
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) },
        ]);
        var handler = new SequentialMockHttpMessageHandler(responses);
        var fakeTime = new FakeTimeProvider();
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var client = new FakeApiClient(
            optionsMonitor,
            NullLogger<ApiClient>.Instance,
            handler,
            fakeTime
        );

        using var cts = new CancellationTokenSource();
        var advanceTask = AdvanceTimeInBackgroundAsync(fakeTime, cts.Token);

        var result = await client.GetDataVariablesAsync("agent-1");

        await cts.CancelAsync();
        await advanceTask;

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(handler.RequestCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task PostVariableAsync_WhenNoConnectionOnFirstAttempt_RetriesAndReturnsResult()
    {
        const string json =
            """{"data":{"publicId":"var-1","address":"40001","name":"Temp","slug":"temp","type":"int16","width":"1"}}""";
        var responses = new Queue<Func<HttpResponseMessage>>([
            () => throw new HttpRequestException("Connection refused"),
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) },
        ]);
        var handler = new SequentialMockHttpMessageHandler(responses);
        var fakeTime = new FakeTimeProvider();
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var client = new FakeApiClient(
            optionsMonitor,
            NullLogger<ApiClient>.Instance,
            handler,
            fakeTime
        );
        var variable = new Variable
        {
            PublicId = "var-1",
            Address = "40001",
            Name = "Temp",
            Slug = "temp",
            Type = "int16",
            Width = "1",
        };

        using var cts = new CancellationTokenSource();
        var advanceTask = AdvanceTimeInBackgroundAsync(fakeTime, cts.Token);

        var result = await client.PostVariableAsync("agent-1", variable);

        await cts.CancelAsync();
        await advanceTask;

        Assert.Multiple(() =>
        {
            Assert.That(result!.Data.PublicId, Is.EqualTo("var-1"));
            Assert.That(handler.RequestCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task PostVariableAsync_WhenRateLimited_WaitsTwoMinutesAndRetries()
    {
        const string json =
            """{"data":{"publicId":"var-1","address":"40001","name":"Temp","slug":"temp","type":"int16","width":"1"}}""";
        var responses = new Queue<Func<HttpResponseMessage>>([
            () => new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) },
        ]);
        var handler = new SequentialMockHttpMessageHandler(responses);
        var fakeTime = new FakeTimeProvider();
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var client = new FakeApiClient(
            optionsMonitor,
            NullLogger<ApiClient>.Instance,
            handler,
            fakeTime
        );
        var variable = new Variable
        {
            PublicId = "var-1",
            Address = "40001",
            Name = "Temp",
            Slug = "temp",
            Type = "int16",
            Width = "1",
        };

        using var cts = new CancellationTokenSource();
        var advanceTask = AdvanceTimeInBackgroundAsync(fakeTime, cts.Token);

        var result = await client.PostVariableAsync("agent-1", variable);

        await cts.CancelAsync();
        await advanceTask;

        Assert.Multiple(() =>
        {
            Assert.That(result!.Data.PublicId, Is.EqualTo("var-1"));
            Assert.That(handler.RequestCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task GetDataVariablesAsync_WhenMultipleNoConnectionFailures_RetriesEachTime()
    {
        const string json = """{"data":[]}""";
        var responses = new Queue<Func<HttpResponseMessage>>([
            () => throw new HttpRequestException("Connection refused"),
            () => throw new HttpRequestException("Connection refused"),
            () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) },
        ]);
        var handler = new SequentialMockHttpMessageHandler(responses);
        var fakeTime = new FakeTimeProvider();
        var optionsMonitor = new FakeOptionsMonitor(DefaultConfig);
        var client = new FakeApiClient(
            optionsMonitor,
            NullLogger<ApiClient>.Instance,
            handler,
            fakeTime
        );

        using var cts = new CancellationTokenSource();
        var advanceTask = AdvanceTimeInBackgroundAsync(fakeTime, cts.Token);

        var result = await client.GetDataVariablesAsync("agent-1");

        await cts.CancelAsync();
        await advanceTask;

        Assert.Multiple(() =>
        {
            Assert.That(result.Data, Is.Empty);
            Assert.That(handler.RequestCount, Is.EqualTo(3));
        });
    }

    private static Task AdvanceTimeInBackgroundAsync(
        FakeTimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        return Task.Run(
            async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    timeProvider.Advance(TimeSpan.FromMinutes(5));
                    await Task.Delay(10, CancellationToken.None);
                }
            },
            CancellationToken.None
        );
    }

    private static void AssertCommonHeaders(HttpRequestMessage request)
    {
        Assert.Multiple(() =>
        {
            Assert.That(request.Headers.Authorization?.Scheme, Is.EqualTo("Bearer"));
            Assert.That(request.Headers.Authorization?.Parameter, Is.EqualTo("bearer-token"));
            Assert.That(request.Headers.GetValues("Api-Application").First(), Is.EqualTo("app-id"));
            Assert.That(request.Headers.GetValues("Api-Company").First(), Is.EqualTo("company-id"));
            Assert.That(request.Headers.GetValues("Api-Version").First(), Is.EqualTo("2"));
        });
    }

    private sealed class MockHttpMessageHandler(HttpStatusCode statusCode, string content)
        : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequest = request;
            return Task.FromResult(
                new HttpResponseMessage(statusCode) { Content = new StringContent(content) }
            );
        }
    }

    private sealed class SequentialMockHttpMessageHandler(
        Queue<Func<HttpResponseMessage>> responses
    ) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestCount++;
            var factory = responses.Dequeue();
            return Task.FromResult(factory());
        }
    }

    private sealed class FakeApiClient(
        IOptionsMonitor<Configuration> config,
        ILogger<ApiClient> logger,
        HttpMessageHandler handler,
        TimeProvider timeProvider
    ) : ApiClient(config, logger, timeProvider, new FakeIxonAuthenticationContext())
    {
        protected override HttpMessageHandler? GetHttpMessageHandler() => handler;
    }

    private sealed class TestBaseAgent(
        IOptionsMonitor<Configuration> config,
        ILogger<BaseAgent> logger,
        HttpMessageHandler handler,
        TimeProvider timeProvider
    ) : BaseAgent(config, logger, timeProvider, new FakeIxonAuthenticationContext())
    {
        protected override HttpMessageHandler? GetHttpMessageHandler() => handler;

        public Task PostAsync(string uri, object body) => Post(uri, body);
    }

    private sealed class FakeIxonAuthenticationContext : IIxonAuthenticationContext
    {
        public IxonHeaders IxonHeaders => new()
        {
            ServiceAccount = new ServiceAccount
            {
                AccessToken = "bearer-token",
                ApiApplicationId = "app-id",

            },
            CompanyId = "company-id",
            AgentId = "agent-id"
        };

        IxonHeaders IIxonAuthenticationContext.IxonHeaders { get => IxonHeaders; set => throw new NotImplementedException(); }
    }

    private sealed class FakeOptionsMonitor(Configuration config) : IOptionsMonitor<Configuration>
    {
        public Configuration CurrentValue => config;

        public Configuration Get(string? name) => config;

        public IDisposable? OnChange(Action<Configuration, string?> listener) => null;
    }
}
