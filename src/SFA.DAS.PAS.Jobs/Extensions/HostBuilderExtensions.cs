using System;
using System.IO;
using System.Net.Http;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.DfESignIn.Auth.Api.Client;
using SFA.DAS.DfESignIn.Auth.Api.Helpers;
using SFA.DAS.DfESignIn.Auth.Configuration;
using SFA.DAS.DfESignIn.Auth.Interfaces;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.PAS.Jobs.Configuration;
using SFA.DAS.PAS.Jobs.Services;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Configurations;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Data;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces.Services;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Data;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Extensions;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Services;

namespace SFA.DAS.PAS.Jobs.Extensions;

public static class HostBuilderExtensions
{
    public static void AddConfiguration(this IConfigurationBuilder builder)
    {
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        builder.AddAzureTableStorage(options =>
        {
            options.ConfigurationKeys = [ConfigurationKeys.PasJobsConfiguration, ConfigurationKeys.DfESignInService];
            options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            options.EnvironmentName = configuration["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
        }).Build();
    }

    public static IHostBuilder ConfigurePasServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.Configure<PasJobsConfiguration>(context.Configuration);

            services.AddSingleton<IBaseConfiguration>(isp => isp.GetService<IOptions<PasJobsConfiguration>>()!.Value);

            services.Configure<CommitmentsApiClientV2Configuration>(c => context.Configuration.GetSection(ConfigurationKeys.CommitmentsApiClientV2).Bind(c));

            services.AddSingleton(cfg => cfg.GetService<IOptions<CommitmentsApiClientV2Configuration>>()!.Value);
            services.AddHttpClient<ICommitmentsV2ApiClient, CommitmentsV2ApiClient>();

            services.AddSingleton<TokenCredential>(new ChainedTokenCredential(
                new ManagedIdentityCredential(new ManagedIdentityCredentialOptions()),
                new AzureCliCredential()));

            services.AddTransient<IImportProviderService, ImportProviderService>();
            services.AddTransient<IProviderRepository, ProviderRepository>();

            services.Configure<DfEOidcConfiguration>(context.Configuration.GetSection("DfEOidcConfiguration"));
            services.Configure<DfEOidcConfiguration>(context.Configuration.GetSection("DfEOidcConfiguration_ProviderRoATP"));

            services.AddSingleton(cfg => cfg.GetService<IOptions<DfEOidcConfiguration>>()!.Value);
            services.AddSingleton(isp => isp.GetService<IOptions<PasJobsConfiguration>>()!.Value.CommitmentNotification);

            services.AddHttpClient<IApiHelper, DfeSignInApiHelper>(options => options.Timeout = TimeSpan.FromMinutes(30))
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(HttpClientRetryPolicy());

            services.AddTransient<ITokenDataSerializer, TokenDataSerializer>();
            services.AddTransient<ITokenBuilder, TokenBuilder>();

            services.AddTransient<IHttpClientWrapper>(serviceProvider =>
            {
                var config = serviceProvider.GetService<ProviderNotificationConfiguration>();
                var httpClient = GetHttpClient(config!);
                return new HttpClientWrapper(httpClient);
            });

            services.AddTransient<IIdamsSyncService, IdamsSyncService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IIdamsEmailServiceWrapper, IdamsEmailServiceWrapper>();

            services.AddLogging()
                .AddTelemetryRegistration((IConfigurationRoot)context.Configuration)
                .AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights();
        });

        return hostBuilder;
    }

    public static IHostBuilder ConfigureDasLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging((context, loggingBuilder) =>
        {
            loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
            loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
            loggingBuilder.AddConsole();
        });

        return hostBuilder;
    }

    private static HttpClient GetHttpClient(IJwtClientConfiguration config)
    {
        return new HttpClientBuilder().WithBearerAuthorisationHeader(new JwtBearerTokenGenerator(config)).Build();
    }

    private static IAsyncPolicy<HttpResponseMessage> HttpClientRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }
}
