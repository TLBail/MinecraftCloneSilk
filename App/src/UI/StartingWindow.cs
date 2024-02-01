using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class StartingWindow : UiWindow
{
    
    private ImGuiWindowFlags windowFlags;
    
    public StartingWindow(Game game, Key? key) : base(game, key) {
        windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings;
    }
    public StartingWindow(Game game) : this(game, null) {}

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
        
        ImGui.Begin("StartingWindow", flags);
        
        
        ImGui.SetCursorPosX(viewport.WorkSize.X / 3);
        ImGui.SetCursorPosY(viewport.WorkSize.Y / 2);
        if (ImGui.Button("Jouer", buttonSize)) Play();
        
        ImGui.SetCursorPosX(viewport.WorkSize.X / 3);
        ImGui.SetCursorPosY(viewport.WorkSize.Y * 0.66f);
        if (ImGui.Button("Options", buttonSize))
        {
            // Logique du bouton "Options"
        }

        ImGui.SetCursorPosX(viewport.WorkSize.X / 3);
        ImGui.SetCursorPosY(viewport.WorkSize.Y  * 0.83f);
        if (ImGui.Button("Quitter", buttonSize)) game.Stop();
        
        ImGui.End();
        // Restaurer le style
        ImGui.PopStyleColor(4);
    }

    private void Play() {
        game.console.SetUiActive(true);
        
        game.FindGameObject<DemoWindow>().Destroy();

        game.AddGameObject(new DebugRayManager(game));
        game.AddGameObject(new ChunkRendererUi(game));
        game.AddGameObject(new InventaireUi(game));
        game.AddGameObject(new ItemBarUi(game));
        game.AddGameObject(new GameUi(game));
        game.AddGameObject(new PauseMenu(game));

        game.FindGameObject<World>().Reset(new WorldNaturalGeneration(1234), WorldMode.DYNAMIC, "Worlds/newWorld");

        this.Destroy();
    }
}