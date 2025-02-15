﻿using System.Net.Http;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.Commitments.Api.Client;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NLog.Logger.Web.MessageHandlers;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using StructureMap;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.DependencyResolution
{
    public class CommitmentsRegistry : Registry
    {
        public CommitmentsRegistry()
        {
            For<CommitmentsApiClientConfiguration>().Use(c => c.GetInstance<IAutoConfigurationService>().Get<CommitmentsApiClientConfiguration>(ConfigurationKeys.CommitmentsApiClient)).Singleton();
            For<ICommitmentsApiClientConfiguration>().Use(c => c.GetInstance<CommitmentsApiClientConfiguration>());

            For<ITrainingProgrammeApi>().Use<TrainingProgrammeApi>()
                .Ctor<HttpClient>().Is(c => GetHttpClient(c));
            
            For<IProviderCommitmentsApi>().Use<ProviderCommitmentsApi>()
              .Ctor<HttpClient>().Is(c => GetHttpClient(c));

            For<IValidationApi>().Use<ValidationApi>()
                .Ctor<HttpClient>().Is(c => GetHttpClient(c));
        }

        private HttpClient GetHttpClient(IContext context)
        {
            var config = context.GetInstance<CommitmentsApiClientConfiguration>();

            var httpClientBuilder = string.IsNullOrWhiteSpace(config.ClientId)
                ? new HttpClientBuilder().WithBearerAuthorisationHeader(new JwtBearerTokenGenerator(config as IJwtClientConfiguration))
                : new HttpClientBuilder().WithBearerAuthorisationHeader(new AzureActiveDirectoryBearerTokenGenerator(config as IAzureActiveDirectoryClientConfiguration));

            return httpClientBuilder
                .WithDefaultHeaders()
                .WithHandler(new RequestIdMessageRequestHandler())
                .WithHandler(new SessionIdMessageRequestHandler())
                .Build();
        }
    }
}