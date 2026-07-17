using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class UpdateUsersFromDfeSignInFunction(IUserSyncService idamsSyncService, ILogger<UpdateUsersFromDfeSignInFunction> logger)
{
    [Function(nameof(UpdateUsersFromDfeSignInFunction))]
    public async Task Run([TimerTrigger("%UpdateUsersFromDfESignInJobSchedule%", RunOnStartup = false)] TimerInfo timerInfo)
    {
        if (timerInfo?.IsPastDue == true)
        {
            logger.LogInformation("UpdateUsersFromDfeSignIn function is running later than scheduled");
        }
        logger.LogInformation("UpdateUsersFromDfeSignInFunction started");

        await idamsSyncService.SyncUsers();

        logger.LogInformation("UpdateUsersFromDfeSignInFunction completed");
    }
}
