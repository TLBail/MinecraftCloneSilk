using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.OpenGL;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model;

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
        game.updatables += Updatables;
        this.gl = game.getGL();
    }



    public void addChunkToRegion(Chunk chunk) {
        if (regionsWithAvailableSpace.Count == 0) {
            createNewRegionBuffer();
        }

        RegionBuffer regionBuffer = regionsWithAvailableSpace.Peek();
        regionBuffer.addChunk(chunk);
        if(regionBuffer.haveAvailableSpace()) {
            regionsWithAvailableSpace.Pop();
        }
        regionBufferByChunk.Add(chunk, regionBuffer);
    }
    
    public void needToUpdateChunk(Chunk chunk) {
        if(!regionsToUpdate.Contains(regionBufferByChunk[chunk])) regionsToUpdate.Add(regionBufferByChunk[chunk]);
    }

    public void removeChunk(Chunk chunk) {
        regionBufferByChunk[chunk]?.removeChunk(chunk);
        if(!regionsWithAvailableSpace.Contains(regionBufferByChunk[chunk])) regionsWithAvailableSpace.Push(regionBufferByChunk[chunk]);
        regionBufferByChunk.Remove(chunk);

    }

    private void Drawables(GL gl, double deltatime) {
        foreach (RegionBuffer region in regions) {
            region.draw();
        }
    }

    private void Updatables(double deltatime) {
        foreach (RegionBuffer region in regionsToUpdate) {
            region.update();
        }
        regionsToUpdate.Clear();
    }

    
    private void createNewRegionBuffer() {
        RegionBuffer region = new RegionBuffer(cubeTexture, gl);
        regionsWithAvailableSpace.Push(region);
        regions.Add(region);
    }
    
    
    
    
}