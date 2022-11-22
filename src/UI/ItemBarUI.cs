using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class ItemBarUi : UiWindow
{
    private string[] blockNames;
    private IMouse mouse;
    private int activeIndex = 0;
    private ImGuiIOPtr imGuiIo;
    private static bool p_open;
    
    public ItemBarUi(Game game) : base(game, null) {
        mouse = game.getMouse();
        
    }

    protected override void start() {
        blockNames = new string[BlockFactory.getInstance().blocksReadOnly.Values.Count];
        int index = 0;
        foreach (var keyValue in BlockFactory.getInstance().blocksReadOnly) {
            blockNames[index] = keyValue.Value.name;
            index++;
        }
        mouse.Scroll += MouseOnScroll;
        imGuiIo = ImGui.GetIO();
    }

    private void MouseOnScroll(IMouse mouse, ScrollWheel scrollWheel) {
        activeIndex = (int)(scrollWheel.Y % blockNames.Length);
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
            for (int i = 0; i < blockNames.Length; i++) {
                itemUi(i);
            }
            ImGui.End();
        }

    }

    private unsafe void itemUi(int index) {
        ImGui.PushID(index);

        if (activeIndex == index) {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f,0.6f, 0.6f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.7f,0.7f, 0.7f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.7f,0.8f, 0.8f, 1.0f));

        }
        
        ImGui.SameLine();
        
        ImGui.BeginGroup();

        ImGui.Button(blockNames[index], new Vector2(80, 20));
        
        ImGui.EndGroup();


        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None | ImGuiDragDropFlags.SourceAllowNullID)) {
            int* ptrPayload = (&index);
            if (*ptrPayload != index) throw new Exception("sa marche pas");
            ImGui.SetDragDropPayload("DND_DEMO_CELL", (IntPtr)(&index), sizeof(int));
            ImGui.Text(blockNames[index]);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget()) {
            ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("DND_DEMO_CELL");
            
            if (payload.NativePtr != null && payload.DataSize == sizeof(int)) {
                int* newIndex = (int*)payload.Data;
                string tmp = blockNames[index];
                blockNames[index] = blockNames[*newIndex];
                blockNames[*newIndex] = tmp;
            }
            ImGui.EndDragDropTarget();
        }

        if (index == activeIndex) {
            ImGui.PopStyleColor(3);
        }
        ImGui.PopID();
    }
}