using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

public class ChunkTest
{
    [OneTimeSetUp]
    public void setUp() {
        Directory.SetCurrentDirectory("./../../../../");
        Chunk.initStaticMembers(null, BlockFactory.getInstance());
    }
    
    [Test]
    public void testChunkIsNotNull() {
        ChunkManagerEmpty chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), null);
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        Assert.NotNull(chunk);
    }
    
    [Test]
    public void testChunkLoadTerrain() {
        ChunkStorage chunkStorage = new ChunkStorage("./Worlds/newWorld");
        ChunkManagerEmpty chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), chunkStorage);
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        Block block = chunk.getBlock(Vector3D<int>.Zero);
        Assert.IsNotNull(block);
        Assert.True(block.name.Equals("grass"));
    }
    
    

}