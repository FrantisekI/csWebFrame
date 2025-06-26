using csWebFrame;
using csWebFrame.components;

namespace csWebFrame.app.login
{
    // Session variable to store the logged-in username
    public class LoggedInUser : SessionVar<string>
    {
        public LoggedInUser(UserSession session) : base(session, "")
        {
        }
    }

    // Session variable to store error messages
    public class LoginError : SessionVar<string>
    {
        public LoginError(UserSession session) : base(session, "")
        {
        }
    }

    // Login button that handles the authentication logic
    public class LoginButton : Button
    {
        private LoggedInUser loggedInUser;
        private LoginError loginError;

        public LoginButton(LoggedInUser user, LoginError error)
        {
            Name = "Login";
            loggedInUser = user;
            loginError = error;
            Redirect = null; // We'll set this dynamically in OnClick

            // Add form elements for username and password
            AddFormElement(new InputElementAtrributes(
                InputElementAtrributes.PossibleAttributes.input, "text", "username")
            {
                id = "username",
                elementClass = "login-input",
                value = ""
            });

            AddFormElement(new InputElementAtrributes(
                InputElementAtrributes.PossibleAttributes.input, "password", "password")
            {
                id = "password", 
                elementClass = "login-input",
                value = ""
            });
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            // Clear any previous error
            loginError.Set("");

            // Get username and password from form data
            if (!data.ContainsKey("username") || !data.ContainsKey("password"))
            {
                loginError.Set("Username and password are required");
                Redirect = "/login/"; // Redirect back to login page
                return;
            }

            string username = data["username"].Trim();
            string password = data["password"].Trim();

            // Simple validation: username and password must be the same and not empty
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                loginError.Set("Username and password cannot be empty");
                Redirect = "/login/"; // Redirect back to login page
                return;
            }

            if (username == password)
            {
                // Successful login - dynamically set redirect
                loggedInUser.Set(username);
                loginError.Set("");
                Redirect = "/login/loggedin"; // Redirect to success page
            }
            else
            {
                // Failed login - dynamically set redirect
                loginError.Set("Login failed: Username and password must be the same");
                Redirect = "/login/loginfailed"; // Redirect to failure page
            }
        }
    }

    // Logout button
    public class LogoutButton : Button
    {
        private LoggedInUser loggedInUser;
        private LoginError loginError;

        public LogoutButton(LoggedInUser user, LoginError error)
        {
            Name = "Logout";
            loggedInUser = user;
            loginError = error;
            // Dynamically set redirect in OnClick instead of constructor
        }

        public override void OnClick(Dictionary<string, string> data)
        {
            // Clear the logged-in user and any errors
            loggedInUser.Set("");
            loginError.Set("");
            // Dynamically set redirect after logout
            Redirect = "/login/";
        }
    }

    // Login page (renamed from Login to Index for cleaner URLs)
    public class Index : DefaultPage
    {
        private LoggedInUser loggedInUser;
        private LoginError loginError;
        private LoginButton loginButton;
        private NavigationComponent navigation;

        public Index(UserSession session) : base(session)
        {
            loggedInUser = new LoggedInUser(session);
            loginError = new LoginError(session);
            loginButton = new LoginButton(loggedInUser, loginError);
            navigation = new NavigationComponent("login");
        }

        public override Dictionary<string, object> Render()
        {
            return new Dictionary<string, object>
            {
                ["title"] = "Login Page",
                ["loginButton"] = loginButton,
                ["errorMessage"] = loginError.Get(),
                ["hasError"] = !string.IsNullOrEmpty(loginError.Get()) ? "true" : "false",
                ["navigation"] = navigation
            };
        }
    }
}