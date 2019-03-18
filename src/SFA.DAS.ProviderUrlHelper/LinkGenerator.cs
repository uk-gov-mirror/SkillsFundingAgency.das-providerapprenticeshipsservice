using System;
using System.Linq;
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

        public string ProviderCommitmentsLink(int? providerId, string path)
        {
            var baseUrl = GetBaseUrl("ProviderCommitmentsBaseUrl");
            return Action(ReplaceProviderId(baseUrl, providerId), path);
            }

        public string ProviderApprenticeshipServiceLink(int? providerId, string path)
        {
            var baseUrl = GetBaseUrl("ProviderApprenticeshipServiceBaseUrl");
            return Action(ReplaceProviderId(baseUrl, providerId), path);
        }

        public string ReservationsLink(int? providerId, string path)
        {
            var baseUrl = GetBaseUrl("ReservationsBaseUrl");
            return Action(ReplaceProviderId(baseUrl, providerId), path);
        }

        public string GenerateNavigationBar(Uri currentUri, int? providerId, string sectionOverride ="")
        {
            var result = new StringBuilder();
            var configuration = _lazyProviderConfiguration.Value;
            foreach (var section in configuration.Sections)
            {
                //find the base url
                var baseUrl = GetBaseUrl(section.BaseUrlKey);

                //var linkHtml = Action(ReplaceProviderId(baseUrl, providerId), link.Path);
                //todo: html writer/tag builder

                var url = Action(baseUrl, section.Path);
                var linkHtml = ReplaceProviderId(url, providerId);

                var cssClass = "";
                if (!String.IsNullOrWhiteSpace(sectionOverride))
                {
                    if (sectionOverride == section.SectionId)
                    {
                        cssClass = "selected";
                    }
                }
                else if (currentUri.ToString().StartsWith(linkHtml))
                {
                    cssClass = "selected";
                }
                
                result.Append("<li>");
                result.Append("<a href=\"");
                result.Append(linkHtml);
                result.Append("\" role =\"menuitem\"");
                result.Append($" class=\"{cssClass}\"");
                result.Append(">");
                result.Append(section.LinkText);
                result.Append("</a>");
                result.Append("</li>");

                    //role = "menuitem" class="@(controllerName == "Account" ? "selected" : "")">Home</a></li>

            }

            return result.ToString();
        }

        public string GetBaseUrl(string key)
        {
            var configuration = _lazyProviderConfiguration.Value;
            var result = configuration.BaseUrls.SingleOrDefault(x => x.BaseUrlKey == key);
            return result != null ? result.BaseUrlValue : "";
        }

        private string ReplaceProviderId(string url, int? providerId)
        {
            if (providerId.HasValue)
            {
                return url.Replace("{providerId}", providerId.Value.ToString());
            }

            return url;
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
    }
}