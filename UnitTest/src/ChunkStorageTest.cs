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
        foreach (var file in Directory.GetFiles("./Worlds/newWorld")) {
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
    
    
    [Test]
    public void testCreatingChunkFollowNorm() {
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        Assert.IsNotNull(chunk);
        chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE];
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    blocks[i, y, z] = chunk.getBlockData(new Vector3D<int>(i, y, z));
                }
            }
        }
        Assert.False(chunkManagerEmpty.chunkStorage.isChunkExistInMemory(chunk));
        chunk.save();
        Assert.True(chunkManagerEmpty.chunkStorage.isChunkExistInMemory(chunk));
        
        using FileStream fs = File.Open(chunkManagerEmpty.chunkStorage.PathToChunk(chunk), FileMode.Open);
        using BinaryReader br = new BinaryReader(fs);
        Assert.That(br.ReadInt32(), Is.EqualTo(1), "version of file");
        Assert.That(br.ReadByte(), Is.EqualTo((byte) chunk.chunkState), "chunkState");
        Assert.That(br.ReadInt32(), Is.EqualTo(0), "tick of chunk");
        int nbBlock = br.ReadInt32();

        
        Console.WriteLine("nb block in palette: " + nbBlock);
        BlockData[] blocksData = new BlockData[nbBlock];
        for (int i = 0; i < nbBlock; i++) {
            blocksData[i] = new BlockData(br.ReadInt16());
            Console.WriteLine(blocksData[i].id);
        }
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.That(new BlockData(br.ReadInt32()), Is.EqualTo(blocks[i, y, z]));
                }
            }
        }
        



    }

}