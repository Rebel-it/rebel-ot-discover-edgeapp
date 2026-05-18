using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.API.Filters;

public class AuthenticationFilter : IActionFilter
{
    private const string ApplicationIdHeader = "Api-Application";
    private const string AccessTokenHeader = "Api-Access-Token";
    private const string CompanyIdHeader = "Api-Company-Id";
    private const string AgentIdHeader = "Api-Agent-Id";
    private const string PlcUrlHeader = "OpcUa-Server-Address";
    private const string PlcUsernameHeader = "OpcUa-Username";
    private const string PlcPasswordHeader = "OpcUa-Password";
    private const string SourceIdHeader = "Source-Id";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue(ApplicationIdHeader, out var applicationId) || string.IsNullOrWhiteSpace(applicationId))
        {
            context.Result = new UnauthorizedObjectResult("The required application id is not provided.");
            return;
        }

        if (!headers.TryGetValue(AccessTokenHeader, out var accessToken) || string.IsNullOrWhiteSpace(accessToken))
        {
            context.Result = new UnauthorizedObjectResult("The required access token is not provided.");
            return;
        }

        headers.TryGetValue(CompanyIdHeader, out var companyId);
        headers.TryGetValue(AgentIdHeader, out var agentId);
        headers.TryGetValue(PlcUrlHeader, out var plcUrl);
        headers.TryGetValue(PlcUsernameHeader, out var plcUsername);
        headers.TryGetValue(PlcPasswordHeader, out var plcPassword);
        headers.TryGetValue(SourceIdHeader, out var sourceId);
        
        var authContext = context.HttpContext.RequestServices.GetRequiredService<IIxonAuthenticationContext>();
        authContext.IxonHeaders = new IxonHeaders
        {
            ServiceAccount = new ServiceAccount
            {
                AccessToken =  accessToken!,
                ApiApplicationId = applicationId!
            },
            CompanyId = companyId,
            AgentId = agentId,
            PlcUrl = plcUrl,
            PlcUsername = plcUsername,
            PlcPassword = plcPassword,
            SourceId = sourceId
        };
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
