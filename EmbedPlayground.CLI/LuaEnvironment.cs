using System.ComponentModel.DataAnnotations;
using NLua;

namespace EmbedPlayground.CLI;

public interface ILuaEnv
{
    void Export(string key, object data);
    bool Remove(string key);
    void Update(Lua state);
    string GetExtraEnv();
}

public class LuaEnvironment : ILuaEnv
{
    private Dictionary<string, object> _env = new();
    private Lua lua;

    public LuaEnvironment(Lua lua)
    {
        this.lua = lua;
    }

    public void Export(string name, object data)
    {
        if (name.Contains(' '))
            throw new InvalidOperationException("Key is not valid");
        _env[name] = data;
        Update();
    }

    public bool Remove(string key) => _env.Remove(key);
    
    public string GetExtraEnv() => string.Join(", ", _env.Keys.Select(k => $"{k} = {k}"));

    public void Update(Lua state)
    {
        foreach(var pair in _env)
        {
            state[pair.Key] = pair.Value;
        }
    }

    public void Update()
    {
        Update(lua);
    }
}