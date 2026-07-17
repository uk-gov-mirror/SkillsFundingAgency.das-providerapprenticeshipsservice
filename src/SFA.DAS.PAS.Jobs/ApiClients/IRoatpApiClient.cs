using System.Threading.Tasks;
using SFA.DAS.PAS.Jobs.ApiModels;

namespace SFA.DAS.PAS.Jobs.ApiClients;

public interface IRoatpApiClient
{
    Task<GetAllProvidersResponse> GetProviders();
}