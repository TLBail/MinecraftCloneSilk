using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public abstract class UiWindow : GameObject
{

    public const Key DEFAUlT_KEY = Key.F2;
    
    private OpenGl openGl;
    protected bool disableInteractionIfCursorIsNotAvailable = true;
    private IKeyboard keyboard;
    private Key? key;
    protected bool visible;

    public UiWindow(Game game, Key? key) : base(game) {
        this.key = key;
        visible = (key == null);
        openGl = game.openGl;
        game.uiDrawables += uiPipeline;
    }

    public UiWindow() : this(Game.getInstance(), DEFAUlT_KEY) {    }

    public void uiPipeline() {
        if (key != null && keyboard.IsKeyPressed((Key)key)) visible = !visible;
        if(!visible) return;
        
        if (openGl.cursorIsNotAvailable()) {
            ImGui.BeginDisabled();
        }
        drawUi();
        if (openGl.cursorIsNotAvailable()) {
            ImGui.EndDisabled();
        }
    }

    protected abstract void drawUi();
}