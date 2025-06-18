namespace csWebFrame.app;

public class index(UserSession s) : DefaultPage(s)
{
    public override Dictionary<string, object> Render()
    {
        return new Dictionary<string, object>
        {
            ["counter"] = 12,
        };
    }
}