using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Example SessionVar for storing text values
    /// </summary>
    public class Text : SessionVar<string>
    {
        public Text(UserSession s, string defaultText) : base(s, defaultText)
        {
        }
    }
}
