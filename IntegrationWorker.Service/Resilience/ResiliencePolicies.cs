using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace IntegrationWorker.Service.Resilience;

public static class ResiliencePolicies
{

    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>() 
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 секунд
                onRetry: (exception, delay, retryCount, context) =>
                {
                    logger.LogWarning(
                        "Retry {RetryCount} after {Delay}s due to: {Exception}",
                        retryCount, delay.TotalSeconds, exception.Message);
                });
    }
    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, delay) =>
                {
                    logger.LogError(
                        "Circuit broken for {Delay}s due to: {Exception}",
                        delay.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit reset - system recovered");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit half-open - testing recovery");
                });
    }
}