#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;

namespace SFA.DAS.ProviderUrlHelper.Core
{
    public static class ProviderUrlHelperExtensions
    {
        public static string ProviderCommitmentsLink(this UrlHelperBase helper, int providerId, string path)
        {
            var linkGenerator = GetLinkGenerator(helper.ActionContext.HttpContext);

            return linkGenerator.ProviderCommitmentsLink(providerId, path);
        }

        public static string ProviderApprenticeshipServiceLink(this UrlHelperBase helper, int providerId, string path)
        {
            var linkGenerator = GetLinkGenerator(helper.ActionContext.HttpContext);

            return linkGenerator.ProviderApprenticeshipServiceLink(providerId, path);

        }

        public static string ReservationsLink(this UrlHelperBase helper, int providerId, string path)
        {
            var linkGenerator = GetLinkGenerator(helper.ActionContext.HttpContext);

            return linkGenerator.ReservationsLink(providerId, path);
        }

        private static ILinkGenerator GetLinkGenerator(HttpContext httpContext)
        {
            return ServiceLocator.Get<ILinkGenerator>(httpContext);
        }

        public static string GenerateNavigationBar(this UrlHelperBase helper, string currentUri, int providerId, LinkGenerator.NavigationSection? sectionOverride)
        {
            var linkGenerator = GetLinkGenerator(helper.ActionContext.HttpContext);
            return linkGenerator.GenerateNavigationBar(currentUri, providerId, sectionOverride);
        }
    }
}
#endif