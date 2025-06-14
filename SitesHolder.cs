using System.Net.Mime;
using FileToData;
using System.Reflection;


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
        Console.WriteLine(path);
        foreach (string folder in Directory.GetDirectories(path))
        {
            successors.Add(Path.GetFileName(folder), CreateTree(folder));
        }

        foreach (string file in Directory.GetFiles(path, "*.html"))
        {
            successors.Add(Path.GetFileNameWithoutExtension(Path.GetFileName(file)),
                CreateLeaf(file));
        }
        
        return new SiteNode(Path.Combine(path, "index.html"), successors);
    }

    private SiteNode CreateLeaf(string path)
    {
        SiteNode node = new SiteNode(path, null);
        CreateSiteObject(ref node, path);
        return node;
    }

    private void CreateSiteObject(ref SiteNode node, string filePath) /// dostane cestu, ktera se muze jmenovat index.html
    {
        Console.WriteLine("Creating site object for: {0}", filePath);
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

    public string RenderPage(string url)
    {
        /// Dojde ke slozce, kterou chce uzivatel otevrit a overi si, jestli existuje
        url = Path.GetFileNameWithoutExtension(url);
        string[] folders = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        SiteNode currentNode = rootNode;
        
        bool fileIsThere = true;
        foreach (string folder in folders)
        {
            foreach (string key in rootNode._next.Keys)
            {
                Console.WriteLine(key);
            }

            Console.WriteLine(folder);
            currentNode = currentNode.GoToNext(folder);
            if (currentNode == null)
            {
                fileIsThere = false;
                break;
            }
        }

        Console.WriteLine("is there: " + url + fileIsThere);
        /// Nejprve zavola funkce v app slozce, pak nacte strukturu stranky z .html
        /// a vymeni veci uvnitr {{...}} za hodnoty promennych
        if (fileIsThere)
        {
            
            string pageContent = File.ReadAllText(
                Path.Combine(AppConstants.AppDir, 
                    (Path.Combine(folders) + ".html")));  // TODO: handle if class is not defined
            
            Dictionary<string, object> variables = currentNode.page.Render();
            foreach (string key in variables.Keys)
            {
                pageContent.Replace($"{{{key}}}", variables[key].ToString());
            }
            
            for (int i = folders.Length - 2; i >= 0 ; i++)
            {
                string pathToCurent = Path.Combine(AppConstants.RootDirectory, "app", Path.Combine((string[])folders.Take(i)), "layout.html");
                if (File.Exists(pathToCurent))
                {
                    pageContent = File.ReadAllText(pathToCurent).Replace("{{child}}", pageContent);
                }
                
                //TODO: if .cs file exist, than run it, there is a problem that how to  
                //  destinguish between index.html/cs and layou.html/cs in the tree structure
            }
        }
        return File.ReadAllText(Path.Combine(AppConstants.RootDirectory, "app", "404.html"));
    }
}

public class SiteNode
{
    public DefaultPage? page; //TODO take care of visibility
    public Dictionary<string, SiteNode>? _next;
    private SiteNode? _previous;

    public SiteNode(string path, Dictionary<string, SiteNode>? successors)
    {
        _next = successors;
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