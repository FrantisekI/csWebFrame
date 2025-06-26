using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Reusable navigation component that can be included on any page
    /// </summary>
    public class NavigationComponent : DefaultHtmlComponent
    {
        private string currentPage;
        
        public NavigationComponent(string currentPageName)
        {
            Name = "Navigation";
            HtmlPathFromComponentRoot = "navigation.html";
            currentPage = currentPageName;
        }

        public override Dictionary<string, object> GetVariables(UserSession session)
        {
            return new Dictionary<string, object>
            {
                ["currentPage"] = currentPage,
                ["homeClass"] = currentPage == "home" ? "active" : "",
                ["counterClass"] = currentPage == "counter" ? "active" : "",
                ["calculatorClass"] = currentPage == "calculator" ? "active" : "",
                ["memoryGameClass"] = currentPage == "memorygame" ? "active" : "",
                ["loginClass"] = currentPage == "login" ? "active" : ""
            };
        }

        public override string GetHtml(UserSession session, PostUrl postUrl)
        {
            return CreateFromHtmlFile(session, postUrl);
        }
    }
}