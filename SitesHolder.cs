using FileToData;

namespace csWebFrame;

public class SitesHolder
{
    SiteNode rootNode;

    public SitesHolder() // TODO: make it precompile for production
    {
        rootNode = new SiteNode(Path.Combine(AppConstants.RootDirectory, "app"), null);
    }

    public string RenderPage(string url)
    {
        /// Dojde ke slozce, kterou chce uzivatel otevrit a overi si, jestli existuje
        string[] folders = url.Split('/');
        SiteNode currentNode = rootNode;
        bool fileIsThere = true;
        foreach (string folder in folders)
        {
            currentNode = currentNode.GoToNext(folder);
            if (currentNode == null)
            {
                fileIsThere = false;
                break;
            }
        }
        
        /// Nejprve zavola funkce v app slozce, pak nacte strukturu stranky z .html
        /// a vymeni veci uvnitr {{...}} za hodnoty promennych
        if (fileIsThere)
        {
            string pageContent = File.ReadAllText(
                Path.Combine(AppConstants.RootDirectory, "app", 
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
    public DefaultPage? page;
    private Dictionary<string, SiteNode>? _next;
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