namespace csWebFrame;

public class HtmlCreator
{
    public static string CreateButtonElement(Button button, string action)
    {
        string html = $"<form action=\"{action}\" class=\"{button.FormClass}\" id=\"{button.FormId}\" method=\"POST\">\n";
        bool wasThereSubmitButton = false;
        if (button.formElements != null) foreach (Button.InputElementAtrributes element in button.formElements)
        {
            html += element.GetHtmlElement();
            if (element.attribute == Button.InputElementAtrributes.PossibleAttributes.button &&
                element.type == "submit")
            {
                wasThereSubmitButton = true;
            }
        }

        if (!wasThereSubmitButton)
        {
            html += $"<button type=\"submit\">{button.Name}</button>\n";
        }
        html += "</form>\n";
        return html;
        
    }

    /**<summary>
     * Vrati stranku jako string - nacte html a v nem nahradi {{var}} promennymi co vygeneruje
     * prislusny obekt
     * </summary>*/
    public static string RenderNode(DefaultHtmlComponent node, UserSession session, PostUrl postUrl)
    {
        string? htmlPath = node.HtmlPath;
        if (node.HtmlPathFromComponentRoot != null)
        {
            htmlPath = Path.Combine(AppConstants.RootDirectory, "components", node.HtmlPathFromComponentRoot);
        }

        Console.WriteLine(htmlPath);
        if (htmlPath == null)
        {
            Console.WriteLine($"there is no path to html in {node.GetType().Name}");
        }
        string pageContent = File.ReadAllText(htmlPath);
        
        Dictionary<string, object> variables = node.GetVariables(session);
        foreach (string key in variables.Keys)
        {
            string value;
            if (typeof(DefaultHtmlComponent).IsAssignableFrom(variables[key].GetType()))
            {
                PostUrl newPostUrl = postUrl;
                newPostUrl.AddNestedComponent(key);
                Console.WriteLine("KEY " + key);
                Console.WriteLine(((DefaultHtmlComponent)variables[key]).GetVariables(session));
                value = ((DefaultHtmlComponent)variables[key]).GetHtml(session, newPostUrl);
            }
            else
            {
                value = variables[key]?.ToString() ?? "";
            }

            pageContent = pageContent.Replace($"{{{{{key}}}}}", value);
            // double braces marge to form one ^^
        }
        
        return pageContent;
    }
    /**<summary>
     * Pro slozky ktere mohou obsahovat layout.
     * Zmeni layout obsahujici child, pokud layout neexistuje, nic se nezmeni
     * muze byt pouzit pouze na SiteNode
     * </summary>*/
    public static void RenderNode(SiteNode node, UserSession session, ref string child, PostUrl postUrl)
    {
        if (node.HtmlPath == null) return;
        string parent = RenderNode(node, session, postUrl);
        if (!parent.Contains("{{child}}"))
        {
            child = $"Layout page {node.HtmlPath} does not contain {{child}}";
            return;
        }
        child = parent.Replace("{{child}}", child);
        
    }
}