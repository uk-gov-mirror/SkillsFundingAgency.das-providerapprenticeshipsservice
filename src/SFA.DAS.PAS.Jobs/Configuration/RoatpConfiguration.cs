using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.PAS.Jobs.Configuration;

[ExcludeFromCodeCoverage]
public class RoatpConfiguration : IManagedIdentityClientConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string IdentifierUri { get; set; }
}
