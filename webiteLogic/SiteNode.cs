namespace csWebFrame;

/**<summary>
 * Reprezentuje uzel stromové struktury stránek aplikace, kde každý uzel může obsahovat
 * reference na další uzly dle stromové hierarchie, odkaz na nadřazený uzel, cestu k souboru
 * a dynamicky vygenerovanou stránku.
 * </summary>*/
public class SiteNode : DefaultHtmlComponent
{
    
    /**<summary>Typ stránky, měl by dědit od DefaultSite</summary>*/
    public Type? PageType;
    /**<summary>Slovník následujících uzlů ve stromové struktuře</summary>*/
    public readonly Dictionary<string, SiteNode>? Next;
    /**<summary>Odkaz na předchozí uzel ve stromové struktuře</summary>*/
    public SiteNode? Previous;

    /**<summary>
     * Vytvoří nový uzel stránky s cestou k HTML souboru a následnými uzly
     * </summary>*/
    public SiteNode(string? htmlPath, Dictionary<string, SiteNode>? successors)
    {
        this.HtmlPath = htmlPath;
        Next = successors;
        if (Next != null) foreach (SiteNode node in Next.Values)
        {
            node.Previous = this;
        }
    }

    /**
     * <summary>
     * Přejde na následující uzel podle zadaného názvu (ve stromové struktuře uzlů má každý potomek svůj klíč
     * pokud neexistuje, vrátí null
     * </summary>
     */
    public SiteNode? GoToNext(string name)
    {
        if (Next != null) return Next.GetValueOrDefault(name);
        return null;
    }

    /**
     * <summary>
     * Získá proměnné pro šablonu stránky z příslušné třídy stránky (odvozené od DefaultComponent)
     * </summary>
     */
    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        if (PageType == null) return new Dictionary<string, object>();
        DefaultPage pageClassObject = (DefaultPage)Activator.CreateInstance(PageType, session)!;
        return pageClassObject.Render();
    }
    /**<summary>
     * Vygeneruje HTML obsah stránky, pokud neexistuje, vrátí prázdný string
     * </summary>*/
    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        if (HtmlPath == null)
        {
            Console.WriteLine($"HtmlPath is null for node {Name}");
            return "";
        }
        postUrl.NewComponent("");
        return HtmlCreator.RenderNode(this, session, postUrl);
    }
}

/**<summary>
 * Template, jak ma vypadat objekt generujici dynamicka data na stranku,
 * ten implementuje tvůrce webové stránky
 * </summary> */
public abstract class DefaultPage(UserSession session)
{
    public UserSession Session = session;
    /**<summary>
     * Generuje proměnné pro šablonu stránky
     * </summary>*/
    public abstract Dictionary<string, object> Render();
}

/**<summary>
 * Abstraktní třída pro tlačítka s možností reakce na kliknutí
 * </summary>*/
public abstract class Button : DefaultHtmlComponent
{
    /**<summary>Název tlačítka</summary>*/
    public string Name = nameof(Button);
    /**<summary>Formulářové prvky přidružené k tlačítku</summary>*/
    public InputElementAtrributes[]? formElements;
    /**<summary>URL pro přesměrování po akci</summary>*/
    public string? Redirect;
    /**<summary>CSS třída formuláře</summary>*/
    public string? FormClass;
    /**<summary>ID formuláře</summary>*/
    public string? FormId;

    /**<summary>
     * Třída reprezentující atributy HTML elementů ve formuláři
     * </summary>*/
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

        /**<summary>
         * Vytvoří nový formulářový element se základními atributy
         * </summary>*/
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
            
        /**<summary>
         * Vytvoří nový formulářový element se všemi možnými atributy
         * </summary>*/
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

        /**<summary>
         * Vygeneruje HTML kód pro daný element formuláře
         * </summary>*/
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

    /**<summary>
     * Přidá nový formulářový element k tlačítku
     * </summary>*/
    public void AddFormElement(InputElementAtrributes element)
    {
        if (formElements == null) formElements = new InputElementAtrributes[1];
        else Array.Resize(ref formElements, formElements.Length + 1);
        formElements[formElements.Length - 1] = element;
    }
    /**
     * <summary>Naimplementované vracení html stringu odpovídajícího tlačítku</summary>
     */
    public override string GetHtml(UserSession session, PostUrl postUrl)
    {
        return HtmlCreator.CreateButtonElement(this, postUrl.GetUrl());
    }
    
    /**
     * <summary>Button nemá potomky, takže vrací prozdné Dictionary</summary>
     */
    public override Dictionary<string, object> GetVariables(UserSession session)
    {
        return new Dictionary<string, object>();
    }
    
    /**<summary>
     * Abstraktní metoda volaná při kliknutí na tlačítko
     * </summary>*/
    public abstract void OnClick(Dictionary<string, string> data);
}
