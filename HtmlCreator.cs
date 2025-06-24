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
    public static string RenderNode(SiteNode node, UserSession session, PostUrl postUrl)
    {
        if (node.Path == null) return "";
        
        string pageContent = File.ReadAllText(node.Path);
        if (node.PageType != null)
        {
            DefaultPage pageClassObject = (DefaultPage)Activator.CreateInstance(node.PageType, session)!;
            Dictionary<string, object> variables = pageClassObject.Render();
            foreach (string key in variables.Keys)
            {
                string value;
                if (typeof(DefaultHtmlComponent).IsAssignableFrom(variables[key].GetType()))
                {
                    PostUrl newPostUrl = postUrl;
                    newPostUrl.AddNestedComponent(key);
                    value = ((DefaultHtmlComponent)variables[key]).GetHtml(session, newPostUrl);
                }
                else
                {
                    value = variables[key].ToString();
                }

                pageContent = pageContent.Replace($"{{{{{key}}}}}", value);
                // double braces marge to form one ^^
            }
        }
        return pageContent;
    }
    /**<summary>
     * Pro slozky ktere mohou obsahovat layout.
     * Zmeni layout obsahujici child, pokud layout neexistuje, nic se nezmeni
     * </summary>*/
    public static void RenderNode(SiteNode node, UserSession session, ref string child, PostUrl postUrl)
    {
        if (node.Path == null) return;
        string parent = RenderNode(node, session, postUrl);
        if (!parent.Contains("{{child}}"))
        {
            child = $"Layout page {node.Path} does not contain {{child}}";
            return;
        }
        child = parent.Replace("{{child}}", child);
        
    }
}