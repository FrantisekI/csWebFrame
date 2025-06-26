using csWebFrame;

namespace csWebFrame.components
{
    /// <summary>
    /// Example button that handles text submission
    /// </summary>
    public class SendText : Button
    {
        private UserSession _session;

        public SendText(UserSession session)
        {
            _session = session;
            Name = "SendText";
            
            // Add form elements for text input
            formElements = new InputElementAtrributes[]
            {
                new InputElementAtrributes(InputElementAtrributes.PossibleAttributes.input, "text", "textInput"),
                new InputElementAtrributes(InputElementAtrributes.PossibleAttributes.button, "submit", "")
                {
                    textInside = "Send Text"
                }
            };
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            Text text = new Text(_session, "Hello World");
            text.SetFromUserData(data);
        }
    }
}
