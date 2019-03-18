#define NETFRAMEWORK

using System;

#if NETFRAMEWORK
namespace SFA.DAS.ProviderUrlHelper
{
    public interface ILinkGenerator
    {
        string ProviderCommitmentsLink(int? providerId, string path);
        string ProviderApprenticeshipServiceLink(int? providerId, string path);
        string ReservationsLink(int? providerId, string path);

        string GenerateNavigationBar(Uri currentUrl, int? providerId, string sectionOverride="");
    }
}
#endif
