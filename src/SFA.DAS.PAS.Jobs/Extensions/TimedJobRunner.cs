using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.PAS.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class TimedJobRunner
{
    public static async Task RunAsync(ILogger logger, string jobName, Func<Task> action)
    {
        logger.LogInformation("{JobName} job started", jobName);

        try
        {
            var timer = Stopwatch.StartNew();
            await action();
            timer.Stop();

            logger.LogInformation("{JobName} job done, Took: {ElapsedMilliseconds} milliseconds", jobName, timer.ElapsedMilliseconds);
        }
        catch (AggregateException exc)
        {
            logger.LogError(exc, "Error running {JobName} function", jobName);
            exc.Handle(ex =>
            {
                logger.LogError(ex, "Inner exception running {JobName} function", jobName);
                return false;
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running {JobName} function", jobName);
            throw;
        }
    }
}
