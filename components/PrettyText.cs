namespace csWebFrame.components;

public class Text(UserSession s, string d) : SessionVar<string>(s, d)
{
    
}
public class SendText : Button
{
    private UserSession _session;
    public SendText(UserSession session)
    {
        _session = session;
    }
    public override void OnClick(Dictionary<string, string> data)
    {
        Text text = new Text(_session, "Hello World");
        text.SetFromUserData(data);

    }
}

public class PrettyText : DefaultHtmlComponent
{
    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        Text text = new Text(session, "Hello World");
        Console.WriteLine("in Text");
        return new Dictionary<string, object>
        {
            ["text"] = text.Get(),
            ["smile"] = (new SmileButton()),
            ["sendText"] = (new SendText(session)),
        };
    }

    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        Text text = new Text(session, "Hello World");
        return $"<u>{text.Get()}</u><br><p> {(new SendText(session)).GetHtml(session, new PostUrl(postUrl, "sendText"))} </p><br>{(new SmileButton()).GetHtml(session, new PostUrl(postUrl, "smile"))}";
    }
}