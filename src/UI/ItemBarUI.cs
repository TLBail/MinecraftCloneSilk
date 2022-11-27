using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class ItemBarUi : UiWindow
{
    private IMouse mouse;
    private ImGuiIOPtr imGuiIo;
    private static bool p_open;
    private const string DNDCELL = "DND_CELL";
    private Texture texture;
    private Texture[] textures;
    private Player player;
    private Inventaire inventaire;
    
    public ItemBarUi(Game game) : base(game, null) {
        mouse = game.getMouse();
    }

    protected override void start() {
        var dic = BlockFactory.getInstance().blocksReadOnly;
        texture = new Texture(game.getGL(), "./Assets/blocks/stone.png");
        textures = new Texture[dic.Count - 1];
        int index = 0;
        foreach (var keyValue in dic) {
        if(keyValue.Value.name.Equals(BlockFactory.AIR_BLOCK)) continue;
        //     blockNames[index] = keyValue.Value.name;
            textures[index] = new Texture(game.getGL(),"./Assets/blocks/" + keyValue.Value.name + ".png");
            index++;
        }
        
        mouse.Scroll += MouseOnScroll;
        imGuiIo = ImGui.GetIO();
        player = (Player)game.gameObjects[typeof(Player).FullName];
        inventaire = player.inventaire;
    }

    private void MouseOnScroll(IMouse mouse, ScrollWheel scrollWheel) {
        inventaire.moveActiveIndexByScroolOffset(scrollWheel.Y);
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
            for (int i = Inventaire.STARTING_ITEM_BAR_INDEX; i <= Inventaire.ENDING_ITEM_BAR_INDEX; i++) {
                itemUi(i);
            }
            ImGui.End();
        }

    }

    private unsafe void itemUi(int index) {
        string blockName = "";
        if (inventaire.inventoryBlocks[index] != null) blockName = inventaire.inventoryBlocks[index].block.name;
        
        ImGui.PushID(index);
        
        if (inventaire.activeIndex == index) {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.7f,0.6f, 0.6f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.7f,0.7f, 0.7f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.7f,0.8f, 0.8f, 1.0f));

        }
        
        ImGui.SameLine();
        
        ImGui.BeginGroup();

        //image
        if (blockName.Length > 0) {
            if(inventaire.inventoryBlocks[index]!.block.fullTexture != null)
                ImGui.ImageButton((IntPtr)inventaire.inventoryBlocks[index]!.block.fullTexture._handle,
                    new Vector2(100, 100));
        } else {
            ImGui.Button(blockName, new Vector2(100, 100));
        }

        ImGui.EndGroup();


        if (blockName.Length > 0 &&  ImGui.BeginDragDropSource(ImGuiDragDropFlags.None | ImGuiDragDropFlags.SourceAllowNullID)) {
            ImGui.SetDragDropPayload(DNDCELL, (IntPtr)(&index), sizeof(int));
            ImGui.Text(blockName);
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget()) {
            ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload(DNDCELL);
            
            if (payload.NativePtr != null && payload.DataSize == sizeof(int)) {
                int* newIndex = (int*)payload.Data;
                (inventaire.inventoryBlocks[index], inventaire.inventoryBlocks[*newIndex]) = (inventaire.inventoryBlocks[*newIndex], inventaire.inventoryBlocks[index]);
            }
            ImGui.EndDragDropTarget();
        }

        if (index == inventaire.activeIndex) {
            ImGui.PopStyleColor(3);
        }
        ImGui.PopID();
    }
}