namespace csWebFrame;

public abstract class DynamicHtmlComponent()
{
    
    public abstract string GetHtml(UserSession session, string openedPath, int indexFromEnd, string pathToComponent);
    public abstract Dictionary<string, object> GetVariables();


}



/**<summary>
 * Reprezentuje uzel stromové struktury stránek aplikace, kde každý uzel může obsahovat
 * reference na další uzly dle stromové hierarchie, odkaz na nadřazený uzel, cestu k souboru
 * a dynamicky vygenerovanou stránku.
 * </summary>*/
public class SiteNode : DefaultHtmlComponent
{
    //TODO take care of visibility
    public Type? PageType; // type should be inherit from DefaultSite
    public readonly Dictionary<string, SiteNode>? Next;
    public SiteNode? Previous;

    public SiteNode(string? htmlPath, Dictionary<string, SiteNode>? successors)
    {
        this.HtmlPath = htmlPath;
        Next = successors;
        if (Next != null) foreach (SiteNode node in Next.Values)
        {
            node.Previous = this;
        }
    }

    public SiteNode? GoToNext(string name)
    {
        if (Next != null) return Next.GetValueOrDefault(name);
        return null;
    }

    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        if (PageType == null) return new Dictionary<string, object>();
        DefaultPage pageClassObject = (DefaultPage)Activator.CreateInstance(PageType, session)!;
        return pageClassObject.Render();
    }
    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        if (HtmlPath == null) return "";
        postUrl.NewComponent("");
        return HtmlCreator.RenderNode(this, session, postUrl);
    }
}

/**<summary>
 * Template, jak ma vypadat objekt generujici dynamicka data na stranku
 * </summary> */
public abstract class DefaultPage(UserSession session)
{
    public UserSession Session = session;
    public abstract Dictionary<string, object> Render();
}

public abstract class Button : DefaultHtmlComponent
{
    public string Name = nameof(Button);
    public InputElementAtrributes[]? formElements;
    public string? Redirect;
    public string? FormClass;
    public string? FormId;

    public class InputElementAtrributes
    {
        public enum PossibleAttributes {input, label, select, option, textarea, button, div}
        public PossibleAttributes attribute;
        public string name;
        public string value;
        public string type;
        public string id;
        public string elementClass;
        public string style;
        public string? labelIsFor;
        public string? textInside; // if text is in form before the childs:
        public  InputElementAtrributes[]? childs;

        public InputElementAtrributes(PossibleAttributes attribute, string type, string name)
        {
            this.attribute = attribute;
            this.name = name;
            this.value = "";
            this.type = type;
            this.id = "";
            this.elementClass = "";
            this.style = "";
            this.textInside = null;
            this.labelIsFor = null;
            this.childs = null;
        }
            
        public InputElementAtrributes(PossibleAttributes attribute, string name, 
            string value, string type, string id, 
            string elementClass, string style, string? textInside, 
            string? labelIsFor, InputElementAtrributes[]? childs)
        {
            this.attribute = attribute;
            this.name = name;
            this.value = value;
            this.type = type;
            this.id = id;
            this.elementClass = elementClass;
            this.style = style;
            this.textInside = textInside;
        }

        public string GetHtmlElement()
        {
            if (attribute == PossibleAttributes.input)
            {
                return $"<input type=\"{type}\" name=\"{name}\" value=\"{value}\" id=\"{id}\" class=\"{elementClass}\" style=\"{style}\">";
            }
                
            string htmlElement = attribute.ToString();
            string startTag = $"<{htmlElement} name=\"{name}\" value=\"{value}\" type=\"{type}\" id=\"{id}\" class=\"{elementClass}\" style=\"{style}\"" +
                              ((attribute == PossibleAttributes.label) ? $"for=\"{labelIsFor}\">\n" : ">\n");
            string endTag = "</" + htmlElement + ">\n";
            if (textInside != null)
            {
                startTag += textInside + "\n";
            }

            if (childs != null)
            {
                foreach (InputElementAtrributes child in childs)
                {
                    startTag += child.GetHtmlElement();
                }
            }
            return startTag + endTag;
                
        }
    }

    public void AddFormElement(InputElementAtrributes element)
    {
        if (formElements == null) formElements = new InputElementAtrributes[1];
        else Array.Resize(ref formElements, formElements.Length + 1);
        formElements[formElements.Length - 1] = element;
    }

    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        return HtmlCreator.CreateButtonElement(this, postUrl.GetUrl());
    }
    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        return new Dictionary<string, object>();
    }
    public abstract void OnClick(Dictionary<string, string> data);
}
// TODO create abstract class for Layouts - so it will be initilized with child
// and than read variables from it to use them somewhere