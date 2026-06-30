using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Rebelit.OT.Discover.EdgeApp.API.Filters;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Filters;

[TestFixture]
public class AuthenticationFilterTests
{
    [Test]
    public void OnActionExecuting_WhenApplicationIdHeaderIsMissing_ReturnsUnauthorized()
    {
        var sut = new AuthenticationFilter();
        var context = CreateActionExecutingContext(new Dictionary<string, string>
        {
            ["Api-Access-Token"] = "token"
        });

        sut.OnActionExecuting(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.TypeOf<UnauthorizedObjectResult>());
            Assert.That(((UnauthorizedObjectResult)context.Result!).Value, Is.EqualTo("The required application id is not provided."));
        });
    }

    [Test]
    public void OnActionExecuting_WhenAccessTokenHeaderIsMissing_ReturnsUnauthorized()
    {
        var sut = new AuthenticationFilter();
        var context = CreateActionExecutingContext(new Dictionary<string, string>
        {
            ["Api-Application"] = "app-id"
        });

        sut.OnActionExecuting(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.TypeOf<UnauthorizedObjectResult>());
            Assert.That(((UnauthorizedObjectResult)context.Result!).Value, Is.EqualTo("The required access token is not provided."));
        });
    }

    [Test]
    public void OnActionExecuting_WhenRequiredHeadersArePresent_SetsAuthenticationContextHeaders()
    {
        var sut = new AuthenticationFilter();
        var context = CreateActionExecutingContext(new Dictionary<string, string>
        {
            ["Api-Application"] = "app-id",
            ["Api-Access-Token"] = "token",
            ["Api-Company-Id"] = "company-1",
            ["Api-Agent-Id"] = "agent-1",
            ["OpcUa-Server-Address"] = "opc.tcp://localhost:4840",
            ["OpcUa-Username"] = "opc-user",
            ["OpcUa-Password"] = "opc-password",
            ["Source-Id"] = "source-1"
        });

        var authContext = context.HttpContext.RequestServices.GetRequiredService<IIxonAuthenticationContext>();

        sut.OnActionExecuting(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Result, Is.Null);
            Assert.That(authContext.IxonHeaders.ServiceAccount.ApiApplicationId, Is.EqualTo("app-id"));
            Assert.That(authContext.IxonHeaders.ServiceAccount.AccessToken, Is.EqualTo("token"));
            Assert.That(authContext.IxonHeaders.CompanyId, Is.EqualTo("company-1"));
            Assert.That(authContext.IxonHeaders.AgentId, Is.EqualTo("agent-1"));
            Assert.That(authContext.IxonHeaders.PlcUrl, Is.EqualTo("opc.tcp://localhost:4840"));
            Assert.That(authContext.IxonHeaders.PlcUsername, Is.EqualTo("opc-user"));
            Assert.That(authContext.IxonHeaders.PlcPassword, Is.EqualTo("opc-password"));
            Assert.That(authContext.IxonHeaders.SourceId, Is.EqualTo("source-1"));
        });
    }

    private static ActionExecutingContext CreateActionExecutingContext(Dictionary<string, string> headers)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IIxonAuthenticationContext, IxonAuthenticationContext>();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        foreach (var header in headers)
        {
            httpContext.Request.Headers[header.Key] = header.Value;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
    }
}
