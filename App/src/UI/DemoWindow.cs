using ImGuiNET;
using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.UI;

public class DemoWindow : UiWindow
{
    public DemoWindow(Game game): base(game, null){}
    protected override void drawUi() {
        ImGui.ShowDemoWindow();
    }
}