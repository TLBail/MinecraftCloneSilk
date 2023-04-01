using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

public class ChunkTest
{
    [OneTimeSetUp]
    public void setUp() {
        TextureManager textureManager = TextureManager.getInstance();
        textureManager.fakeLoad();
        
    }
    
    [Test]
    public void testChunkIsNotNull() {
        ChunkManagerEmpty chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration());
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        Assert.NotNull(chunk);
    }
    
    [Test]
    public void testChunkLoadTerrain() {
        ChunkManagerEmpty chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration());
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        chunk.setWantedChunkState(ChunkState.GENERATEDTERRAIN);
        Block block = chunk.getBlock(Vector3D<int>.Zero);
        Assert.IsNotNull(block);
        Assert.True(block.name.Equals("grass"));
    }
    
    

}