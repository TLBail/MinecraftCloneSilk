using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using Silk.NET.GLFW;
using Silk.NET.Input;

namespace MinecraftCloneSilk.UI;

public class InventaireUi : UiWindow
{
    private Inventaire inventaire = null!;
    private const string DNDCELL = "DND_CELL";
    private const ImGuiWindowFlags FLAGS = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar;
    private OpenGl openGl;
    public InventaireUi(Game game) : base(game, Key.E) {
        this.openGl = game.openGl;
    }

    protected override void SetVisible(IKeyboard keyboard, Key key, int a) {
        if(key != this.key) return;
        base.SetVisible(keyboard, key, a);
        openGl.SetCursorMode(visible ? CursorModeValue.CursorNormal : CursorModeValue.CursorDisabled);
    }

    protected override void Start() {
        inventaire = game.FindGameObject<Player>().inventaire;
    }

    protected override void DrawUi() {
        const float sizeX = 800.0f;
        const float sizeY = 660.0f;
        var viewport = ImGui.GetMainViewport();
        var workPos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
        var workSize = viewport.WorkSize;
        Vector2 windowPos, windowPosPivot;
        windowPos.X = workPos.X + (workSize.X / 2) - (sizeX / 2);
        windowPos.Y = workPos.Y + (workSize.Y / 2) - (sizeY / 2);
        ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(sizeX,sizeY));
        ImGui.Begin("Inventaire", FLAGS);
        ImGui.Text("inventaire");
        var footerHeigthToReserve = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
        if (ImGui.BeginChild("inventoryblocks", new Vector2(0, -footerHeigthToReserve), false)) {
            for (int x = 0; x < Inventaire.INVENTORYSIZE; x++) {
                if(x > 0 && x % 8== 0)ImGui.NewLine();
                ItemUi(x);
            }
        }
        ImGui.End();
    }
    
    
    private unsafe void ItemUi(int index) {
        string blockName = inventaire.inventoryBlocks[index]?.block.name ?? "";
        
        ImGui.PushID(index);
        
        
        ImGui.SameLine();
        
        //image
        ImGui.BeginGroup();
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
        
        ImGui.PopID();
    }
    
}