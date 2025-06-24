namespace csWebFrame.app.about.bob;

public class Counter(UserSession s, int val) : SessionVar<int>(s, val)
{
    public void Increment()
    {
        Set(Get() + 1);
    }
}

public class Text(UserSession s, string val) : SessionVar<string>(s, val);

public class Layout(UserSession s) : DefaultPage(s)
{
    public class CountUp : Button
    {
        readonly Counter _counter;
        public CountUp(Counter counter)
        {
            _counter = counter;
        }
        public override void OnClick(Dictionary<string, string> data)
        {
            _counter.SetFromUserData("number", data);
            Console.WriteLine("pressed the button");
            Console.WriteLine(_counter.Get());
        }
    }

    public class WritePoem : Button
    {
        readonly Text _text;
        public WritePoem(Text text)
        {
            _text = text;
        }
        public override void OnClick(Dictionary<string, string> data)
        {
            _text.SetFromUserData("poem", data);
        }
    }
    public override Dictionary<string, object> Render()
    {
        Counter counter  = new Counter(Session, 12);
        Text text = new Text(Session, "Hello World!");
        
        CountUp countUp = new CountUp(counter);
        WritePoem textB = new WritePoem(text);
        textB.AddFormElement(new Button.InputElementAtrributes(Button.InputElementAtrributes.PossibleAttributes.input, "text", "poem"));
        textB.Name = "Poem";
        // counter.Increment();
        return new Dictionary<string, object>
        {
            ["id"] = 55,
            ["counter"] = countUp,
            ["poem"] = textB,
            ["text"] = text.Get(),
        };
    }
}