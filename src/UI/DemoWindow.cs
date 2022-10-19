using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.UI;

public class DemoWindow
{
    public DemoWindow()
    {
        Game.getInstance().uiDrawables += UiDrawables;
    }

    private void UiDrawables()
    {
        ImGuiNET.ImGui.ShowDemoWindow();
    }
}