using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.PAS.Jobs.Configuration;

[ExcludeFromCodeCoverage]
public class CommitmentsApiClientV2Configuration
{
    public string ApiBaseUrl { get; set; }
    public string IdentifierUri { get; set; }
}
