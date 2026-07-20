using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class SynchorniseUsersFunction(IUserSyncService userSyncService, ILogger<SynchorniseUsersFunction> logger)
{
    [Function(nameof(SynchorniseUsersFunction))]
    public async Task Run([TimerTrigger("%SynchorniseUsersFunctionSchedule%", RunOnStartup = false)] TimerInfo timerInfo)
    {
        logger.LogInformation("SynchorniseUsersFunction started");

        await userSyncService.SyncUsers();

        logger.LogInformation("SynchorniseUsersFunction completed");
    }
}
