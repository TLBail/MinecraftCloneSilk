using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using ImGuiNET;
using MinecraftCloneSilk.Audio;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.UI.UiComponent;

namespace MinecraftCloneSilk.UI.Start;

internal class Options : Screen, IDisposable
{
   private StartingWindow startingWindow; 
   private Button returnButton;
   
   private AudioEffect selectionEffect;
   private AudioEffect hoverEffect;
   
   private ImGuiWindowFlags windowFlags;
   
   public Options(StartingWindow startingWindow) {
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
      
      returnButton = new Button("Retour", buttonStyle, buttonSound);
   }

   public unsafe void DrawUi() {
      bool use_work_area = true;
      ImGuiWindowFlags flags =  ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing;

      ImGuiViewportPtr viewport = ImGui.GetMainViewport();
      Vector2 buttonSize = new Vector2(viewport.WorkSize.X * 0.3f, 50);
      ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
      ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
        
      if (ImGui.Begin("StartingWindow", flags)) {
         ImGui.Text("Options");
         ImGui.Separator();
         ImGui.SetCursorPosY(50);
         ImGuiTabBarFlags tab_bar_flags = ImGuiTabBarFlags.NoCloseWithMiddleMouseButton;
         if (ImGui.BeginTabBar("##tabs", tab_bar_flags))
         {
            if (ImGuiPlus.BeginTabItemNoClose("World", ImGuiTabItemFlags.None)) {
               if (ImGui.Button("random")) {
                  startingWindow.config.seed = Random.Shared.Next(int.MinValue, int.MaxValue);
               }
               ImGui.InputInt("Seed", ref startingWindow.config.seed);
               
               WorldMode[] worldModes = (WorldMode[])Enum.GetValues(typeof(WorldMode));
               if(ImGui.BeginCombo("worldMode",startingWindow.config.worldMode.ToString())) {
                  for (int n = 0; n < worldModes.Length; n++)
                  {
                     bool isSelected = (startingWindow.config.worldMode.ToString() == worldModes[n].ToString());
                     if (ImGui.Selectable(worldModes[n].ToString(), isSelected)) {
                        startingWindow.config.worldMode = Enum.Parse<WorldMode>(worldModes[n].ToString());                    
                     }
                     if (isSelected)
                        ImGui.SetItemDefaultFocus();   // Set the initial focus when opening the combo (scrolling + for keyboard navigation support in the upcoming navigation branch)
                  }
                  ImGui.EndCombo();
               }
               
               ImGui.EndTabItem();
            }

            if (ImGuiPlus.BeginTabItemNoClose("Save", ImGuiTabItemFlags.None)) {
               ImGui.Checkbox("Save the world", ref startingWindow.config.saveTheWorld);
               if (startingWindow.config.saveTheWorld) {
                  ImGui.InputText("Save Path", ref startingWindow.config.savePath, 255);
               }
               ImGui.EndTabItem();
            }

            if (ImGuiPlus.BeginTabItemNoClose("Rendering", ImGuiTabItemFlags.None)) {
               ImGui.DragInt("Render Distance", ref startingWindow.config.renderDistance, 1);
               ImGui.EndTabItem();
            }


            ImGui.EndTabBar();
         }
 
         if(returnButton.Draw(new(
               viewport.WorkSize.X / 3,
               viewport.WorkSize.Y * 0.83f), buttonSize)) startingWindow.RetourHome();
      }
        
      ImGui.End();
   }
   
 


   public void Dispose() {
      hoverEffect.Dispose();
      selectionEffect.Dispose();
   }
 
}