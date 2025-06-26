using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Reusable navigation component that can be included on any page
    /// </summary>
    public class NavigationComponent : DefaultHtmlComponent
    {
        private readonly string _currentPage;

        public NavigationComponent(string currentPage)
        {
            _currentPage = currentPage;
            Name = "NavigationComponent";
        }

        public override Dictionary<string, object> GetVariables(UserSession session)
        {
            return new Dictionary<string, object>
            {
                ["currentPage"] = _currentPage
            };
        }

        public override string GetHtml(UserSession session, PostUrl postUrl)
        {
            string homeClass = _currentPage == "home" ? "nav-active" : "";
            string aboutClass = _currentPage == "about" ? "nav-active" : "";
            string animalsClass = _currentPage == "animals" ? "nav-active" : "";
            string catsClass = _currentPage == "cats" ? "nav-active" : "";
            string docsClass = _currentPage == "docs" ? "nav-active" : "";

            return $@"
<nav class=""navigation"">
    <ul>
        <li><a href=""/"" class=""{homeClass}"">Home</a></li>
        <li><a href=""/zviratka"" class=""{animalsClass}"">Zvířátka</a></li>
        <li><a href=""/zviratka/kotatka"" class=""{catsClass}"">Kočky</a></li>
        <li><a href=""/docs"" class=""{docsClass}"">Docs</a></li>
        <li><a href=""/about"" class=""{aboutClass}"">About</a></li>
    </ul>
</nav>";
        }
    }
}
