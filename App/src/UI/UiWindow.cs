using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public abstract class UiWindow : GameObject
{

    public const Key DEFAULT_KEY = Key.F2;
    
    private OpenGl openGl;
    private IKeyboard? keyboard;
    protected Key? key;
    protected bool visible;
    protected bool needMouse = true;
    
    
    public UiWindow(Game game, Key? key) : base(game) {
        this.key = key;
        visible = (key == null);
    }

    protected override void mStart() {
        if (key.HasValue) {
            keyboard = game.GetKeyboard();
            keyboard.KeyDown += SetVisible;
        }
        openGl = game.openGl;
        game.uiDrawables += UiPipeline;
        base.mStart();
    }

    public override void Destroy() {
        base.Destroy();
        game.uiDrawables -= UiPipeline;
        if (key.HasValue) {
            keyboard!.KeyDown -= SetVisible;
        }
    }

    protected virtual void SetVisible(IKeyboard keyboard, Key key, int a) {
        if(ImGui.GetIO().WantTextInput) return;
        if (key == this.key) visible = !visible;
    }
    public void UiPipeline() {
        if(!visible) return;
        
        
        bool disableInteraction = needMouse && openGl.CursorIsNotAvailable();
        if (disableInteraction) {
            ImGui.BeginDisabled();
        }
        DrawUi();
        if (disableInteraction) {
            ImGui.EndDisabled();
        }
    }

    protected abstract void DrawUi();
}