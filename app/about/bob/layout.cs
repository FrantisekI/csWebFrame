namespace csWebFrame.app.about.bob;

public class Counter(UserSession s, int val) : SessionVar<int>(s, val)
{
    public void Increment()
    {
        Set(Get() + 1);
    }
}

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
            _counter.SetFromUserData(data);
        }
    }
    public override Dictionary<string, object> Render()
    {
        Counter counter  = new Counter(Session, 12);
        CountUp countUp = new CountUp(counter);
        
        // counter.Increment();
        return new Dictionary<string, object>
        {
            ["id"] = counter.Get(),
            ["counter"] = countUp,
        };
    }
}