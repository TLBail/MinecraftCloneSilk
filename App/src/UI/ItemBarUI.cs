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
    private IMouse? mouse;
    private ImGuiIOPtr imGuiIo;
    private static bool pOpen;
    private const string DNDCELL = "DND_CELL";
    private Texture texture = null!;
    private Texture[] textures = null!;
    private Player player = null!;
    private Inventaire inventaire = null!;
    
    public ItemBarUi(Game game) : base(game, null) {
        mouse = game.GetMouse();
        needMouse = false;
    }
    
    

    protected override void Start() {
        var dic = BlockFactory.GetInstance().blocks;
        texture = new Texture(game.GetGl(), Generated.FilePathConstants.Blocks.stone_png);
        textures = new Texture[dic.Count - 1];
        int index = 0;
        foreach (var keyValue in dic) {
            if (!keyValue.Value.name.Equals(BlockFactory.AIR_BLOCK)) {
                textures[index] = new Texture(game.GetGl(), Generated.FilePathConstants.Blocks.DirectoryPath + "/"+ keyValue.Value.name + ".png");
                index++;
            }
        }
        
        mouse!.Scroll += MouseOnScroll;
        imGuiIo = ImGui.GetIO();
        player = (Player)game.gameObjects[typeof(Player).FullName!];
        inventaire = player.inventaire;
    }

    private void MouseOnScroll(IMouse mouse, ScrollWheel scrollWheel) {
        inventaire.MoveActiveIndexByScroolOffset(scrollWheel.Y);
    }

    protected override void DrawUi() {
        ImGuiWindowFlags windowFlags  = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing| ImGuiWindowFlags.NoNav;
        const float pad = 10.0f;
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        Vector2 workSize = viewport.WorkSize;
        Vector2 windowPos, windowPosPivot;
        windowPos.X = (workPos.X + workSize.X) * 0.80f ;
        windowPos.Y = (workPos.Y + workSize.Y - pad);
        windowPosPivot.X = 1.0f;// : 0.0f;
        windowPosPivot.Y = 1.0f; // : 0.0f;
        ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always, windowPosPivot);
        windowFlags |= ImGuiWindowFlags.NoMove;
        ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background
        if (ImGui.Begin("item bar", ref pOpen, windowFlags)) {
            for (int i = Inventaire.STARTING_ITEM_BAR_INDEX; i <= Inventaire.ENDING_ITEM_BAR_INDEX; i++) {
                ItemUi(i);
            }
            ImGui.End();
        }

    }

    private unsafe void ItemUi(int index) {
        string blockName = inventaire.inventoryBlocks[index]?.block.name ?? "";
        
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
                ImGui.ImageButton((IntPtr)inventaire.inventoryBlocks[index]!.block.fullTexture!.handle,
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