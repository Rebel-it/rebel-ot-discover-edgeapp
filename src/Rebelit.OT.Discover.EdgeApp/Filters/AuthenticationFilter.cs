using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;

namespace Rebelit.OT.Discover.EdgeApp.Filters;

public class AuthenticationFilter : IActionFilter
{
    private const string ApplicationIdHeader = "Api-Application";
    private const string AccessTokenHeader = "Api-Access-Token";

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

        var authContext = context.HttpContext.RequestServices.GetRequiredService<IIxonAuthenticationContext>();
        authContext.IxonCredentials = new IxonCredentials
        {
            ApplicationId = applicationId!,
            AccessToken = accessToken!
        };
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
