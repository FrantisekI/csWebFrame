namespace csWebFrame;

public abstract class DefaultHtmlComponent
{
    public string Name { get; set; }
    public string? HtmlPath;
    public string? HtmlPathFromComponentRoot;
    
    public abstract Dictionary<string, object> GetVariables(UserSession session);
    public abstract string GetHtml(UserSession session, PostUrl postUrl);

    public string CreateFromHtmlFile(UserSession session, PostUrl postUrl)
    {
        Console.WriteLine(HtmlPathFromComponentRoot);
        return HtmlCreator.RenderNode(this, session, postUrl);
    }
}

public struct PostUrl
{
    public string Url;
    public int IndexFromEnd;
    public string PathToComponent;
    
    public PostUrl(string url, int indexFromEnd)
    {
        ChangeUrl(url);
        IndexFromEnd = indexFromEnd;
        PathToComponent = "";
    }

    public PostUrl(PostUrl oldUrl, string addNestedComponent)
    {
        ChangeUrl(oldUrl.Url);
        IndexFromEnd = oldUrl.IndexFromEnd;
        PathToComponent = oldUrl.PathToComponent;
        AddNestedComponent(addNestedComponent);
    }

    public void ChangeUrl(string url)
    {
        if (url.EndsWith("index")) url = url.Substring(0, url.Length - "/index".Length);
        Url = url;
    }
    public void AddNestedComponent(string name)
    {
        if (PathToComponent == "")
        {
            PathToComponent = name;
        }
        else
        {
            PathToComponent += "/" + name;
        }
    }

    public void NewComponent(string name)
    {
        PathToComponent = name;
    }

    public void IncrementIndex()
    {
        IndexFromEnd++;
    }
    public string GetUrl()
    {
        return $"{Url}&{IndexFromEnd}&{PathToComponent}";
    }
}

