namespace csWebFrame;
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

/**
 * <summary>Slouží pro vytvoření Form action argumentu u Komponent odvozených od Button,
 * je potřeba dát si pozor na změny tohoto při běhu programu - obsahuje cestu k právě otevřené
 * stránce, na jaké stránce se nachází komponenta obsahující tlačítko (to program zajišťuje sám)
 * ale pokud předáváte PostUrl potomkovi, vnitřní komponentě, měli bysete do PostUrl přidat
 * odpovídající klíč z GetVariables
 *
 * podle této url bude pak vypadat hlavička POST requestu poslaná tlačítkem
 * </summary>
 */
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
    /**
     * <summary>Slouží ke snazšímu přidání komponenty na konec cesty k ní
     * </summary>
     */
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
    
    /**
     * <summary>Přidá vnořenou komponentu
     * cesta ke komponentám by neměla začínat / (o to se funkce postará)
     * </summary>
     */
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
    
    /**
     * <summary>Vrátí odkaz, 
     * </summary>
     */
    public string GetUrl()
    {
        return $"{Url}&{IndexFromEnd}&{PathToComponent}";
    }
}

