namespace csWebFrame.app.about.bob;

public class Number(UserSession s, int val) : SessionVar<int>(s, val)
{
    
}


public class index(UserSession s) : DefaultPage(s)
{
    public class SetNumber() : Button
    {
        private Number _number;
        public SetNumber(Number number) : this()
        {
            _number = number;
        }

        public override void OnClick(Dictionary<string, string> data)
        {
           _number.SetFromUserData( "NUM",data);
                
        }

        public KeyValuePair<string, Button> Hash(string key)
        {
            return new KeyValuePair<string, Button>(key, this);
        }
    }
    public override Dictionary<string, object> Render()
    {
        Number num = new Number(Session, 12);
        SetNumber setNumber = new SetNumber(num);
        return new Dictionary<string, object>
        {
            ["var"] = "Hello World!",
            ["nummberr"] = num.Get(),
            ["setNumber"] = setNumber,
            
        };
            
    }
}