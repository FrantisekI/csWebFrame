using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app.zviratka
{
    public class Index : DefaultPage
    {
        public Index(UserSession session) : base(session)
        {
        }

        public override Dictionary<string, object> Render()
        {
            var navigation = new NavigationComponent("animals");
            
            // Create multiple animal components
            var dog = new ZvireComponent("Pes");
            var cat = new ZvireComponent("Kočka");
            var rabbit = new ZvireComponent("Králík");

            return new Dictionary<string, object>
            {
                ["title"] = "Zvířátka",
                ["navigation"] = navigation,
                ["dog"] = dog,
                ["cat"] = cat,
                ["rabbit"] = rabbit
            };
        }
    }
}
