using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Audio;
using MinecraftCloneSilk.UI.UiComponent;

namespace MinecraftCloneSilk.UI.Start;

internal class Home : Screen, IDisposable
{
   private StartingWindow startingWindow; 
   private Button playButton;
   private Button optionButton;
   private Button quitButton;
   
   private AudioEffect selectionEffect;
   private AudioEffect hoverEffect;
   
   private ImGuiWindowFlags windowFlags;
   
   public Home(StartingWindow startingWindow) {
      this.startingWindow = startingWindow;
      
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

   public void DrawUi() {
      bool use_work_area = true;
      ImGuiWindowFlags flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing;

      ImGuiViewportPtr viewport = ImGui.GetMainViewport();
      Vector2 buttonSize = new Vector2(viewport.WorkSize.X * 0.3f, 50);
      ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
      ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
        
      if (ImGui.Begin("StartingWindow", flags)) {
         if (playButton.Draw(new(
                viewport.WorkSize.X / 3.0f,
                viewport.WorkSize.Y / 2.0f), buttonSize)) {
            startingWindow.Play();
         }
        
         if (optionButton.Draw(new(
                viewport.WorkSize.X / 3,
                viewport.WorkSize.Y * 0.66f), buttonSize)) {
            startingWindow.Option();
         }
            
         if(quitButton.Draw(new(
               viewport.WorkSize.X / 3,
               viewport.WorkSize.Y * 0.83f), buttonSize)) startingWindow.Quit();
      }
        
      ImGui.End();
   }

   public void Dispose() {
      hoverEffect.Dispose();
      selectionEffect.Dispose();
   }
}