using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Rebelit.OT.Discover.EdgeApp.Connections.IXON.Factories;

internal static class IxonResiliencePipelineFactory
{
    internal static ResiliencePipeline<HttpResponseMessage> Create(
        TimeProvider timeProvider,
        ILogger logger,
        Func<Task<string>> tokenRefreshFunc
    )
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage> { TimeProvider = timeProvider }
            .AddRetry(BuildUnauthorizedRetryOptions(logger, tokenRefreshFunc))
            .AddRetry(BuildRateLimitRetryOptions(logger))
            .AddRetry(BuildNoConnectionRetryOptions(logger))
            .Build();
    }

    private static RetryStrategyOptions<HttpResponseMessage> BuildUnauthorizedRetryOptions(
        ILogger logger,
        Func<Task<string>> tokenRefreshFunc
    )
    {
        return new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult(r =>
                r.StatusCode == HttpStatusCode.Unauthorized
            ),
            Delay = TimeSpan.Zero,
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Constant,
            OnRetry = async args =>
            {
                logger.LogWarning(
                    "IXON API returned 401 Unauthorized. Refreshing token and retrying (attempt {AttemptNumber}).",
                    args.AttemptNumber + 1
                );
                await tokenRefreshFunc();
            },
        };
    }

    private static RetryStrategyOptions<HttpResponseMessage> BuildRateLimitRetryOptions(
        ILogger logger
    )
    {
        return new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult(r =>
                r.StatusCode == HttpStatusCode.TooManyRequests
            ),
            Delay = TimeSpan.FromMinutes(2),
            MaxRetryAttempts = int.MaxValue,
            BackoffType = DelayBackoffType.Constant,
            OnRetry = args =>
            {
                logger.LogWarning(
                    "IXON API rate limit hit (429). Waiting {Delay} before retrying (attempt {AttemptNumber}).",
                    args.RetryDelay,
                    args.AttemptNumber + 1
                );
                return ValueTask.CompletedTask;
            },
        };
    }

    private static RetryStrategyOptions<HttpResponseMessage> BuildNoConnectionRetryOptions(
        ILogger logger
    )
    {
        return new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<HttpRequestException>(
                ex => ex.StatusCode == null
            ),
            Delay = TimeSpan.FromMinutes(1),
            MaxRetryAttempts = int.MaxValue,
            BackoffType = DelayBackoffType.Constant,
            OnRetry = args =>
            {
                logger.LogWarning(
                    "IXON API connection failed. Waiting {Delay} before retrying (attempt {AttemptNumber}).",
                    args.RetryDelay,
                    args.AttemptNumber + 1
                );
                return ValueTask.CompletedTask;
            },
        };
    }
}
