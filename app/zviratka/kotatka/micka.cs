using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app.zviratka.kotatka
{
    public class micka : DefaultPage
    {
        public micka(UserSession session) : base(session)
        {
        }

        public override Dictionary<string, object> Render()
        {
            var navigation = new NavigationComponent("micka");
            var mickaComponent = new ZvireComponent("Micka");
            
            // Counter for how many times Micka was visited
            var mickaPocitadlo = new Counter(Session, 0);
            mickaPocitadlo.Set(mickaPocitadlo.Get() + 1);
            
            // Text for Micka's mood
            var mickaNalada = new Text(Session, "spokojená");

            return new Dictionary<string, object>
            {
                ["title"] = "Micka - Naše kočička",
                ["navigation"] = navigation,
                ["mickaComponent"] = mickaComponent,
                ["navstev"] = mickaPocitadlo.Get(),
                ["nalada"] = mickaNalada.Get(),
                ["popis"] = "Micka je krásná kočička, která ráda spí a hraje si s klubíčkem."
            };
        }
    }
}
