using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class ConsoleUi : UiWindow
{
    private Console console;
    private readonly ImGuiWindowFlags windowFlags;
    private bool scrollToBottom;
    private readonly bool autoscrool = true;
    private IKeyboard keyboard;
    private string inputText = "";
    private bool isFocused;

    public ConsoleUi(Game game) : base(game, null) {
        base.needMouse = false;
        this.console = game.console;
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize |
                      ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoMove;
        keyboard = game.openGl.primaryKeyboard;
    }
    
    protected override unsafe void DrawUi() {
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
        ImGui.SetNextWindowBgAlpha(isFocused ? 0.35f : 0.25f); // Transparent background
        if (ImGui.Begin("Console", windowFlags)) {
            var footerHeigthToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            if (ImGui.BeginChild("ScroollingRegion", new Vector2(0, -footerHeigthToReserve), false,
                    ImGuiWindowFlags.HorizontalScrollbar)) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
                foreach (var log in console.logs) DrawLog(log);
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
            if (ImGui.InputText("", ref inputText, 255, inputTextFlags, Callback)) {
                console.ExecCommand(inputText);
                scrollToBottom = true;
                ImGui.SetNextFrameWantCaptureKeyboard(false);
                ImGui.SetNextWindowFocus();
                inputText = "";
            }else if (keyboard.IsKeyPressed(Key.T)) {
                ImGui.SetKeyboardFocusHere(-1);
            }

            if (keyboard.IsKeyPressed(Key.Escape)) {
                ImGui.SetKeyboardFocusHere();
            }
        
            isFocused = ImGui.IsWindowFocused();
            
        }
        
        ImGui.End();
    }

    private unsafe int Callback(ImGuiInputTextCallbackData* data) {
        //AddLog("cursor: %d, selection: %d-%d", data->CursorPos, data->SelectionStart, data->SelectionEnd);
        switch (data->EventFlag) {
            case ImGuiInputTextFlags.CallbackCompletion:
                string word = Marshal.PtrToStringUTF8(new IntPtr(data->Buf), data->BufTextLen);
                List<string> candidate = new List<string>();
                foreach (string command in console.commands.Keys) {
                    if (command.StartsWith(word)) {
                        candidate.Add(command);
                    }
                }
                if (candidate.Count == 0) {
                    console.Log("No match for " + word);
                }else{
                    if (candidate.Count == 1) {
                        inputText = candidate[0] + " ";
                    } else {
                        console.Log("Possible matches: ");
                        foreach (string command in candidate) {
                            console.Log("- " + command);
                        }
                    }
                }
                break;
            case ImGuiInputTextFlags.CallbackHistory: 
                break;
        }
        return 0;
    }

    private void DrawLog(Console.LogRecord logRecord) {
        switch (logRecord.logType) {
            case Console.LogType.INFO:
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                break;
            case Console.LogType.ERROR:
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                break;
            case Console.LogType.WARNING:
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
                break;
            case Console.LogType.NULL:
                ImGui.Text(logRecord.text);
                return;
        }

        ImGui.Text(logRecord.dateTime.ToLongTimeString() + " : " + logRecord.text);
        ImGui.PopStyleColor();
    }

}