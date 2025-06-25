Obecne: komponenta

```cs
/**
 * <summary> Každá komponenta je od tohoto odvozená,
 * požaduje se, aby měla 2 funkce: GetVariables a GetHtml
 * pokud chcete generovat html doplňováním do souboru, nastavte na něj cestu do HtmlPathFromComponentRoot
 * .html soubor uložte do složky components a cesta bude relativně vůči složce components
 * tedy [projectDir]/components/index.html odpovídá HtmlPathFromComponentRoot = "index.html"
 * </summary>
 */
public abstract class DefaultHtmlComponent
{
    public string Name { get; set; }
    public string? HtmlPath;
    public string? HtmlPathFromComponentRoot;
    /**
     * <summary>
     * Dostane UserSession ze které se dají získávat proměnné a má za úkol vrátit proměnné,
     * které se budou dosazovat do výsledného html, (pokud negenerujete html pomocí funkce
     * CreateFromHtmlFile, tak stačí vracet jen instance objektů, které odpovídají dětím
     * této komponenty)
     *
     * formát vrácení:
     * <code><![CDATA[
     * return new Dictionary<string,object>()
     * {
     *      ["klic"] = (new SessionVar<TVar>).Get() // zadání promenne primou hodnotou
     *      ["klic2"] = (new DefaultHtmlComponent) // pokud obsahuje vnitrni komponentu vrat ji jako objekt
     *      // aby se dala vytvorit spravne POST cesta u Button
     * };
     * ]]></code>
     * </summary>
     */
    public abstract Dictionary<string, object> GetVariables(UserSession session);
    
    /**
     * <summary>
     * Vrátí html reprezentaci této komponenty, proměnné si můžete brát ze session,
     * pokud stránku generujete sami, tak každé podkomponentě je potřeba přidat do
     * PostUrl jeho klíč, pro správné uložení POST cesty tedy do textu doplnit:
     * potomek.GetHtml(session, new PostUrl(postUrl, (odpovídající klíč z GetVariables))
     *
     * Pokud chcete html vygenerovat dosazováním do souboru, tak můžete zavolat CreateFromHtmlFile,
     * která se o PostUrl postará sama
     * </summary>
     */
    public abstract string GetHtml(UserSession session, PostUrl postUrl);

    public string CreateFromHtmlFile(UserSession session, PostUrl postUrl)
    {
        return HtmlCreator.RenderNode(this, session, postUrl);
    }
}
```

stranka ulozena v app directory

```cs
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
```

tlacítko

```cs
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
```

Promenne:

```cs
/**<summary>
 * Typově bezpečná třída pro práci s relačními proměnnými
 * všechny instance této třídy se stejnou UserSession odpovídají stejné proměnné, pokud
 * potřebujete třeba list, tak vytvořte SessionVar<type[]> 
 *
 * tvoří se class "Number"(UserSession s, int i) : SessionVar<int>(s, i);
 *
 * </summary>*/
public abstract class SessionVar<TVar>
{
    private readonly UserSession _session; //this should be a reference to global UserSession 

    /**<summary>
     * Vytvoří novou relační proměnnou s výchozí hodnotou
     * </summary>*/
    protected SessionVar(UserSession session, TVar defaultValue)
    {
        _session = session;
        if (!session.IsVarSet(this.GetType()))
        {
            SessionVariableBase variableHolder = new SessionVariableBase(defaultValue);
            session.CreateVar(this.GetType(), variableHolder);
        }
    }
    /**<summary>
     * Získá hodnotu proměnné z relace
     * </summary>*/
    public TVar Get()
    {
        return _session.GetVar<TVar>(this.GetType());
    }
    /**<summary>
     * Nastaví hodnotu proměnné v relaci
     * </summary>*/
    public void Set(TVar value)
    {
        _session.SetVar(this.GetType(), value);
    }
    /**
     * Z POST requestu od usera dostaneme proměnné, session proměnnou můžeme nastavit z nich, to uděláme tak,
     * že vrátíme 
    */
    public bool SetFromUserData(string key, Dictionary<string, string> userData)
    {
        if (!userData.ContainsKey(key))
        {
            Console.WriteLine($"No {key} in user data");
            return false;
        }
        TVar var;
        try
        {
            var = (TVar)Convert.ChangeType(userData[key], typeof(TVar));
        }
        catch
        {
            Console.WriteLine($"Variable {this.GetType()} is in invalid format");
            return false;
        }
        Set(var);
        return true;
    }

    /**<summary>
     * Nastaví hodnotu z uživatelských dat podle názvu typu, protože proměnné jsou stejně rozlišovány podle jejich třídy
     * 
     * </summary>*/
    public bool SetFromUserData(Dictionary<string, string> userData)
    {
        return SetFromUserData(this.GetType().Name, userData);
    }
}
```