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
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings    | ImGuiWindowFlags.NoNav;
    }
    public GameUi(Game game) : this(game, null) {}

    protected override void  drawUi() {

        const float PAD = 0.0f;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        Vector2 workSize = viewport.WorkSize;
        Vector2 windowPos, windowPosPivot;
        windowPos.X = workPos.X + PAD;
        windowPos.Y = workPos.Y + PAD;
        windowPosPivot.X = 0.0f;
        windowPosPivot.Y = 0.0f;
        ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
        windowFlags |= ImGuiWindowFlags.NoMove;
        
        ImGui.Begin("Game", windowFlags);
        

        foreach (GameObject gameObject in game.gameObjects.Values) {
            if (!(gameObject is UiWindow) && ImGui.CollapsingHeader("gameObject : " + gameObject.GetType().Name)) {
                gameObject.toImGui();
                ImGui.Separator();
                
            }
        }
        
        ImGui.End();
    }
}