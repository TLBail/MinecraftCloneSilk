using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Button = MinecraftCloneSilk.UI.UiComponent.Button;

namespace MinecraftCloneSilk.UI;

public class PauseMenu : UiWindow
{
    private ImGuiWindowFlags flags;
    private Button returnButton;
    private Button optionButton;
    private Button quitButton;
    private OpenGl openGl;
    
    

    
    public PauseMenu(Game game) : base(game, Key.Escape) {
        flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings ;
        Button.ButtonStyle buttonStyle = new(
            new Vector4(0.2f, 0.2f, 0.2f, 1.0f), // Fond foncé
            new Vector4(0.3f, 0.3f, 0.3f, 1.0f), // Plus clair au survol
            new Vector4(0.1f, 0.1f, 0.1f, 1.0f), // Encore plus foncé lors du clic
            new Vector4(1.0f, 1.0f, 1.0f, 1.0f) // Texte blanc
        );
        returnButton = new Button("Retour", buttonStyle);
        optionButton = new Button("Options", buttonStyle);
        quitButton = new Button("Quitter", buttonStyle);
        openGl = game.openGl;
    }
    protected override void SetVisible(IKeyboard keyboard, Key key, int a) {
        if(key != this.key) return;
        base.SetVisible(keyboard, key, a);
        openGl.SetCursorMode(visible ? CursorModeValue.CursorNormal : CursorModeValue.CursorDisabled);
    }

    protected override void  DrawUi() {
        bool use_work_area = true;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 buttonSize = new Vector2(viewport.WorkSize.X * 0.3f, 50);
        ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
        ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
 
        ImGui.Begin("Pause Menu", flags);

        if (returnButton.Draw(new(
                viewport.WorkSize.X / 3,
                viewport.WorkSize.Y * (1 / 4f)), buttonSize)) {
            base.visible = false;
            openGl.SetCursorMode(CursorModeValue.CursorDisabled);
        }
        
        if(optionButton.Draw(new(
        viewport.WorkSize.X / 3,
        viewport.WorkSize.Y * (2/4f)), buttonSize)){}
        
        if(quitButton.Draw(new (
        viewport.WorkSize.X / 3,
        viewport.WorkSize.Y * (3/4f)), buttonSize)) game.Stop();
        
        ImGui.End();
    } 
}