using Microsoft.Extensions.Logging;
using Rebelit.OT.Discover.EdgeApp.SharedKernel.IxonAuthentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rebelit.OT.Discover.EdgeApp.Tests.Services.Helpers;

public class UnitTestHelpers
{
    public  IxonAuthenticationContext CreateAuthenticationContext(string agentId) =>
        new()
        {
            IxonHeaders = new IxonHeaders
            {
                ServiceAccount = new ServiceAccount
                {
                    AccessToken = "token",
                    ApiApplicationId = "app-id",
                },
                AgentId = agentId,
            },
        };

    
}
