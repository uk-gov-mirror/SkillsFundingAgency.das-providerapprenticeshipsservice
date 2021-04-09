﻿using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Services
{
    public class ClientContentService : IClientContentService
    {
        private readonly IContentApiClient _client;

        public ClientContentService(IContentApiClient client)
        {
            _client = client;
        }

        public async Task<string> Get(string type, string applicationId)
        {
            var cachedContent = await _client.Get(type, applicationId);

            return cachedContent;
        }
    }
}
