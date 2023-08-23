using NLua;
using NLua.Event;
using NLua.Exceptions;

namespace EmbedPlayground.CLI;

public class LuaInterpreter : IDisposable
{
    public string Name { get; set; } = $"anonymous@{DateTime.Now}";
    public string Source { get; set; } = String.Empty;
    public bool IsRunning { get; } = false;
    private Lua _state;
    private CancellationToken? _currentToken;
    
    public LuaInterpreter()
    {
        _state = new Lua();
        _state.DebugHook += StateOnDebugHook;
        
        
        var baseScript = File.ReadAllText("./Sandbox.lua");
        _state["log"] = this.Debug;
        //_state.RegisterFunction("log", typeof(LuaInterpreter).GetMethod(nameof(Debug)));
        var sandbox = _state.DoString(baseScript, "BaseScript");
    }

    private void StateOnDebugHook(object? sender, DebugHookEventArgs e)
    {
        if (_currentToken is not { IsCancellationRequested: true }) return;
        
        var l = sender as Lua;
        l?.State?.Error("Execution cancelled");
    }

    public Task<(object? ok, object? result)> ExecuteAsync(CancellationToken token = default)
    {
        _currentToken = token;
        return Task.Run(Execute, token);
    }

    private (object? ok, object? result) Execute()
    {
        return ExecuteSandboxedWithErrorHandling(this.Source, this.Name);
    }

    private (object? ok, object? result) ExecuteSandboxedWithErrorHandling(string code, string name)
    {
        lock (_state)
        {
            try
            {
                _state["__code"] = code;

                #if DUMP_VARS
                var ex = _state.DoString("print(__code)", "debug_print");
                var globals = _state.DoString("""
for k,v in pairs(_G) do
    print("Global key", k, "value", v)
end
""", "globals");
                
                var locals = _state.DoString("""
local i = 0
repeat
    local k, v = debug.getlocal(1, i)
    if k then
        print(k, v)
        i = i + 1
    end
until nil == k
""", "locals");
                #endif
                var quota = false;
                var env = "log = log";
                var tmp = _state.DoString($"local ok, result = pcall(sandbox.run, __code, {{quota={quota}, env = {{ {env} }}}})", Name);
                var (ok, result) = (_state["ok"], _state["result"]);
                return (ok, result);
            }
            catch (LuaScriptException lse)
            {
                Console.Error.WriteLine(lse);
            }
        }

        return (null, null);
    }

    public void Debug(string pattern, params object[] args)
    {
        Console.Error.WriteLine($"{Name}:" + pattern, args);
    }
    
    public void Dispose()
    {
        _state.Dispose();
    }
}

