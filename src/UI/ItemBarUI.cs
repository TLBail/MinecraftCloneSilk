using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class ItemBarUi : UiWindow
{
    private List<string> blockNames;
    private IMouse mouse;
    private int index = 0;
    private ImGuiIOPtr imGuiIo;
    private static bool p_open;
    
    public ItemBarUi(Game game) : base(game, null) {
        blockNames = new List<string>();
        mouse = game.getMouse();
        
    }

    protected override void start() {
        foreach (var keyValue in BlockFactory.getInstance().blocksReadOnly) {
            blockNames.Add(keyValue.Value.name);
        }
        mouse.Scroll += MouseOnScroll;
        imGuiIo = ImGui.GetIO();
    }

    private void MouseOnScroll(IMouse mouse, ScrollWheel scrollWheel) {
        index = (int)(scrollWheel.Y % blockNames.Count);
    }

    protected override void drawUi() {
        ImGuiWindowFlags windowFlags  = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing| ImGuiWindowFlags.NoNav;
        const float PAD = 10.0f;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 work_pos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        Vector2 work_size = viewport.WorkSize;
        Vector2 window_pos, window_pos_pivot;
        window_pos.X = (work_pos.X + work_size.X) * 0.75f ;
        window_pos.Y = (work_pos.Y + work_size.Y - PAD);
        window_pos_pivot.X = 1.0f;// : 0.0f;
        window_pos_pivot.Y = 1.0f; // : 0.0f;
        ImGui.SetNextWindowPos(window_pos, ImGuiCond.Always, window_pos_pivot);
        windowFlags |= ImGuiWindowFlags.NoMove;
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("item bar", ref p_open, windowFlags)) {
            for (int i = 0; i < blockNames.Count; i++) {
                itemUi(i);
            }
            ImGui.End();
        }

    }

    private void itemUi(int index) {
        ImGui.PushID(index);
        
        
    }
}