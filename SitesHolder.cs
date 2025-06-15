using System.Net.Mime;
using FileToData;
using System.Reflection;
using System.Reflection.Emit;


namespace csWebFrame;

public class SitesHolder
{
    SiteNode rootNode;

    public SitesHolder() // TODO: make it precompile for production
    {
        rootNode = CreateTree(AppConstants.AppDir);
    }
    
    private SiteNode CreateTree(string path) 
    {
        Dictionary<string, SiteNode> successors = new Dictionary<string, SiteNode>();
        //Console.WriteLine(path);
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
        CreateSiteObject(ref node, Path.Combine(path, "layout.cs"));
        Console.WriteLine("created {0}, {1}", path, pathToHtml);
        return node;
    }

    private SiteNode CreateLeaf(string path)
    {
        SiteNode node = new SiteNode(path, null);
        CreateSiteObject(ref node, path);
        Console.WriteLine("created {0}", path);
        return node;
    }

    private void CreateSiteObject(ref SiteNode node, string filePath) /// dostane cestu, ktera se muze jmenovat index.html
    {
        // Console.WriteLine("Creating site object for: {0}", filePath);
        string relativePath = filePath.Remove(0, AppConstants.AppDir.Length + 1); // +1 to remove the leading slash
        string namespacePath = "csWebFrame.app." + Path.GetDirectoryName(relativePath).Replace("/", ".").Replace("\\", ".");
        string className = Path.GetFileNameWithoutExtension(filePath);
        
        if (namespacePath == "csWebFrame.app.")
        {
            namespacePath = "csWebFrame.app";
        }
    
        string fullTypeName = $"{namespacePath}.{className}";
        Console.WriteLine(fullTypeName);

        var type = Assembly.GetExecutingAssembly().GetType(fullTypeName);
        if (type != null && typeof(DefaultPage).IsAssignableFrom(type))
        {
            node.page = (DefaultPage)Activator.CreateInstance(type);
            Console.WriteLine("Created class {0}", type.Name);
        }
        else if (type == null)
        {
            // Optional: Log that no matching class was found, but don't throw
            Console.WriteLine($"No class found for {fullTypeName}");
        }
        else
        {
            throw new InvalidOperationException($"Class {type.Name} must inherit from DefaultPage");
        }

    }

    public string RenderNode(SiteNode node)
    {
        if (node.path == null) return "";
        
        string pageContent = File.ReadAllText(node.path);
        if (node.page != null)
        {
            Console.WriteLine("page is not null");
            Dictionary<string, object> variables = node.page.Render();
            foreach (string key in variables.Keys)
            {
                Console.WriteLine(key);
                pageContent = pageContent.Replace($"{{{{{key}}}}}", variables[key].ToString()); 
                // double braces marge to form one ^^
            }
        }
        return pageContent;
    }

    public void RenderNode(SiteNode node, ref string child)
    {
        if (node.path == null) return;
        string parent = RenderNode(node);
        if (!parent.Contains("{{child}}"))
        {
            child = $"Layout page {node.path} does not contain {{child}}";
            return;
        }
        child = parent.Replace("{{child}}", child);
        
    }
    
    public string RenderPage(string url)
    {
        /// Dojde ke slozce, kterou chce uzivatel otevrit a overi si, jestli existuje
        int lastDot = url.LastIndexOf('.');
        if (lastDot != -1) url = url.Substring(0, lastDot);
        url = url.TrimEnd('/');
        
        string[] folders = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        SiteNode currentNode = rootNode;
        
        bool fileIsThere = true;
        foreach (string folder in folders)
        {
            Console.WriteLine("folder {0}", folder);
            Console.WriteLine(folder);
            currentNode = currentNode.GoToNext(folder);
            if (currentNode == null)
            {
                fileIsThere = false;
                break;
            }
        }
        // handle if it is folder
        if (fileIsThere)
        {
            if (currentNode._next != null)
            {
                if (currentNode._next.ContainsKey("index"))
                {
                    currentNode = currentNode._next["index"];
                }
                else
                {
                    fileIsThere = false;
                }
            }
            
        }
        Console.WriteLine("is there: " + url + " " + fileIsThere);
        /// Nejprve zavola funkce v app slozce, pak nacte strukturu stranky z .html
        /// a vymeni veci uvnitr {{...}} za hodnoty promennych
        if (fileIsThere)
        {
            string pageContent = RenderNode(currentNode);
            // string pageContent = File.ReadAllText(
            //     Path.Combine(AppConstants.AppDir, 
            //         (Path.Combine(folders) + ".html")));  // TODO: handle if class is not defined
            // if (currentNode.page != null)
            // {
            //     Console.WriteLine("page is not null");
            //     Dictionary<string, object> variables = currentNode.page.Render();
            //     foreach (string key in variables.Keys)
            //     {
            //         Console.WriteLine(key);
            //         pageContent = pageContent.Replace($"{{{{{key}}}}}", variables[key].ToString()); 
            //         // double braces marge to form one ^^
            //     }
            // }
            for (int i = folders.Length - 1; i >= 0 ; i--)
            {
                currentNode = currentNode._previous;
                RenderNode(currentNode, ref pageContent);
                // Console.WriteLine(i);
                // string pathToCurent = Path.Combine(AppConstants.RootDirectory, "app", 
                //     Path.Combine(folders.Take(i).ToArray()), 
                //     "layout.html");
                // // Console.WriteLine("layout.html exists {0}", File.Exists(pathToCurent));
                // if (File.Exists(pathToCurent))
                // {
                //     pageContent = File.ReadAllText(pathToCurent).Replace("{{child}}", pageContent);
                // }
                //
                //
                // if (currentNode != null && currentNode.page != null)
                // {
                //     Console.WriteLine("is not null");
                //     Dictionary<string, object> variables = currentNode._next["layout"].page.Render();
                //     foreach (string key in variables.Keys)
                //     {
                //         pageContent = pageContent.Replace($"{{{{{key}}}}}", variables[key].ToString()); 
                //     }
                // }
                // TODO: if .cs file exist, than run it, there is a problem that how to  
                //  destinguish between index.html/cs and layout.html/cs in the tree structure
            }
            return pageContent;
        }
        return File.ReadAllText(Path.Combine(AppConstants.RootDirectory, "app", "404.html"));
    }
}

public class SiteNode
{
    public DefaultPage? page; //TODO take care of visibility
    public Dictionary<string, SiteNode>? _next;
    public SiteNode? _previous;
    public string? path;
    // TODO: refactor to also hold the path to .html file

    public SiteNode(string? path, Dictionary<string, SiteNode>? successors)
    {
        this.path = path;
        _next = successors;
        if (_next != null) foreach (SiteNode node in _next.Values)
        {
            node._previous = this;
        }
    }

    public SiteNode? GoToNext(string name)
    {
        return _next.GetValueOrDefault(name);
    }
    
    
}

public abstract class DefaultPage
{
    public abstract Dictionary<string, object> Render();

    /*public string RenderPage(string htmlPath)
    {
        string htmlContent = File.ReadAllText(htmlPath);
        Dictionary<string, object> variables = Render();
        foreach (string key in variables.Keys)
        {
            htmlContent.Replace($"{{{key}}}", variables[key].ToString());
        }
        return htmlContent;
    }

    public string RenderPage(string htmlPath, string child)
    {
        return RenderPage(htmlPath).Replace("{{child}}", child);
    }*/
}