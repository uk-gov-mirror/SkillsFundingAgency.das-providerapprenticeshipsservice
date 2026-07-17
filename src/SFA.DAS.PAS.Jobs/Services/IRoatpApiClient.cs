using System.Threading.Tasks;
using SFA.DAS.PAS.Jobs.ApiModels;

namespace SFA.DAS.PAS.Jobs.Services;

public interface IRoatpApiClient
{
    Task<GetAllProvidersResponse> GetProviders();
}