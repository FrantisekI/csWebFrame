using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Animal component that displays animal information with an image
    /// </summary>
    public class ZvireComponent : DefaultHtmlComponent
    {
        private readonly string text;

        /// <summary>
        /// Constructor for ZvireComponent
        /// </summary>
        /// <param name="animalName">Name of the animal to display</param>
        public ZvireComponent(string animalName)
        {
            text = animalName;
            Name = "ZvireComponent";
        }

        /// <summary>
        /// Returns the variables for this component
        /// </summary>
        /// <param name="session">User session</param>
        /// <returns>Dictionary of variables</returns>
        public override Dictionary<string, object> GetVariables(UserSession session)
        {
            var obrazek = new ImageComponent($"zviratka/{text}.svg");
            return new Dictionary<string, object>
            {
                // obdobně, jako u Render() u DefaultPage
                ["obrazek"] = obrazek,
                ["text"] = text
            };
        }

        /// <summary>
        /// Returns HTML representation of this component
        /// </summary>
        /// <param name="session">User session</param>
        /// <param name="postUrl">Post URL for nested components</param>
        /// <returns>HTML string</returns>
        public override string GetHtml(UserSession session, PostUrl postUrl)
        {
            var obrazek = new ImageComponent($"zviratka/{text}.svg");
            return $"<div class=\"zvire-component\"><u>{text}</u><br>{obrazek.GetHtml(session, new PostUrl(postUrl, "obrazek"))}</div>";
        }
    }
}
