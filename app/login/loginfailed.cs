using csWebFrame;

namespace csWebFrame.app.login
{
    // Login failed page
    public class Loginfailed : DefaultPage
    {
        private LoginError loginError;

        public Loginfailed(UserSession session) : base(session)
        {
            loginError = new LoginError(session);
        }

        public override Dictionary<string, object> Render()
        {
            string errorMessage = loginError.Get();
            
            return new Dictionary<string, object>
            {
                ["title"] = "Login Failed",
                ["errorMessage"] = !string.IsNullOrEmpty(errorMessage) ? errorMessage : "Login failed. Please try again.",
                ["hasError"] = "true"
            };
        }
    }
}