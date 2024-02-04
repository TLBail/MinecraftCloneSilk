using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Audio;
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
    private AudioEffect selectionEffect;
    private AudioEffect hoverEffect;
    
    public PauseMenu(Game game) : base(game, Key.Escape) {
        flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings ;
        selectionEffect = new AudioEffect(Generated.FilePathConstants.Audio.selection_ogg);
        hoverEffect = new AudioEffect(Generated.FilePathConstants.Audio.hover_ogg);
        Button.ButtonStyle buttonStyle = new(
            new Vector4(0.2f, 0.2f, 0.2f, 1.0f), // Fond foncé
            new Vector4(0.3f, 0.3f, 0.3f, 1.0f), // Plus clair au survol
            new Vector4(0.1f, 0.1f, 0.1f, 1.0f), // Encore plus foncé lors du clic
            new Vector4(1.0f, 1.0f, 1.0f, 1.0f) // Texte blanc
        );
        Button.ButtonSound buttonSound = new(
            hoverEffect,
            selectionEffect
        );
        returnButton = new Button("Retour", buttonStyle, buttonSound);
        optionButton = new Button("Options", buttonStyle, buttonSound);
        quitButton = new Button("Quitter", buttonStyle, buttonSound);
        openGl = game.openGl;
    }
    protected override void SetVisible(IKeyboard keyboard, Key key, int a) {
        if(key != this.key) return;
        if (key == this.key) visible = !visible;
        openGl.SetCursorMode(visible ? CursorModeValue.CursorNormal : CursorModeValue.CursorDisabled);
    }



    protected override void  DrawUi() {
        bool use_work_area = true;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 buttonSize = new Vector2(viewport.WorkSize.X * 0.3f, 50);
        ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
        ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
 
        if(ImGui.Begin("Pause Menu", flags)){

            if (returnButton.Draw(new(
                    viewport.WorkSize.X / 3,
                    viewport.WorkSize.Y * (1 / 4f)), buttonSize)) {
                base.visible = false;
                openGl.SetCursorMode(CursorModeValue.CursorDisabled);
            }

            if (optionButton.Draw(new(
                    viewport.WorkSize.X / 3,
                    viewport.WorkSize.Y * (2 / 4f)), buttonSize)) {
            }

            if (quitButton.Draw(new(
                    viewport.WorkSize.X / 3,
                    viewport.WorkSize.Y * (3 / 4f)), buttonSize)) game.Stop();
            
        }
        
        ImGui.End();
    } 
    
    public override void Destroy() {
        base.Destroy();
        selectionEffect.Dispose();
        hoverEffect.Dispose();
    }
}