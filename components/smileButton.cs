namespace csWebFrame.components;

public class SendSmile : Button
{
    string Name = "Smile";
    public override void OnClick(Dictionary<string, string> data)
    {
        Console.WriteLine("pressed the SMILE button");
    }
}


public class SmileButton : DefaultHtmlComponent
{

    public SmileButton()
    {
        HtmlPathFromComponentRoot = "smile.html";
    }
    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        return new Dictionary<string, object>()
        {
            ["button_for_smile"] = new SendSmile(),
        };
    }

    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        Console.WriteLine("gettting html for smile button");
        return CreateFromHtmlFile(session, postUrl);
    }
}