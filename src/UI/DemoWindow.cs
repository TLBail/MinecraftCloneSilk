using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.UI;

public class DemoWindow : GameObject
{
    
    public DemoWindow()
    {
        game.uiDrawables += UiDrawables;
    }

    private void UiDrawables()
    {
        ImGuiNET.ImGui.ShowDemoWindow();
    }
}