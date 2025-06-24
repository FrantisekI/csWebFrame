using System.Reflection;


namespace csWebFrame;
/**<summary>
 * Drzi stromovou strukturu stranek aplikace, kdyz dostane GET request, tak skombinuje stranky v html
 * s tim co vrati napsane prislusne funkce v .cs
 * </summary>*/
public class SitesHolder
{
    private SiteNode _rootNode;
    

    public SitesHolder()
    {
        _rootNode = CreateTree(AppConstants.AppDir);
    }
    
    /**<summary>
     * Vytvari stromvou strukturu adresaru
     *
     * vrati SiteNode se strankou korespondujici ke zdejsimu layout.html/cs a jako potomky ma
     * podadresare slozky
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
     * na vstupu dostane 
     * Popravde si uplne nejsem jisty, jak funguje
     * </summary>*/
    private void CreateSiteExecutable(ref SiteNode node) /// dostane cestu, ktera se muze jmenovat index.html
    {
        if (node.Path == null) return;
        string filePath = node.Path;
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
     * handles the buttonClick a vrati redirect cestu,
     * pokud nenajde stranku, nebo se neco pokazi, vrati null, a meli bychom odpovedet 400
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
        string buttonKey = pathParts[2];
        (SiteNode? buttonContainer, _) = FindNode(pathToButton);
        if (buttonContainer == null)
        {
            Console.WriteLine("Request path not found");
            return null;
        }
        if (buttonContainer.Path == null) return "";
        
        for (int i = 0; i < indexFromEnd; i++)
        {
            buttonContainer = buttonContainer.Previous;
        }
        if (buttonContainer.PageType != null)
        {
            DefaultPage pageClassObject = (DefaultPage)Activator.CreateInstance(buttonContainer.PageType, session)!;
            Console.WriteLine("searching in" + pageClassObject.GetType().Name);
            Dictionary<string, object> variables = pageClassObject.Render();
            if (!variables.ContainsKey(buttonKey))
            {
                Console.WriteLine($"Button named {buttonKey} not found");
                return null;
            }
            object potentialButton = variables[buttonKey];
            
            if (typeof(Button).IsAssignableFrom(potentialButton.GetType()))
            {
                Button button = (Button)potentialButton;
                button.OnClick(data);
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
        int indexFromEnd = 0;
        if (currentNode != null)
        {
            string pageContent = HtmlCreator.RenderNode(currentNode, session, indexFromEnd, pagePath);
            
            while (currentNode.Previous != null)
            {
                indexFromEnd++;
                currentNode = currentNode.Previous!;
                HtmlCreator.RenderNode(currentNode, session, ref pageContent, indexFromEnd, pagePath);
            }
            return pageContent;
        }
        
        SiteNode? notFoundNode = _rootNode.GoToNext("404");
        return notFoundNode != null ? HtmlCreator.RenderNode(notFoundNode, session, 0, "/") : "404 Page Not Found";
    }

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