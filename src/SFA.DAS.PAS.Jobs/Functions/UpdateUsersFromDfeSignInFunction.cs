using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SFA.DAS.PAS.Jobs.Extensions;
using SFA.DAS.PAS.Jobs.Services;

namespace SFA.DAS.PAS.Jobs.Functions;

public class UpdateUsersFromDfeSignInFunction(IIdamsSyncService idamsSyncService, ILogger<UpdateUsersFromDfeSignInFunction> logger)
{
    [Function(nameof(UpdateUsersFromDfeSignInFunction))]
    public Task Run([TimerTrigger("%UpdateUsersFromDfESignInJobSchedule%", RunOnStartup = false)] TimerInfo timerInfo)
    {
        return TimedJobRunner.RunAsync(logger, "UpdateUsersFromDfESignIn", idamsSyncService.SyncUsers);
    }
}
