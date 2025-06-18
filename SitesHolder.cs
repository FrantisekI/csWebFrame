using System.Reflection;


namespace csWebFrame;
/**<summary>
 * Drzi stromovou strukturu stranek aplikace, kdyz dostane GET request, tak skombinuje stranky v html
 * s tim co vrati napsane prislusne funkce v .cs
 * </summary>*/
public class SitesHolder
{
    private SiteNode _rootNode;

    public SitesHolder() // TODO: make it precompile for production
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
        CreateSiteObject(ref node);
        return node;
    }
    /**<summary>
     * Podobne, jako CreateTree, ale pouziva se na soubory, ne slozky nema tedy zadne potomky
     * </summary>*/
    private SiteNode CreateLeaf(string path)
    {
        SiteNode node = new SiteNode(path, null);
        CreateSiteObject(ref node);
        Console.WriteLine("created {0}", path);
        return node;
    }
    
    /**<summary>
     * na vstupu dostane 
     * Popravde si uplne nejsem jisty, jak funguje
     * </summary>*/
    private void CreateSiteObject(ref SiteNode node) /// dostane cestu, ktera se muze jmenovat index.html
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
            node.Page = (DefaultPage)Activator.CreateInstance(type)!;
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
        if (node.Page != null)
        {
            Dictionary<string, object> variables = node.Page.Render();
            foreach (string key in variables.Keys)
            {
                pageContent = pageContent.Replace($"{{{{{key}}}}}", variables[key].ToString()); 
                // double braces marge to form one ^^
            }
        }
        return pageContent;
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
    public string RenderPage(string url)
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
        Console.WriteLine($"is there: {url} {currentNode != null}");
        /// Nejprve zavola funkce v app slozce, pak nacte strukturu stranky z .html
        /// a vymeni veci uvnitr {{...}} za hodnoty promennych
        if (currentNode != null)
        {
            string pageContent = RenderNode(currentNode);
            
            for (int i = folders.Length - 1; i >= 0 ; i--)
            {
                currentNode = currentNode.Previous!;
                RenderNode(currentNode, ref pageContent);
            }
            return pageContent;
        }
        
        SiteNode? notFoundNode = _rootNode.GoToNext("404");
        return notFoundNode != null ? RenderNode(notFoundNode) : "404 Page Not Found";
    }
}
/**<summary>
 * Reprezentuje uzel stromové struktury stránek aplikace, kde každý uzel může obsahovat
 * reference na další uzly dle stromové hierarchie, odkaz na nadřazený uzel, cestu k souboru
 * a dynamicky vygenerovanou stránku.
 * </summary>*/
public class SiteNode
{
    public DefaultPage? Page; //TODO take care of visibility
    public readonly Dictionary<string, SiteNode>? Next;
    public SiteNode? Previous;
    public readonly string? Path;

    public SiteNode(string? path, Dictionary<string, SiteNode>? successors)
    {
        this.Path = path;
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
}

/**<summary>
 * Template, jak ma vypadat objekt generujici dynamicka data na stranku
 * </summary> */
public abstract class DefaultPage(UserSession session)
{
    public UserSession Session = session;
    public abstract Dictionary<string, object> Render();
}