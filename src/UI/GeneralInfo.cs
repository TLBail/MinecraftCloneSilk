using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class GeneralInfo : UiWindow
{

    public GeneralInfo(Game game, Key? key) : base(game, key) {
        needMouse = false;
    }
    public GeneralInfo(Game game) : this(game, null){}
    static int corner = 1;
    
    protected override void drawUi() {

        ImGuiIOPtr io = ImGui.GetIO();
        
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings  | ImGuiWindowFlags.NoFocusOnAppearing  | ImGuiWindowFlags.NoNav;
        if (corner != -1)
        {
            const float PAD = 10.0f;
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            Vector2 workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
            Vector2 workSize = viewport.WorkSize;
            Vector2 windowPos, windowPosPivot;
            windowPos.X = (corner == 1) ? (workPos.X + workSize.X - PAD) : (workPos.X + PAD);
            windowPos.Y = (corner == 2) ? (workPos.Y + workSize.Y - PAD) : (workPos.Y + PAD);
            windowPosPivot.X = (corner == 1) ? 1.0f : 0.0f;
            windowPosPivot.Y = (corner == 2) ? 1.0f : 0.0f;
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
            windowFlags |= ImGuiWindowFlags.NoMove;
        }
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("Example: Simple overlay", windowFlags))
        {
            ImGui.Text( (1000.0f / ImGui.GetIO().Framerate).ToString("F") +  " ms/frame ( "+ ImGui.GetIO().Framerate.ToString("F1") + " FPS)" );
            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.MenuItem("Custom", null, corner == -1)) corner = -1;
                if (ImGui.MenuItem("Top-left", null, corner == 0)) corner = 0;
                if (ImGui.MenuItem("Top-right", null, corner == 1)) corner = 1;
                if (ImGui.MenuItem("Bottom-left", null, corner == 2)) corner = 2;
                if (ImGui.MenuItem("Bottom-right", null, corner == 3)) corner = 3;
                ImGui.EndPopup();
            }
        }
        ImGui.End();
            
    }
}