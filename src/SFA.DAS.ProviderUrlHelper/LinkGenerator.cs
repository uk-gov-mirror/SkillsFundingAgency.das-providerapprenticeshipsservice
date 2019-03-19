using System;
using System.Text;
using SFA.DAS.AutoConfiguration;

namespace SFA.DAS.ProviderUrlHelper
{
    public class LinkGenerator : ILinkGenerator
    {
        private readonly Lazy<ProviderUrlConfiguration> _lazyProviderConfiguration;

        public LinkGenerator(IAutoConfigurationService autoConfigurationService)
        {
            _lazyProviderConfiguration =
                new Lazy<ProviderUrlConfiguration>(() => LoadProviderUrlConfiguration(autoConfigurationService));
        }

        public string ProviderCommitmentsLink(int providerId, string path)
        {
            var configuration = _lazyProviderConfiguration.Value;
            var baseUrl = configuration.ProviderCommitmentsBaseUrl;

           return Action(baseUrl, path);
        }

        public string ProviderApprenticeshipServiceLink(int providerId, string path)
        {
            var configuration = _lazyProviderConfiguration.Value;
            var baseUrl = configuration.ProviderApprenticeshipServiceBaseUrl;

            return Action(baseUrl, path);
        }

        public string ReservationsLink(int providerId, string path)
        {
            var configuration = _lazyProviderConfiguration.Value;
            var baseUrl = configuration.ReservationsBaseUrl;

            return Action(baseUrl, path);
        }

        private ProviderUrlConfiguration LoadProviderUrlConfiguration(IAutoConfigurationService autoConfigurationService)
        {
            var configuration = autoConfigurationService.Get<ProviderUrlConfiguration>();

            return configuration;
        }

        private static string Action(string baseUrl, string path)
        {
            var trimmedBaseUrl = baseUrl.TrimEnd('/');
            var trimmedPath = path.Trim('/');

            return $"{trimmedBaseUrl}/{trimmedPath}";
        }

        public string GenerateNavigationBar(string currentUrl, int providerId, NavigationSection? sectionOverride)
        {
            var result = new StringBuilder();
            result.Append("<ul role=\"menubar\" id=\"global - nav - links\">");
            result.Append(GenerateNavigationLink(ProviderApprenticeshipServiceLink(providerId, "/account"), currentUrl, "Home", sectionOverride == NavigationSection.Home));
            result.Append(GenerateNavigationLink(ProviderApprenticeshipServiceLink(providerId, $"/{providerId}/apprentices/manage/all"), currentUrl, "Manage your apprentices", sectionOverride == NavigationSection.ManageApprentices));
            result.Append(GenerateNavigationLink(ProviderApprenticeshipServiceLink(providerId, $"/{providerId}/apprentices/cohorts"), currentUrl, "Your cohorts", sectionOverride == NavigationSection.YourCohorts));
            result.Append(GenerateNavigationLink(ProviderApprenticeshipServiceLink(providerId, $"/{providerId}/agreements"), currentUrl, "Organisations and Agreements", sectionOverride == NavigationSection.Agreements));
            result.Append("</ul>");

            return result.ToString();
        }


        private string GenerateNavigationLink(int providerId, string path, Func<int, string, string> linkFunc, string currentUrl, string linkText, bool selectedByOverride)
        {
            var result = new StringBuilder();
            result.Append("<li>");
            var url = linkFunc(providerId, path);

            var selectedClass = selectedByOverride || currentUrl.StartsWith(url) ? "selected" : "";

            result.Append("<a href=\"");
            result.Append(url);
            result.Append("\" role =\"menuitem\"");
            result.Append($" class=\"{selectedClass}\"");
            result.Append(">");
            result.Append(linkText);
            result.Append("</a>");

            result.Append("</li>");
            return result.ToString();
        }


        private string GenerateNavigationLink(string url, string currentUrl, string linkText, bool selectedByOverride)
        {
            var result = new StringBuilder();
            result.Append("<li>");
            var selectedClass = selectedByOverride || currentUrl.StartsWith(url) ? "selected" : "";
            
            result.Append("<a href=\"");
            result.Append(url);
            result.Append("\" role =\"menuitem\"");
            result.Append($" class=\"{selectedClass}\"");
            result.Append(">");
            result.Append(linkText);
            result.Append("</a>");

            result.Append("</li>");
            return result.ToString();
        }



        public enum NavigationSection
        {
            Home, ManageApprentices, YourCohorts, Agreements
        }
    }
}