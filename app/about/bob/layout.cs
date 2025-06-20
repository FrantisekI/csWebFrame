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
    public override Dictionary<string, object> Render()
    {
        Counter counter  = new Counter(Session, 12);

        counter.Increment();
        // counter.Increment();
        return new Dictionary<string, object>
        {
            ["id"] = counter.Get(),
        };
    }
}