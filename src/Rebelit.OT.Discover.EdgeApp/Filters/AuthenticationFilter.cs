using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Filters;

public class AuthenticationFilter : IActionFilter
{
    private const string ApplicationIdHeader = "Api-Application";
    private const string AccessTokenHeader = "Api-Access-Token";
    private const string CompanyIdHeader = "Api-Company-Id";
    private const string AgentIdHeader = "Api-Agent-Id";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue(ApplicationIdHeader, out var applicationId) || string.IsNullOrWhiteSpace(applicationId))
        {
            context.Result = new BadRequestObjectResult(new { message = $"Missing required header: {ApplicationIdHeader}" });
            return;
        }

        if (!headers.TryGetValue(AccessTokenHeader, out var accessToken) || string.IsNullOrWhiteSpace(accessToken))
        {
            context.Result = new BadRequestObjectResult(new { message = $"Missing required header: {AccessTokenHeader}" });
            return;
        }

        headers.TryGetValue(CompanyIdHeader, out var companyId);
        headers.TryGetValue(AgentIdHeader, out var agentId);
        
        var authContext = context.HttpContext.RequestServices.GetRequiredService<IIxonAuthenticationContext>();
        authContext.IxonHeaders = new IxonHeaders
        {
            ServiceAccount = new ServiceAccount
            {
                AccessToken =  accessToken!,
                ApiApplicationId = applicationId!
            },
            CompanyId = companyId,
            AgentId = agentId
        };
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
