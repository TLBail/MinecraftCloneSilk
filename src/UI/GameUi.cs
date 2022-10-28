using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class GameUi : GameObject
{
    private World world;
    private Key? key;
    private IKeyboard keyboard;
    private bool visible;
    
    public GameUi(Game game, Key? key) : base(game)
    {
        this.key = key;
        game.uiDrawables += UiDrawables;
        keyboard = game.getKeyboard();
        visible = (key == null);
    }
    public GameUi(Game game) : this(game, null) {}

    
    private void UiDrawables()
    {
        if (key != null && keyboard.IsKeyPressed((Key)key)) visible = !visible;
        if(!visible) return;
        
        
        ImGui.Begin("Game");

        
        foreach (GameObject gameObject in game.gameObjects.Values) {
            if (ImGui.CollapsingHeader("gameObject : " + gameObject.GetType().Name)) {
                gameObject.toImGui();
                ImGui.Separator();
                
            }
        }
        
        
        ImGui.End();
    }
}