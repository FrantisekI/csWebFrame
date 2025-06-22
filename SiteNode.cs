namespace csWebFrame;

/**<summary>
 * Reprezentuje uzel stromové struktury stránek aplikace, kde každý uzel může obsahovat
 * reference na další uzly dle stromové hierarchie, odkaz na nadřazený uzel, cestu k souboru
 * a dynamicky vygenerovanou stránku.
 * </summary>*/
public class SiteNode
{
    //TODO take care of visibility
    public Type? PageType; // type should be inherit from DefaultSite
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
    
    
    public abstract class Button
    {
        public string Name = nameof(Button);
        public string? Text;
        public string? Redirect;
        public string? ButtonClass;
        public string? ButtonId;
        public string? FormClass;
        public string? FormId;

        public abstract void OnClick(Dictionary<string, string> data);
    }
}
// TODO create abstract class for Layouts - so it will be initilized with child
// and than read variables from it to use them somewhere