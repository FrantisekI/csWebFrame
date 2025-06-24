namespace csWebFrame;
using System;
using System.Collections.Concurrent;  // For ConcurrentDictionary
using System.Threading;               // For Timer
using System.Linq;                    // For LINQ operations in cleanup
using System.Collections.Generic;     // For collections
using System.Net;                       // For HttpListener directly

public class UserSession
{
    public UserSession(string id, DateTime lastActivity)
    {
        
    }
    private readonly Dictionary<Type, SessionVariableBase> _variables = new Dictionary<Type, SessionVariableBase>();
    public DateTime LastActivity { get; set; }
    public bool IsVarSet(Type variableKey)
    {
        return _variables.ContainsKey(variableKey);
    }

    public T GetVar<T>(Type variableKey)
    {
        return (T)_variables[variableKey].Var;
    }

    public void CreateVar(Type variableKey, SessionVariableBase variableHolder)
    {
        _variables.Add(variableKey, variableHolder);
    }
    public void SetVar<T>(Type variableKey, T value)
    {
        _variables[variableKey].Set(value);
    }
}

public class SessionVariableBase(object value)
{
    public object Var {get; private set;} = value;

    public void Set(object value)
    {
        Var = value;
    }
}

public abstract class SessionVar<TVar>
{
    private readonly UserSession _session; //this should be a reference to global UserSession 
    
    public SessionVar(UserSession session, TVar defaultValue)
    {
        _session = session;
        if (!session.IsVarSet(this.GetType()))
        {
            SessionVariableBase variableHolder = new SessionVariableBase(defaultValue);
            session.CreateVar(this.GetType(), variableHolder);
        }
    }
    public TVar Get()
    {
        return _session.GetVar<TVar>(this.GetType());
    }
    public void Set(TVar value)
    {
        _session.SetVar(this.GetType(), value);
    }
    /**
     * Pori posilani POST requestu od usera dostane funkce post data, ty preda teto funkci
     * a pokud v datech je promenna a ve spravnem formatu, vrati true a nastavi tuto hodnotu
     * jinak jen vrati false
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

    public bool SetFromUserData(Dictionary<string, string> userData)
    {
        return SetFromUserData(this.GetType().Name, userData);
    }
}

public class SessionManager
{
    private readonly ConcurrentDictionary<string, UserSession> _sessions = new();
    private readonly Timer _cleanupTimer;
    private int _timeoutMinutes = 30;
    
    public SessionManager()
    {
        // Clean up expired sessions every 10 minutes
        _cleanupTimer = new Timer(CleanupExpiredSessions, null, 
            TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
    }
    
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
    
    private UserSession CreateNewSession(HttpListenerResponse response)
    {
        string sessionId = Guid.NewGuid().ToString();
        var session = new UserSession(sessionId, DateTime.UtcNow);
        
        _sessions[sessionId] = session;
        SetSessionCookie(response, sessionId);
        
        return session;
    }
    private string? GetSessionIdFromCookie(HttpListenerRequest request)
    {
        var sessionCookie = request.Cookies["SessionId"];
        if (sessionCookie == null) return null;
        return sessionCookie.Value;
    }

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