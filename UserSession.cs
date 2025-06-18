namespace csWebFrame;

public class UserSession
{
    private readonly Dictionary<Type, SessionVariableBase> _variables = new Dictionary<Type, SessionVariableBase>();
    
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

public struct SessionVariableBase(object value)
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
}