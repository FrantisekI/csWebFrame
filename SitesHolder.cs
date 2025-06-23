using System.Reflection;


namespace csWebFrame;
/**<summary>
 * Drzi stromovou strukturu stranek aplikace, kdyz dostane GET request, tak skombinuje stranky v html
 * s tim co vrati napsane prislusne funkce v .cs
 * </summary>*/
public class SitesHolder
{
    private SiteNode _rootNode;
    private UserSession _session; //TODO: make some Sessions holder - to have multiple users (it should be
    // passed as a argument to Render function

    public SitesHolder() // TODO: make it precompile for production
    {
        _session = new UserSession();
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
     * Vrati stranku jako string - nacte html a v nem nahradi {{var}} promennymi co vygeneruje
     * prislusny obekt
     * </summary>*/
    private string RenderNode(SiteNode node)
    {
        if (node.Path == null) return "";
        
        string pageContent = File.ReadAllText(node.Path);
        if (node.PageType != null)
        {
            DefaultPage pageClassObject = (DefaultPage)Activator.CreateInstance(node.PageType, _session)!;
            Dictionary<string, object> variables = pageClassObject.Render();
            foreach (string key in variables.Keys)
            {
                if (typeof(DefaultPage.Button).IsAssignableFrom(variables[key].GetType()))
                {
                    Console.WriteLine("we are on page {0} and there is a button", node.Path);
                }
                else
                {
                    pageContent = pageContent.Replace($"{{{{{key}}}}}", variables[key].ToString());
                    // double braces marge to form one ^^
                }
            }
        }
        return pageContent;
    }

    private string CreateButtonElement(DefaultPage.Button button)
    {
        return "<form ?action=\"{buttonIdentifier}\" method=\"POST\" ?class=\"\" ?id=\"\" >" +
            "<button ?class=\"\" ?id=\"\" ></button></form>";
    }
    
    /**<summary>
     * handles the buttonClick a vrati redirect cestu,
     * pokud nenajde stranku, nebo se neco pokazi, vrati null, a meli bychom odpovedet 400
     */
    public string? PostRequest(string url, Dictionary<string, string> data)
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
        SiteNode? buttonContainer = FindNode(pathToButton);
        if (buttonContainer == null)
        {
            Console.WriteLine("Request path not found");
            return null;
        }
        if (buttonContainer.Path == null) return "";
        
        if (buttonContainer.PageType != null)
        {
            for (int i = 0; i < indexFromEnd; i++)
            {
                buttonContainer = buttonContainer.Previous;
            }
            DefaultPage pageClassObject = (DefaultPage)Activator.CreateInstance(buttonContainer.PageType, _session)!;
            Console.WriteLine(pageClassObject.GetType().Name);
            Dictionary<string, object> variables = pageClassObject.Render();
            object potentialButton = variables[buttonKey];
            
            if (typeof(DefaultPage.Button).IsAssignableFrom(potentialButton.GetType()))
            {
                DefaultPage.Button button = (DefaultPage.Button)potentialButton;
                button.OnClick(data);
                if (button.Redirect == null) return pathToButton;
                return button.Redirect;
            }
            
        }
        Console.WriteLine("Pressed Button has not been found.");
        return null;
    }
    
    /**<summary>
     * Pro slozky ktere mohou obsahovat layout.
     * Zmeni layout obsahujici child, pokud layout neexistuje, nic se nezmeni 
     * </summary>*/
    private void RenderNode(SiteNode node, ref string child)
    {
        if (node.Path == null) return;
        string parent = RenderNode(node);
        if (!parent.Contains("{{child}}"))
        {
            child = $"Layout page {node.Path} does not contain {{child}}";
            return;
        }
        child = parent.Replace("{{child}}", child);
        
    }
    
    /**<summary>
     * Podiva se, jestli chtena stranka je definovana, pokud ne vrati stranku 404
     * jinak nacte koncovou stranku a postupne pridava layouty z otcovskych adresaru
     * 
     * stranku vrati jako string
     * </summary>*/
    public string? RenderPage(string url)
    {
        
        SiteNode? currentNode = FindNode(url);
        
        /// Nejprve zavola funkce v app slozce, pak nacte strukturu stranky z .html
        /// a vymeni veci uvnitr {{...}} za hodnoty promennych
        if (currentNode != null)
        {
            string pageContent = RenderNode(currentNode);
            
            while (currentNode.Previous != null)
            {
                currentNode = currentNode.Previous!;
                RenderNode(currentNode, ref pageContent);
            }
            return pageContent;
        }
        
        SiteNode? notFoundNode = _rootNode.GoToNext("404");
        return notFoundNode != null ? RenderNode(notFoundNode) : "404 Page Not Found";
    }

    public SiteNode? FindNode(string url)
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
            if (currentNode.Next != null)
            {
                if (currentNode.Next.ContainsKey("index"))
                {
                    currentNode = currentNode.Next["index"];
                }
                
            }
            
        }
        return currentNode;
    }
}