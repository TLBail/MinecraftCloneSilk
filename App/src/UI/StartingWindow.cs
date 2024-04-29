using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Audio;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Button = MinecraftCloneSilk.UI.UiComponent.Button;
namespace MinecraftCloneSilk.UI;

public class StartingWindow : UiWindow
{
    
    private ImGuiWindowFlags windowFlags;
    private Button playButton;
    private Button optionButton;
    private Button quitButton;
    private AudioEffect selectionEffect;
    private AudioEffect hoverEffect;
    
    public StartingWindow(Game game, Key? key) : base(game, key) {
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings;
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
        playButton = new Button("Jouer", buttonStyle, buttonSound);
        optionButton = new Button("Options", buttonStyle, buttonSound);
        quitButton = new Button("Quitter", buttonStyle, buttonSound);
    }
    public StartingWindow(Game game) : this(game, null) {}

    public override void Destroy() {
        base.Destroy();
        hoverEffect.Dispose();
        selectionEffect.Dispose();
    }

    protected override void  DrawUi() {
      bool use_work_area = true;
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing;

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 buttonSize = new Vector2(viewport.WorkSize.X * 0.3f, 50);
        ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
        ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
        
        
        // Définir le style des boutons
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)); // Fond foncé
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 1.0f)); // Plus clair au survol
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.1f, 0.1f, 0.1f, 1.0f)); // Encore plus foncé lors du clic
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)); // Texte blanc

        if (ImGui.Begin("StartingWindow", flags)) {
            if (playButton.Draw(new(
                    viewport.WorkSize.X / 3.0f,
                    viewport.WorkSize.Y / 2.0f), buttonSize)) {
                Play();
            }
        
            if (optionButton.Draw(new(
                    viewport.WorkSize.X / 3,
                    viewport.WorkSize.Y * 0.66f), buttonSize)) {
            }
            
            if(quitButton.Draw(new(
                    viewport.WorkSize.X / 3,
                    viewport.WorkSize.Y * 0.83f), buttonSize)) game.Stop();
            
            
        }
        
        ImGui.End();
        ImGui.PopStyleColor(4);
    }

    private void Play() {
        game.AddGameObject(new DebugRayManager(game));
        game.AddGameObject(new ConsoleUi(game));
        game.AddGameObject(new ChunkRendererUi(game));
        game.AddGameObject(new InventaireUi(game));
        game.AddGameObject(new ItemBarUi(game));
        game.AddGameObject(new GameUi(game));
        game.AddGameObject(new PauseMenu(game));
        

        game.FindGameObject<World>().Reset(new WorldNaturalGeneration(1234), WorldMode.DYNAMIC, "Worlds/newWorld");

        this.Destroy();
    }
}