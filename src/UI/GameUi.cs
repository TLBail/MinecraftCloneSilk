using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.GLFW;
using Silk.NET.Input;
namespace MinecraftCloneSilk.UI;

public class GameUi : UiWindow
{
    public GameUi(Game game, Key? key) : base(game, key) {    }
    public GameUi(Game game) : this(game, null) {}

    protected override void  drawUi()
    {
        ImGui.Begin("Game");
        
        foreach (GameObject gameObject in game.gameObjects.Values) {
            if (!(gameObject is UiWindow) && ImGui.CollapsingHeader("gameObject : " + gameObject.GetType().Name)) {
                gameObject.toImGui();
                ImGui.Separator();
                
            }
        }
        
        ImGui.End();
    }
}