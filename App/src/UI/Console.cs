using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class Console 
{
    public Dictionary<string, Action<string[]>> commands { get; private set; }
    public List<LogRecord> logs;
    public record LogRecord(string text, LogType logType, DateTime dateTime);

    public enum LogType
    {
        WARNING,
        INFO,
        ERROR,
        NULL
    }


    public Console(Game game) {
        commands = new Dictionary<string, Action<string[]>>();
        logs = new List<LogRecord>();
        commands.Add("/help", (commandParams) =>
        {
            Log("commands : ");
            foreach (var key in commands.Keys) logs.Add(new LogRecord("- " + key, LogType.NULL, DateTime.Now));
        });
        commands.Add("/clear", (commandParams) => { logs.Clear(); });
        commands.Add("/gameObjects", (commandParams) =>
        {
            Log("gameobjects  : ");
            foreach (var pair in game.gameObjects) {
                Log(@"- " + pair.Key + " : " + pair.Value);
            }
        });
    }
    public void AddCommand(string key, Action<string[]> action) => commands.Add(key, action);
    public void Log(string text, LogType logType = LogType.NULL) => logs.Add(new LogRecord(text, logType, DateTime.Now));
    public void ExecCommand(string textCommand) {
        foreach (var keyValuePair in commands)
            if (textCommand.StartsWith(keyValuePair.Key)) {
                try {
                    keyValuePair.Value?.Invoke(textCommand.Split()[1..]);
                }
                catch (Exception e) {
                    Log(e.Message, LogType.ERROR);
                }
                return;
            }
        Log(textCommand, LogType.INFO);
    }
    public void RemoveCommand(string s) {
        commands.Remove(s);
    }
}