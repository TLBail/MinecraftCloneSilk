using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public abstract class UiWindow : GameObject
{

    public const Key DEFAULT_KEY = Key.F2;
    
    private OpenGl openGl;
    protected bool disableInteractionIfCursorIsNotAvailable = true;
    private IKeyboard? keyboard;
    protected Key? key;
    protected bool visible;
    protected bool needMouse = true;
    public static bool keyboardUseInConsole = false;
    
    public UiWindow(Game game, Key? key) : base(game) {
        this.key = key;
        visible = (key == null);
        openGl = game.openGl;
        game.uiDrawables += UiPipeline;
        if (key.HasValue) {
            keyboard = game.GetKeyboard();
            keyboard.KeyDown += SetVisible;
        }
    }

    protected virtual void SetVisible(IKeyboard keyboard, Key key, int a) {
        if(keyboardUseInConsole) return;
        if (key == this.key) visible = !visible;
    }

    public UiWindow() : this(Game.GetInstance(), DEFAULT_KEY) {    }

    public void UiPipeline() {
        if(!visible) return;
        
        
        if (needMouse && openGl.CursorIsNotAvailable()) {
            ImGui.BeginDisabled();
        }
        DrawUi();
        if (needMouse && openGl.CursorIsNotAvailable()) {
            ImGui.EndDisabled();
        }
    }

    protected abstract void DrawUi();
}