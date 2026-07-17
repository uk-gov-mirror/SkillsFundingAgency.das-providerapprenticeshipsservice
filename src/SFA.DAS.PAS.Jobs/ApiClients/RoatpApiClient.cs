using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.Authentication.Extensions.Legacy;
using SFA.DAS.PAS.Jobs.ApiModels;
using SFA.DAS.PAS.Jobs.Configuration;

namespace SFA.DAS.PAS.Jobs.ApiClients;

public class RoatpApiClient(
    HttpClient httpClient,
    RoatpConfiguration config,
    ILogger<RoatpApiClient> logger) : ApiClientBase(httpClient), IRoatpApiClient
{
    public async Task<GetAllProvidersResponse> GetProviders()
    {
        logger.LogInformation("Getting Providers from RoATP API");

        var url = $"{BaseUrl()}Organisations";
        var content = await GetAsync(url);

        if (string.IsNullOrWhiteSpace(content))
        {
            logger.LogWarning("RoATP API returned an empty response for {Url}", url);
            return new GetAllProvidersResponse { Organisations = [] };
        }

        var response = JsonConvert.DeserializeObject<GetAllProvidersResponse>(content);

        if (response?.Organisations == null)
        {
            logger.LogWarning("RoATP API response could not be deserialized for {Url}", url);
            return new GetAllProvidersResponse { Organisations = [] };
        }

        return response;
    }

    private string BaseUrl() => config.ApiBaseUrl.TrimEnd('/') + "/";
}
