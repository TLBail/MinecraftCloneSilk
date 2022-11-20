using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class Console : UiWindow
{
    public delegate void ExecCommand(string[] commandParams);
    private Dictionary<string, ExecCommand> commands;

    private List<Log> logs;

    public record Log(string text, LogType logType, DateTime dateTime);

    public enum LogType
    {
        WARNING,
        INFO,
        ERROR,
        NULL
    }

    private readonly bool autoscrool = true;
    private int historyPos = -1;
    private readonly ImGuiWindowFlags windowFlags;
    private bool autoScrool = true;
    private bool scrollToBottom;
    private IKeyboard keyboard;

    public Console(Game game) : base(game, null) {
        needMouse = false;
        commands = new Dictionary<string, ExecCommand>();
        keyboard = game.getKeyboard();
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize |
                      ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoMove;

        commands.Add("/help", (commandParams) =>
        {
            logs.Add(new Log("commands : ", LogType.NULL, DateTime.Now));
            foreach (var key in commands.Keys) logs.Add(new Log("- " + key, LogType.NULL, DateTime.Now));
        });
        commands.Add("/clear", (commandParams) => { logs.Clear(); });

        logs = new List<Log>();
        logs.Add(new Log("super Info", LogType.INFO, DateTime.Now));
        logs.Add(new Log("warning ! ", LogType.WARNING, DateTime.Now));
        logs.Add(new Log("erreur ....", LogType.ERROR, DateTime.Now));
    }

    public void addCommand(string key, ExecCommand action) => commands.Add(key, action);
    public void log(string text, LogType logType = LogType.NULL) => logs.Add(new Log(text, logType, DateTime.Now));
    
    protected override unsafe void drawUi() {
        const float PADX = 10.0f;
        const float PADY = 200.0f;
        var viewport = ImGui.GetMainViewport();
        var workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        var workSize = viewport.WorkSize;
        Vector2 windowPos, windowPosPivot;
        windowPos.X = workPos.X + PADX;
        windowPos.Y = workPos.Y + workSize.Y - PADY;
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
            foreach (var log in logs) drawLog(log);
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
            result = "";
            scrollToBottom = true;
            ImGui.CaptureKeyboardFromApp(false);
        }else if (keyboard.IsKeyPressed(Key.T)) {
            ImGui.SetKeyboardFocusHere(-1);
        }

        ImGui.End();
    }


    private void execCommand(string textCommand) {
        foreach (var keyValuePair in commands)
            if (textCommand.StartsWith(keyValuePair.Key)) {
                keyValuePair.Value?.Invoke(textCommand.Split()[1..]);
                return;
            }
        log(textCommand, LogType.INFO);
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
                    log("No match for " + word);
                }else{
                    log("Possible matches: ");
                    foreach (string command in candidate) {
                        log("- " + command);
                    }
                }
                break;
            case ImGuiInputTextFlags.CallbackHistory: 
                break;
        }
        return 0;
    }

    private void drawLog(Log log) {
        switch (log.logType) {
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
                ImGui.Text(log.text);
                return;
        }

        ImGui.Text(log.dateTime.ToLongTimeString() + " : " + log.text);
        ImGui.PopStyleColor();
    }
}