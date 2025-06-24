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

    }
    public override Dictionary<string, object> Render()
    {
        Number num = new Number(Session, 12);
        SetNumber setNumber = new SetNumber(num);
        setNumber.formElements = new[] {
            new SetNumber.InputElementAtrributes(Button.InputElementAtrributes.PossibleAttributes.input, "number", "number")
        };
        setNumber.Name = "Set Number";
        setNumber.formElements[0].name = "NUM";
        setNumber.formElements[0].value = "42";
        return new Dictionary<string, object>
        {
            ["var"] = "Hello World!",
            ["nummberr"] = num.Get(),
            ["setNumber"] = setNumber,
            
        };
            
    }
}