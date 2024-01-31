using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class Console : UiWindow
{
    private Dictionary<string, Action<string[]>> commands;
    private List<LogRecord> logs;
    public record LogRecord(string text, LogType logType, DateTime dateTime);

    public enum LogType
    {
        WARNING,
        INFO,
        ERROR,
        NULL
    }

    private readonly bool autoscrool = true;
    private readonly ImGuiWindowFlags windowFlags;
    private bool scrollToBottom;
    private IKeyboard keyboard;
    public bool isUiActive { get; private set; }

    public Console(Game game) : base(game, null) {
        base.console = this;
        needMouse = false;
        commands = new Dictionary<string, Action<string[]>>();
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize |
                      ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoMove;
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
    protected override unsafe void DrawUi() {
        if(isUiActive) ConsoleUi();
    }

    public void SetUiActive(bool active) {
        if (active) {
            keyboard = game.GetKeyboard();
        }
        isUiActive = active;
    }

    private unsafe void ConsoleUi() {
        const float padx = 10.0f;
        const float pady = 200.0f;
        var viewport = ImGui.GetMainViewport();
        var workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        var workSize = viewport.WorkSize;
        Vector2 windowPos, windowPosPivot;
        windowPos.X = workPos.X + padx;
        windowPos.Y = workPos.Y + workSize.Y - pady;
        windowPosPivot.X = 0.0f;
        windowPosPivot.Y = 1.0f;
        ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
        ImGui.SetNextWindowSize(new Vector2(500, 400));
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        ImGui.Begin("Console", windowFlags);
        var footerHeigthToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        if (ImGui.BeginChild("ScroollingRegion", new Vector2(0, -footerHeigthToReserve), false,
                ImGuiWindowFlags.HorizontalScrollbar)) {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
            foreach (var log in logs) DrawLog(log);
            if (scrollToBottom || (autoscrool && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())) {
                ImGui.SetScrollHereY(1.0f);
            }

            scrollToBottom = false;
            ImGui.PopStyleVar();
        }

        ImGui.EndChild();
        ImGui.Separator();
        var inputTextFlags = ImGuiInputTextFlags.EnterReturnsTrue |
                             ImGuiInputTextFlags.CallbackCompletion |
                             ImGuiInputTextFlags.CallbackHistory;
        var result = "";
        if (ImGui.InputText("", ref result, 255, inputTextFlags, Callback)) {
            execCommand(result);
            scrollToBottom = true;
            ImGui.SetNextFrameWantCaptureKeyboard(false);
            ImGui.SetNextWindowFocus();
        }else if (keyboard.IsKeyPressed(Key.T)) {
            ImGui.SetKeyboardFocusHere(-1);
            
        }
        keyboardUseInConsole = ImGui.IsWindowFocused();
        
        ImGui.End();
    }



    private void execCommand(string textCommand) {
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

    private unsafe int Callback(ImGuiInputTextCallbackData* data) {
        //AddLog("cursor: %d, selection: %d-%d", data->CursorPos, data->SelectionStart, data->SelectionEnd);
        switch (data->EventFlag) {
            case ImGuiInputTextFlags.CallbackCompletion:
                string word = Marshal.PtrToStringUTF8(new IntPtr(data->Buf), data->BufTextLen);
                List<string> candidate = new List<string>();
                foreach (string command in commands.Keys) {
                    if (command.StartsWith(word)) {
                        candidate.Add(command);
                    }
                }
                if (candidate.Count == 0) {
                    Log("No match for " + word);
                }else{
                    Log("Possible matches: ");
                    foreach (string command in candidate) {
                        Log("- " + command);
                    }
                }
                break;
            case ImGuiInputTextFlags.CallbackHistory: 
                break;
        }
        return 0;
    }

    private void DrawLog(LogRecord logRecord) {
        switch (logRecord.logType) {
            case LogType.INFO:
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                break;
            case LogType.ERROR:
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                break;
            case LogType.WARNING:
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                break;
            case LogType.NULL:
                ImGui.Text(logRecord.text);
                return;
        }

        ImGui.Text(logRecord.dateTime.ToLongTimeString() + " : " + logRecord.text);
        ImGui.PopStyleColor();
    }

    public void RemoveCommand(string s) {
        commands.Remove(s);
    }
}