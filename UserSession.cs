namespace csWebFrame;
using System;
using System.Collections.Concurrent;  // For ConcurrentDictionary
using System.Threading;               // For Timer
using System.Linq;                    // For LINQ operations in cleanup
using System.Collections.Generic;     // For collections
using System.Net;                       // For HttpListener directly

/**<summary>
 * Třída pro správu uživatelské relace a jejích proměnných
 * funkce z této třídy jsou určeny čistě pro interní funkce programu, není implementován error handeling
 * pro tvorbu stránek používejte SessionVar
 * </summary>*/
public class UserSession
{
    /**<summary>
     * Vytvoří novou uživatelskou relaci
     * </summary>*/
    public UserSession(string id, DateTime lastActivity)
    {
        this.LastActivity = lastActivity;
    }
    private readonly Dictionary<Type, SessionVariableBase> _variables = new Dictionary<Type, SessionVariableBase>();
    public DateTime LastActivity { get; set; }
    /**<summary>
     * Kontroluje, zda je proměnná daného typu nastavena v relaci
     * </summary>*/
    public bool IsVarSet(Type variableKey)
    {
        LastActivity = DateTime.UtcNow;
        return _variables.ContainsKey(variableKey);
    }

    /**<summary>
     * Získá hodnotu proměnné daného typu z relace
     * </summary>*/
    public T GetVar<T>(Type variableKey)
    {
        LastActivity = DateTime.UtcNow;
        return (T)_variables[variableKey].Var;
    }

    /**<summary>
     * Vytvoří novou proměnnou v relaci
     * </summary>*/
    public void CreateVar(Type variableKey, SessionVariableBase variableHolder)
    {
        LastActivity = DateTime.UtcNow;
        _variables.Add(variableKey, variableHolder);
    }
    /**<summary>
     * Nastaví hodnotu existující proměnné v relaci
     * </summary>*/
    public void SetVar<T>(Type variableKey, T value)
    {
        LastActivity = DateTime.UtcNow;
        _variables[variableKey].Set(value);
    }
}

/**<summary>
 * Obal na Proměnné v UserSession, aby se dali uložit do datové struktury
 * </summary>*/
public class SessionVariableBase(object value)
{
    public object Var {get; private set;} = value;

    /**<summary>
     * Nastaví hodnotu proměnné
     * </summary>*/
    public void Set(object value)
    {
        Var = value;
    }
}

/**<summary>
 * Typově bezpečná třída pro práci s relačními proměnnými
 * všechny instance této třídy se stejnou UserSession odpovídají stejné proměnné, pokud
 * potřebujete třeba list, tak vytvořte SessionVar<type[]> 
 *
 * tvoří se class "Number"(UserSession s, int i) : SessionVar<int>(s, i);
 *
 * </summary>*/
public abstract class SessionVar<TVar>
{
    private readonly UserSession _session; //this should be a reference to global UserSession 

    /**<summary>
     * Vytvoří novou relační proměnnou s výchozí hodnotou
     * </summary>*/
    protected SessionVar(UserSession session, TVar defaultValue)
    {
        _session = session;
        if (!session.IsVarSet(this.GetType()))
        {
            SessionVariableBase variableHolder = new SessionVariableBase(defaultValue);
            session.CreateVar(this.GetType(), variableHolder);
        }
    }
    /**<summary>
     * Získá hodnotu proměnné z relace
     * </summary>*/
    public TVar Get()
    {
        return _session.GetVar<TVar>(this.GetType());
    }
    /**<summary>
     * Nastaví hodnotu proměnné v relaci
     * </summary>*/
    public void Set(TVar value)
    {
        _session.SetVar(this.GetType(), value);
    }
    /**
     * Z POST requestu od usera dostaneme proměnné, session proměnnou můžeme nastavit z nich, to uděláme tak,
     * že vrátíme 
    */
    public bool SetFromUserData(string key, Dictionary<string, string> userData)
    {
        if (!userData.ContainsKey(key))
        {
            Console.WriteLine($"No {key} in user data");
            return false;
        }
        TVar var;
        try
        {
            var = (TVar)Convert.ChangeType(userData[key], typeof(TVar));
        }
        catch
        {
            Console.WriteLine($"Variable {this.GetType()} is in invalid format");
            return false;
        }
        Set(var);
        return true;
    }

    /**<summary>
     * Nastaví hodnotu z uživatelských dat podle názvu typu, protože proměnné jsou stejně rozlišovány podle jejich třídy
     * 
     * </summary>*/
    public bool SetFromUserData(Dictionary<string, string> userData)
    {
        return SetFromUserData(this.GetType().Name, userData);
    }
}

/**<summary>
 * Správce relací pro webovou aplikaci
 * </summary>*/
public class SessionManager
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();
    private readonly Timer _cleanupTimer;
    private int _timeoutMinutes = 30;
    
    /**<summary>
     * Vytvoří nového správce relací s pravidelným čištěním
     * </summary>*/
    public SessionManager()
    {
        // Clean up expired sessions every 10 minutes
        _cleanupTimer = new Timer(CleanupExpiredSessions, null, 
            TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
    }
    
    /**<summary>
     * Získá existující nebo vytvoří novou relaci pro požadavek
     * </summary>*/
    public UserSession GetOrCreateSession(HttpListenerRequest request, HttpListenerResponse response)
    {
        string? sessionId = GetSessionIdFromCookie(request);
        
        if (sessionId != null && _sessions.TryGetValue(sessionId, out var session))
        {
            session.LastActivity = DateTime.UtcNow; // Simple touch
            return session;
        }
        
        return CreateNewSession(response);
    }
    
    /**<summary>
     * Vytvoří novou relaci a nastaví cookie
     * </summary>*/
    private UserSession CreateNewSession(HttpListenerResponse response)
    {
        string sessionId = Guid.NewGuid().ToString();
        var session = new UserSession(sessionId, DateTime.UtcNow);
        
        _sessions[sessionId] = session;
        SetSessionCookie(response, sessionId);
        
        return session;
    }
    /**<summary>
     * Získá ID relace z cookie v požadavku
     * </summary>*/
    private string? GetSessionIdFromCookie(HttpListenerRequest request)
    {
        var sessionCookie = request.Cookies["SessionId"];
        if (sessionCookie == null) return null;
        return sessionCookie.Value;
    }

    /**<summary>
     * Nastaví cookie s ID relace v odpovědi
     * </summary>*/
    private void SetSessionCookie(HttpListenerResponse response, string sessionId)
    {
        var cookie = new Cookie("SessionId", sessionId)
        {
            HttpOnly = true,        // Prevents JavaScript access
            Secure = false,         // Set to true if using HTTPS
            Path = "/",            // Available for entire site
            Expires = DateTime.UtcNow.AddDays(30) // Optional: set expiration
        };
    
        response.Cookies.Add(cookie);
    }
    
    /**<summary>
     * Odstraní vypršelé relace ze slovníku
     * </summary>*/
    private void CleanupExpiredSessions(object state)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-_timeoutMinutes); // 30 min timeout
        var expiredKeys = _sessions
            .Where(kvp => kvp.Value.LastActivity < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();
            
        foreach (var key in expiredKeys)
            _sessions.TryRemove(key, out _);
    }
}