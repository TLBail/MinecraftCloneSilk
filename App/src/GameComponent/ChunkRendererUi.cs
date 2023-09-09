using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.RegionDrawing;

namespace MinecraftCloneSilk.UI;

public class ChunkRendererUi : GameObject
{
    
    private ChunkBufferObjectManager chunkBufferObjectManager;
    
    
    public ChunkRendererUi(Game game) : base(game){}

    protected override void Start() {
        chunkBufferObjectManager = game.chunkBufferObjectManager!;
    }


    public override void ToImGui() {
        ImGui.Text("ChunkRendererUi");
        ImGui.Text("nbRegion : " + chunkBufferObjectManager.regions.Count);
        int nbRegionDrawing = 0;
        int nbRegionWithVertices = 0;
        int nbChunk = 0;
        foreach (RegionBuffer region in chunkBufferObjectManager.regions) {
            if(region.haveDrawLastFrame) nbRegionDrawing++;
            if(region.nbVertex > 0) nbRegionWithVertices++;
            nbChunk += region.chunkCount;
        }
        ImGui.Text("nbRegionDrawing : " + nbRegionDrawing);
        ImGui.Text("nbRegionWithVertices : " + nbRegionWithVertices);
        ImGui.Text("nbChunk drawable : " + nbChunk);
    }
}