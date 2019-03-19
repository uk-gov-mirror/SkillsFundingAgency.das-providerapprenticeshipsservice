#if NET462

using UrlHelper=System.Web.Mvc.UrlHelper;

namespace SFA.DAS.ProviderUrlHelper.Framework
{
    public static class ProviderUrlHelperExtensions
    {
        public static string ProviderCommitmentsLink(this UrlHelper helper, int providerId, string path)
        {
            var linkGenerator = GetLinkGenerator();
            
            return linkGenerator.ProviderCommitmentsLink(providerId, path);
        }

        public static string ProviderApprenticeshipServiceLink(this UrlHelper helper, int providerId, string path)
        {
            var linkGenerator = GetLinkGenerator();

            return linkGenerator.ProviderApprenticeshipServiceLink(providerId, path);
        }

        public static string ReservationsLink(this UrlHelper helper, int providerId, string path)
        {
            var linkGenerator = GetLinkGenerator();

            return linkGenerator.ReservationsLink(providerId, path);
        }

        private static ILinkGenerator GetLinkGenerator()
        {
            var linkGenerator = ServiceLocator.Get<ILinkGenerator>();

            return linkGenerator;
        }
    }
}
#endif
