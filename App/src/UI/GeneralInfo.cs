using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.UI;

public class GeneralInfo : UiWindow
{

    public GeneralInfo(Game game) : this(game, null){}
    public GeneralInfo(Game game, Key? key = null) : base(game, key) {
        needMouse = false;
    }

    static int corner = 1;
    
    protected override void DrawUi() {

        ImGuiIOPtr io = ImGui.GetIO();
        
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings  | ImGuiWindowFlags.NoFocusOnAppearing  | ImGuiWindowFlags.NoNav;
        if (corner != -1)
        {
            const float pad = 10.0f;
            ImGuiViewportPtr viewport = ImGui.GetMainViewport();
            Vector2 workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
            Vector2 workSize = viewport.WorkSize;
            Vector2 windowPos, windowPosPivot;
            windowPos.X = (corner == 1) ? (workPos.X + workSize.X - pad) : (workPos.X + pad);
            windowPos.Y = (corner == 2) ? (workPos.Y + workSize.Y - pad) : (workPos.Y + pad);
            windowPosPivot.X = (corner == 1) ? 1.0f : 0.0f;
            windowPosPivot.Y = (corner == 2) ? 1.0f : 0.0f;
            ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
            windowFlags |= ImGuiWindowFlags.NoMove;
        }
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("FPS", windowFlags))
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