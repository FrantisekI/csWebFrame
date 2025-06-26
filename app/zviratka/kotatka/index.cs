using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app.zviratka.kotatka
{
    public class index : DefaultPage
    {
        public index(UserSession session) : base(session)
        {
        }

        public override Dictionary<string, object> Render()
        {
            var navigation = new NavigationComponent("cats");
            
            // Create cat components
            var generalCat = new ZvireComponent("Kočka");
            var micka = new ZvireComponent("Micka");

            return new Dictionary<string, object>
            {
                ["title"] = "Kočky - Sekce koťátek",
                ["navigation"] = navigation,
                ["generalCat"] = generalCat,
                ["micka"] = micka
            };
        }
    }
}
