using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app
{
    public class Docs : DefaultPage
    {
        public Docs(UserSession session) : base(session)
        {
        }

        public override Dictionary<string, object> Render()
        {
            var navigation = new NavigationComponent("docs");

            return new Dictionary<string, object>
            {
                ["title"] = "Programmer Documentation",
                ["subtitle"] = "Technická dokumentace csWebFrame frameworku",
                ["navigation"] = navigation
            };
        }
    }
}
