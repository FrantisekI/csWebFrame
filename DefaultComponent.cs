namespace csWebFrame;

public abstract class DefaultComponent
{
    public string Name { get; set; }
    public string PathToHml { get; set; }
    
    public abstract Dictionary<string, object> GetVariables();
    
    public string getHtml()
    {
        return "";
    }
    
}

