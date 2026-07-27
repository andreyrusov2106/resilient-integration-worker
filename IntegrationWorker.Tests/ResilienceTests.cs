using IntegrationWorker.Service.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using Polly.CircuitBreaker;
using Xunit;

namespace IntegrationWorker.Tests;

public class ResilienceTests
{
    [Fact]
    public async Task RetryPolicy_ShouldRetryThreeTimes_OnFailure()
    {
        var mockLogger = new Mock<ILogger>();
        var retryPolicy = ResiliencePolicies.CreateRetryPolicy(mockLogger.Object);
        int attemptCount = 0;

        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                attemptCount++;
                await Task.Delay(10); 
                throw new Exception("Simulated failure");
            });
        });

        Assert.Equal(4, attemptCount);
    }

    [Fact]
    public async Task CircuitBreakerPolicy_ShouldOpenAfterFiveFailures()
    {
        var mockLogger = new Mock<ILogger>();
        var circuitBreaker = ResiliencePolicies.CreateCircuitBreakerPolicy(mockLogger.Object);

        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await circuitBreaker.ExecuteAsync(async () =>
                {
                    await Task.Delay(10);
                    throw new Exception("Simulated failure");
                });
            });
        }

        Assert.True(circuitBreaker.CircuitState == CircuitState.Open);

        await Assert.ThrowsAsync<Polly.CircuitBreaker.BrokenCircuitException>(async () =>
        {
            await circuitBreaker.ExecuteAsync(async () =>
            {
                await Task.Delay(10);
            });
        });
    }
}