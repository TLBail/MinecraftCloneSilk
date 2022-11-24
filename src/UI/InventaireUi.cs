using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class InventaireUi : UiWindow
{
    private Inventaire inventaire;
    private const string DNDCELL = "DND_CELL";
    private const ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar;
    
    public InventaireUi(Game game) : base(game, Key.E) {
        
    }

    protected override void start() {
        inventaire = ((Player)game.gameObjects[typeof(Player).FullName]).inventaire;
    }

    protected override void drawUi() {
        ImGui.Begin("Inventaire", flags);
        ImGui.Text("inventaire");
        var footerHeigthToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        if (ImGui.BeginChild("inventoryblocks", new Vector2(0, -footerHeigthToReserve), false)) {
            for (int x = 0; x < Inventaire.INVENTORYSIZE; x++) {
                if(x > 0 && x % 8== 0)ImGui.NewLine();
                itemUi(x);
            }
        }
        ImGui.End();
    }
    
    
    private unsafe void itemUi(int index) {
        string blockName = "";
        if (inventaire.get(index) != null) blockName = inventaire.get(index).block.name;
        
        ImGui.PushID(index);
        
        
        ImGui.SameLine();
        
        ImGui.BeginGroup();

        //image
        Vector2 uvMin = Vector2.Zero;
        Vector2 uvMax = new Vector2(1.0f);
        Vector4 tintCol = new Vector4(1.0f);
        Vector4 borderCol = new Vector4(1.0f, 1.0f, 1.0f, 0.5f);
        if (blockName.Length > 0) {
            if(inventaire.inventoryBlocks[index]!.block.fullTexture != null)
                ImGui.Image((IntPtr)inventaire.inventoryBlocks[index]!.block.fullTexture._handle, new Vector2(80, 80), uvMin, uvMax, tintCol, borderCol);
            ImGui.Button(blockName, new Vector2(80, 20));
        } else {
            // ImGui.Image((IntPtr)textures[index]._handle, new Vector2(80, 80), uvMin, uvMax, tintCol, borderCol);
            ImGui.Button(blockName, new Vector2(80, 20));
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
        
        ImGui.PopID();
    }
    
}