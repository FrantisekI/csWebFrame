using System.Reflection;


namespace csWebFrame;
/**<summary>
 * Drzi stromovou strukturu stranek aplikace, kdyz dostane GET request, tak vrátí odpovídající html
 * každý vrchol ve stromu odpovídá html stránce, nebo jejímu "obalu"
 *
 * Na POST request vvyřeší opět logiku unvitř 
 * </summary>*/
public class SitesHolder
{
    private SiteNode _rootNode;
    

    public SitesHolder()
    {
        _rootNode = CreateTree(AppConstants.AppDir);
    }
    
    /**<summary>
     * Rekurzivně vytvari stromvou strukturu adresářů
     *
     * vrati SiteNode se strankou korespondujici ke zdejsimu layout.html/cs a jako potomky ma
     * podadresare slozky
     *
     * tvorba se provádí jednou při inicializování projektu
     * </summary>*/
    private SiteNode CreateTree(string path)
    {
        Dictionary<string, SiteNode> successors = new Dictionary<string, SiteNode>();
        
        foreach (string folder in Directory.GetDirectories(path))
        {
            successors.Add(Path.GetFileNameWithoutExtension(Path.GetFileName(folder)),
                CreateTree(folder));
        }

        foreach (string file in Directory.GetFiles(path, "*.html"))
        {
            if (file.EndsWith("layout.html")) continue;
            successors.Add(Path.GetFileNameWithoutExtension(Path.GetFileName(file)),
                CreateLeaf(file));
        }

        string? pathToHtml = Path.Exists(Path.Combine(path, "layout.html")) ?
            Path.Combine(path, "layout.html") : null;
        
        SiteNode node = new SiteNode(pathToHtml, successors);
        CreateSiteExecutable(ref node);
        return node;
    }
    /**<summary>
     * Podobne, jako CreateTree, ale pouziva se na soubory, ne slozky nema tedy zadne potomky
     * </summary>*/
    private SiteNode CreateLeaf(string path)
    {
        SiteNode node = new SiteNode(path, null);
        CreateSiteExecutable(ref node);
        Console.WriteLine("created {0}", path);
        return node;
    }
    
    /**<summary>
     * zařídí, že SiteNode bude ukazovat na zkompilovanou třídu odvozenou od DefaultPage,
     * od ní se pak za běhu vytváří instance (nevím úplně jak to funguje)
     * </summary>*/
    private void CreateSiteExecutable(ref SiteNode node) /// dostane cestu, ktera se muze jmenovat index.html
    {
        if (node.HtmlPath == null) return;
        string filePath = node.HtmlPath;
        string relativePath = filePath.Remove(0, AppConstants.AppDir.Length + 1); // +1 to remove the leading slash
        string namespacePath = "csWebFrame.app." + Path.GetDirectoryName(relativePath)!.Replace("/", ".").Replace("\\", ".");
        string className = Path.GetFileNameWithoutExtension(filePath);
        string pascalCaseClassName = className.Substring(0, 1).ToUpper() + className.Substring(1);
        if (namespacePath == "csWebFrame.app.")
        {
            namespacePath = "csWebFrame.app";
        }
    
        string fullTypeName = $"{namespacePath}.{className}";

        var type = Assembly.GetExecutingAssembly().GetType(fullTypeName);
        if (type == null)
        {
            // Try with PascalCase class name if normal doesn't exist
            string pascalCaseFullTypeName = $"{namespacePath}.{pascalCaseClassName}";
            type = Assembly.GetExecutingAssembly().GetType(pascalCaseFullTypeName);
        }

        if (type != null && typeof(DefaultPage).IsAssignableFrom(type))
        {
            node.PageType = type;
        }
        else if (type == null)
        {
            // Optional: Log that no matching class was found, but don't throw
            Console.WriteLine($"No class found for {fullTypeName} or {namespacePath}.{pascalCaseClassName}");
        }
        else
        {
            throw new InvalidOperationException($"Class {type.Name} must inherit from DefaultPage");
        }

    }



    /**<summary>
     * Reaguje na POST request, interně zpracuje POST data a pak vrátí redirect správu
     * url - je hlavička post Requestu, (to co tvoří PostUrl struct), podle toho se pozná, komu daný
     * request patří
     * data - jsou předzpracovaná data z POST body
     *
     * pokud se něco pokazí, vrátí null 
     */
    public string? PostRequest(string url, Dictionary<string, string> data, UserSession session)
    {
        string[] pathParts = url.Split('&');
        if (pathParts.Length != 3)
        {
            Console.WriteLine("Bad POST request path");
            return null;
        }
        
        string strIndex = pathParts[1];
        int indexFromEnd = int.TryParse(strIndex, out int indexParsed) ? indexParsed : -1;
        if (indexFromEnd < 0 || indexFromEnd >= pathParts.Length)
        {
            Console.WriteLine("Bad index in POST request path");
            return null;
        }

        string pathToButton = pathParts[0];
        string[] componentKey = pathParts[2].Split("/"); 
        int len = componentKey.Length;
        (SiteNode? buttonContainer, _) = FindNode(pathToButton);
        if (buttonContainer == null)
        {
            Console.WriteLine("Request path not found");
            return null;
        }
        if (buttonContainer.HtmlPath == null) return "";
        
        for (int i = 0; i < indexFromEnd; i++)
        {
            buttonContainer = buttonContainer.Previous;
        }

        if (buttonContainer == null)
        {
            Console.WriteLine("Requested path not found");
            return null;
        }
        
        
        object currentComponent = buttonContainer;
        for (int i = 0; i < len; i++)
        {
            
            Dictionary<string, object> variables = ((DefaultHtmlComponent)currentComponent).GetVariables(session);
            
            if (!variables.ContainsKey(componentKey[i]))
            {
                Console.WriteLine($"Button named {componentKey[i]} not found");
                return null;
            }
            currentComponent = variables[componentKey[i]];
        }
        

        if (typeof(Button).IsAssignableFrom(currentComponent.GetType()))
        {
            Button button = (Button)currentComponent;
            button.OnClick(data);
            Console.WriteLine("Pressed Button " + button.Redirect);
            if (button.Redirect == null)
            {
                if (pathToButton.EndsWith("index"))
                {
                    return pathToButton.Substring(0, pathToButton.Length - "index".Length);
                }
                return pathToButton;
            }
            return button.Redirect;
        }

        Console.WriteLine("Pressed Button has not been found.");
        return null;
    }

    

    /**<summary>
     * Podiva se, jestli chtena stranka je definovana, pokud ne vrati stranku 404
     * jinak nacte koncovou stranku a postupne pridava layouty z otcovskych adresaru
     * 
     * stranku vrati jako string
     * </summary>*/
    public string? RenderPage(string url, UserSession session)
    {
        (SiteNode? currentNode, string pagePath) = FindNode(url);
        
        /// Nejprve zavola funkce v app slozce, pak nacte strukturu stranky z .html
        /// a vymeni veci uvnitr {{...}} za hodnoty promennych
        if (currentNode != null)
        {
            PostUrl postUrl = new PostUrl(pagePath, 0);   
            string pageContent = HtmlCreator.RenderNode(currentNode, session, postUrl);
            
            while (currentNode.Previous != null)
            {
                currentNode = currentNode.Previous!;
                postUrl.IncrementIndex();
                HtmlCreator.RenderNode(currentNode, session, ref pageContent, postUrl);
            }
            return pageContent;
        }
        
        SiteNode? notFoundNode = _rootNode.GoToNext("404");
        return notFoundNode != null ? HtmlCreator.RenderNode(notFoundNode, session, new PostUrl("/", 0)) : "404 Page Not Found";
    }
    
    /**
     * <summary>Pomocná funkce, která dostane na vstupu hledané url od uživatele, upraví ho, od zbytečných lomítek a
     * přebívajících cest - tuto cestu poté také vrátí
     *
     * pokud hledaná stránka existuje, vrátí její objekt
     * </summary>
     */
    public (SiteNode?, string) FindNode(string url)
    {
        /// Dojde ke slozce, kterou chce uzivatel otevrit a overi si, jestli existuje
        int lastDot = url.LastIndexOf('.');
        if (lastDot != -1) url = url.Substring(0, lastDot);
        url = url.TrimEnd('/');
        
        string[] folders = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        SiteNode? currentNode = _rootNode;
        
        //bool fileIsThere = true;
        foreach (string folder in folders)
        {
            currentNode = currentNode.GoToNext(folder);
            if (currentNode == null)
            {
                //fileIsThere = false;
                break;
            }
        }
        // handle if it is a folder
        if (currentNode != null)
        {
            
            if (currentNode.Next != null && currentNode.Next.ContainsKey("index"))
            {
                url += "/index";
                currentNode = currentNode.Next["index"];
            }
            
        }

        return (currentNode, url);
    }
}