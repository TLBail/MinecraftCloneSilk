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
    protected bool needMouse = true;
    
    public UiWindow(Game game, Key? key) : base(game) {
        this.key = key;
        visible = (key == null);
        openGl = game.openGl;
        game.uiDrawables += uiPipeline;
        if (key.HasValue) {
            keyboard = game.getKeyboard();
            keyboard.KeyDown += setVisible;
        }
    }

    protected virtual void setVisible(IKeyboard keyboard, Key key, int a) {
        if (key == this.key) visible = !visible;
    }

    public UiWindow() : this(Game.getInstance(), DEFAUlT_KEY) {    }

    public void uiPipeline() {
        if(!visible) return;
        
        
        if (needMouse && openGl.cursorIsNotAvailable()) {
            ImGui.BeginDisabled();
        }
        drawUi();
        if (needMouse && openGl.cursorIsNotAvailable()) {
            ImGui.EndDisabled();
        }
    }

    protected abstract void drawUi();
}