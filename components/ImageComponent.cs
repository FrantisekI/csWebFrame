using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Simple image component that displays an image from a source path
    /// </summary>
    public class ImageComponent : DefaultHtmlComponent
    {
        private readonly string _imagePath;

        public ImageComponent(string imagePath)
        {
            _imagePath = imagePath;
            Name = "ImageComponent";
        }

        public override Dictionary<string, object> GetVariables(UserSession session)
        {
            return new Dictionary<string, object>();
        }

        public override string GetHtml(UserSession session, PostUrl postUrl)
        {
            return $"<img src=\"{_imagePath}\" alt=\"Image\" style=\"max-width: 100%; height: auto;\" />";
        }
    }
}
