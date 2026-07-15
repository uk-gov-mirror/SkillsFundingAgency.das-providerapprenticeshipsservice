using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class UpdateUsersFromDfeSignInFunction(IIdamsSyncService idamsSyncService, ILogger<UpdateUsersFromDfeSignInFunction> logger)
{
    private readonly IIdamsSyncService _idamsSyncService = idamsSyncService;
    private readonly ILogger<UpdateUsersFromDfeSignInFunction> _logger = logger;

    [Function(nameof(UpdateUsersFromDfeSignInFunction))]
    public async Task Run([TimerTrigger("%UpdateUsersFromDfESignInJobSchedule%", RunOnStartup = false)] TimerInfo timerInfo)
    {
        _logger.LogInformation("UpdateUsersFromDfESignIn job started");
        try
        {
            var timer = Stopwatch.StartNew();
            await _idamsSyncService.SyncUsers();
            timer.Stop();

            _logger.LogInformation("UpdateUsersFromDfESignIn job done, Took: {ElapsedMilliseconds} milliseconds", timer.ElapsedMilliseconds);
        }
        catch (AggregateException exc)
        {
            _logger.LogError(exc, "Error running UpdateUsersFromDfESignIn function");
            exc.Handle(ex =>
            {
                _logger.LogError(ex, "Inner exception running UpdateUsersFromDfESignIn function");
                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running UpdateUsersFromDfESignIn function");
            throw;
        }
    }
}
