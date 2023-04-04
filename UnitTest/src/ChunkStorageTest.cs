using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

public class ChunkStorageTest
{
    private ChunkManagerEmpty chunkManagerEmpty;

    
    [OneTimeSetUp]
    public void setUp() {
        TextureManager textureManager = TextureManager.getInstance();
        textureManager.fakeLoad();
        
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), new ChunkStorage("./Worlds/newWorld"));
    }

    [SetUp]
    public void setup() {
        DirectoryInfo directory = Directory.CreateDirectory("./Worlds/newWorld");
        foreach (var file in         Directory.GetFiles("./Worlds/newWorld")) {
            File.Delete(file);
        }   
    }
    
    
    [Test]
    public void testChunkStorageHaveOpenFolder() {
        ChunkStorage chunkStorage = new ChunkStorage("./Worlds/newWorld");
        Assert.IsNotNull(chunkStorage);
    }

    [Test]
    public void testCreatingChunk() {
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        Assert.IsNotNull(chunk);
        chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        Assert.False(chunkManagerEmpty.chunkStorage.isChunkExistInMemory(chunk));
        chunk.save();
        Assert.True(chunkManagerEmpty.chunkStorage.isChunkExistInMemory(chunk));
        
        
    }
}