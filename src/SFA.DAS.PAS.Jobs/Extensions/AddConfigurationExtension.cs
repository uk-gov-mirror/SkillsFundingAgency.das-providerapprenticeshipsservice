using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.PAS.Jobs.Configuration;

namespace SFA.DAS.PAS.Jobs.Extensions;

[ExcludeFromCodeCoverage]
public static class AddConfigurationExtension
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
        });
    }
}
