using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app
{
    public class Index : DefaultPage
    {
        private NavigationComponent navigation;

        public Index(UserSession session) : base(session)
        {
            navigation = new NavigationComponent("home");
        }

        public override Dictionary<string, object> Render()
        {
            return new Dictionary<string, object>
            {
                ["title"] = "csWebFrame Demo Application",
                ["subtitle"] = "A powerful C# web framework for building dynamic applications",
                ["navigation"] = navigation
            };
        }
    }
}