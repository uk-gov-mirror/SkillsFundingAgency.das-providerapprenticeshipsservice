using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.PAS.Jobs.Configuration;

[ExcludeFromCodeCoverage]
public class RoatpConfiguration
{
    public string ApiBaseUrl { get; set; }
    public string IdentifierUri { get; set; }
}
