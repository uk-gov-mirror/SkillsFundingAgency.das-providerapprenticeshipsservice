namespace SFA.DAS.ProviderUrlHelper
{
    public class ProviderUrlConfiguration
    {
        public BaseUrlKeyValuePair[] BaseUrls { get; set; }
        public Section[] Sections { get; set; }
    }

    public class BaseUrlKeyValuePair
    {
        public string BaseUrlKey { get; set; }
        public string BaseUrlValue { get; set; }
    }

    public class Section
    {
        public string SectionId { get; set; }
        public string LinkText { get; set; }
        public string BaseUrlKey { get; set; }
        public string Path { get; set; }
    }



}