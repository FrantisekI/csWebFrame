using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app
{
    public class About : DefaultPage
    {
        public About(UserSession session) : base(session)
        {
        }

        public override Dictionary<string, object> Render()
        {
            var navigation = new NavigationComponent("about");

            return new Dictionary<string, object>
            {
                ["title"] = "O aplikaci",
                ["navigation"] = navigation,
                ["framework"] = "csWebFrame",
                ["author"] = "František Zápotocký",
                ["year"] = "2025"
            };
        }
    }
}
