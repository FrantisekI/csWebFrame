namespace csWebFrame.app.about.bob;

public class index(UserSession s) : DefaultPage(s)
{
    public class Counter() : Button
    {
        public override void OnClick(Dictionary<string, string> data)
        {
           
                
        }

        public KeyValuePair<string, Button> Hash(string key)
        {
            return new KeyValuePair<string, Button>(key, this);
        }
    }
    public override Dictionary<string, object> Render()
    {
        Counter counter = new Counter();
        return new Dictionary<string, object>
        {
            ["var"] = "Hello World!",
            
        };
            
    }
}