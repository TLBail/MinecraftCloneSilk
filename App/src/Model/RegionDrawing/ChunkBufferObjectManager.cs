using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.OpenGL;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.RegionDrawing;

public class ChunkBufferObjectManager
{
    private GL gl;

    private List<RegionBuffer> regions = new List<RegionBuffer>();
    private List<RegionBuffer> regionsToUpdate = new List<RegionBuffer>();
    
    private Dictionary<Chunk, RegionBuffer> regionBufferByChunk = new Dictionary<Chunk, RegionBuffer>();
    
    private Stack<RegionBuffer> regionsWithAvailableSpace =  new Stack<RegionBuffer>();
    private Texture cubeTexture;
    private Game game;
    
    public ChunkBufferObjectManager(Texture cubeTexture, Game game) {
        this.cubeTexture = cubeTexture;
        this.game = game;
        game.drawables += Drawables;
        game.updatables += Update;
        this.gl = game.GetGl();
    }



    public void AddChunkToRegion(Chunk chunk) {
        if (regionsWithAvailableSpace.Count == 0) {
            CreateNewRegionBuffer();
        }

        RegionBuffer regionBuffer = regionsWithAvailableSpace.Peek();
        regionBuffer.AddChunk(chunk);
        if(regionBuffer.HaveAvailableSpace()) {
            regionsWithAvailableSpace.Pop();
        }
        regionBufferByChunk.Add(chunk, regionBuffer);
    }
    
    public void NeedToUpdateChunk(Chunk chunk) {
        if(!regionsToUpdate.Contains(regionBufferByChunk[chunk])) regionsToUpdate.Add(regionBufferByChunk[chunk]);
    }

    public void RemoveChunk(Chunk chunk) {
        regionBufferByChunk[chunk]?.RemoveChunk(chunk);
        NeedToUpdateChunk(chunk);
        if(!regionsWithAvailableSpace.Contains(regionBufferByChunk[chunk])) regionsWithAvailableSpace.Push(regionBufferByChunk[chunk]);
        regionBufferByChunk.Remove(chunk);

    }

    private void Drawables(GL gl, double deltatime) {
        foreach (RegionBuffer region in regions) {
            region.Draw();
        }
    }

    private void Update(double deltatime) {
        foreach (RegionBuffer region in regionsToUpdate) {
            region.Update();
        }
        regionsToUpdate.Clear();
    }

    
    private void CreateNewRegionBuffer() {
        RegionBuffer region = new RegionBuffer(cubeTexture, gl);
        regionsWithAvailableSpace.Push(region);
        regions.Add(region);
    }
    
    
    
    
}