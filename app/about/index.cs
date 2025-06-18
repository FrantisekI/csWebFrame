namespace csWebFrame.app.about;

public class index(UserSession s) : DefaultPage(s)
{
    public override Dictionary<string, object> Render()
    {
        return new Dictionary<string, object>
        {
            ["counter"] = 77,
        };
    }
}