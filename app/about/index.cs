namespace csWebFrame.app.about;

public class index : DefaultPage
{
    public override Dictionary<string, object> Render()
    {
        return new Dictionary<string, object>
        {
            ["counter"] = 77,
        };
    }
}