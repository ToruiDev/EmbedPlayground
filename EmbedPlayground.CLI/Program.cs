// See https://aka.ms/new-console-template for more information

using EmbedPlayground.CLI;
using KeraLua;
using Lua = NLua.Lua;
using LuaFunction = NLua.LuaFunction;

var interpreter = new LuaInterpreter()
{
	Source = """
		for i=0,10,1
		do
			log('hello world ')
		end 
	"""
};
var res = await interpreter.ExecuteAsync();
Console.WriteLine(res);
return;

Console.WriteLine("Hello, World!");

	
using Lua state = new Lua ();

state["x"] = 27;
state.DoString(@"y = 10 + x*(5 + 2)");

var ev = new EventDict();
state["events"] = ev;

state.SetDebugHook(LuaHookMask.Line, 0);
state.DebugHook += (sender, eventArgs) =>
{
	if (eventArgs.LuaDebug.CurrentLine < 1) return;
	
	var l = sender as Lua;
	l?.State?.Error("Execution manually aborted");
};

state.DoString("""
	function ScriptFunc (val1)
		events:Debug(val1)
	end
""");

state.DoString("events:On('init', ScriptFunc)");



ev.events["init"].Call("hello world");

Console.WriteLine("MathResult: " + state["y"]);

class EventDict
{
	public Dictionary<string, LuaFunction> events = new();
	
	public void On(string eventName, LuaFunction func)
	{
		events[eventName] = func;
	}

	public void Debug(string s)
	{
		Console.WriteLine("Lua:>" + s);
	}
}