using csWebFrame;

namespace csWebFrame.app.login
{
    // Logged in success page
    public class Loggedin : DefaultPage
    {
        private LoggedInUser loggedInUser;
        private LogoutButton logoutButton;

        public Loggedin(UserSession session) : base(session)
        {
            loggedInUser = new LoggedInUser(session);
            var loginError = new LoginError(session);
            logoutButton = new LogoutButton(loggedInUser, loginError);
        }

        public override Dictionary<string, object> Render()
        {
            string username = loggedInUser.Get();
            
            // Show username even if empty - let the page handle display
            return new Dictionary<string, object>
            {
                ["title"] = "Welcome!",
                ["username"] = !string.IsNullOrEmpty(username) ? username : "Guest",
                ["logoutButton"] = logoutButton
            };
        }
    }
}