using System.Linq;
using Moq;
using NUnit.Framework;
using SFA.DAS.AutoConfiguration;
using SFA.DAS.ProviderUrlHelper;

namespace SFA.DAS.ProviderUrlHelperTests
{
    [TestFixture]
    public class LinkGeneratorTests
    {
        [TestCase("base", "path", "base/path")]
        [TestCase("base/", "path", "base/path")]
        [TestCase("base", "/path", "base/path")]
        [TestCase("base/", "/path", "base/path")]
        public void ProviderCommitmentsLink_(string providerApprenticeshipServiceUrl, string path, string expectedUrl)
        {
            var fixtures = new LinkGeneratorTestFixtures()
                .WithProviderApprenticeshipServiceBaseUrl(providerApprenticeshipServiceUrl);

            var actualUrl = fixtures.GetProviderApprenticeshipServiceLink(1, path);

            Assert.AreEqual(expectedUrl, actualUrl);
        }
    }
    public class LinkGeneratorTestFixtures
    {
        public LinkGeneratorTestFixtures()
        {
            AutoConfigurationServiceMock = new Mock<IAutoConfigurationService>();

            ProviderUrlConfiguration = new ProviderUrlConfiguration
            {
                BaseUrls = new[] {new BaseUrlKeyValuePair {BaseUrlKey = "ProviderApprenticeshipServiceBaseUrl", BaseUrlValue = "base"}}
            };           

            AutoConfigurationServiceMock.Setup(acs => acs.Get<ProviderUrlConfiguration>()).Returns(ProviderUrlConfiguration);
        }

        public ProviderUrlConfiguration ProviderUrlConfiguration { get; }
        public Mock<IAutoConfigurationService> AutoConfigurationServiceMock { get; }
        public IAutoConfigurationService AutoConfigurationService => AutoConfigurationServiceMock.Object;

        public LinkGeneratorTestFixtures WithProviderApprenticeshipServiceBaseUrl(string baseUrl)
        {
            var config = ProviderUrlConfiguration.BaseUrls.Single(x => x.BaseUrlKey == "ProviderApprenticeshipServiceBaseUrl");
            config.BaseUrlValue = baseUrl;
            return this;
        }

        public string GetProviderApprenticeshipServiceLink(int providerId, string path)
        {
            var linkGenerator = new LinkGenerator(AutoConfigurationService);
            return linkGenerator.ProviderApprenticeshipServiceLink(providerId, path);
        }
    }
}
