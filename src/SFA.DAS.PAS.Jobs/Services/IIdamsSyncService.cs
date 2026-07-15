using System.Threading.Tasks;

namespace SFA.DAS.PAS.Jobs.Services;

public interface IIdamsSyncService
{
    Task SyncUsers();
}
