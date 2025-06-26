using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app
{
    public class Index : DefaultPage
    {
        public Index(UserSession session) : base(session)
        {
        }

        public override Dictionary<string, object> Render()
        {
            // README Example 1: SessionVar<int> - Counter
            var counter = new Counter(Session, 0);
            counter.Set(counter.Get() + 1);

            // README Example 2: SessionVar<string> - Text storage
            var text = new Text(Session, "Hello World");
            
            // README Example 3: String variable (exactly as in README)
            string ZviratkoDne = "Pes";
            
            // README Example 4: Component instance (exactly as in README)
            var komponenta = new ZvireComponent("Kočka");
            
            // Additional components for demonstration
            var navigation = new NavigationComponent("home");
            var sendTextButton = new SendText(Session);
            
            // Multiple animal components to show variety
            var dogComponent = new ZvireComponent("Pes");
            var rabbitComponent = new ZvireComponent("Králík");

            return new Dictionary<string, object>
            {
                ["title"] = "csWebFrame Demo Application",
                ["subtitle"] = "Implementace všech příkladů z README dokumentace",
                ["navigation"] = navigation,
                
                // README Examples exactly as documented:
                ["dnesni"] = ZviratkoDne, // jako string
                ["komponenta"] = komponenta, // jako instance komponenty
                
                // SessionVar examples
                ["counter"] = counter.Get(),
                ["textValue"] = text.Get(),
                
                // Button example
                ["sendButton"] = sendTextButton,
                
                // Additional components
                ["dogComponent"] = dogComponent,
                ["rabbitComponent"] = rabbitComponent
            };
        }
    }
}