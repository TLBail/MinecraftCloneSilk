using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.GLFW;
using Silk.NET.Input;
namespace MinecraftCloneSilk.UI;

public class GameUi : UiWindow
{
    
    private ImGuiWindowFlags windowFlags;
    
    public GameUi(Game game, Key? key) : base(game, key) {
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings;
    }
    public GameUi(Game game) : this(game, null) {}

    protected override void  DrawUi() {
        const float pad = 0.0f;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        Vector2 workSize = viewport.WorkSize;
        Vector2 windowPos, windowPosPivot;
        windowPos.X = workPos.X + pad;
        windowPos.Y = workPos.Y + pad;
        windowPosPivot.X = 0.0f;
        windowPosPivot.Y = 0.0f;
        ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
        windowFlags |= ImGuiWindowFlags.NoMove;

        if (ImGui.Begin("Game", windowFlags)) {
            foreach (GameObject gameObject in game.gameObjects.Values) {
                if (!(gameObject is UiWindow) && ImGui.CollapsingHeader("gameObject : " + gameObject.GetType().Name)) {
                    gameObject.ToImGui();
                    ImGui.Separator();
                }
            }
            
        }
        

        
        ImGui.End();
    }
}