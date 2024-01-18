using System.IO.Compression;
using System.Text;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;
using UnitTest.fakeClass;

namespace UnitTest;

public class ChunkStorageTest
{
    private ChunkManagerEmpty chunkManagerEmpty;
    private ChunkLoader chunkLoader;
    private ChunkStorage chunkStorage;
    
    [OneTimeSetUp]
    public void SetUp() {
        Chunk.InitStaticMembers(null, BlockFactory.GetInstance());
        chunkStorage = new ChunkStorage("./Worlds/newWorld");
        chunkManagerEmpty = new ChunkManagerEmpty(new WorldFlatGeneration(), chunkStorage);
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.SYNC);
    }

    [SetUp]
    public void Setup() {
        DirectoryInfo directory = Directory.CreateDirectory("./Worlds/newWorld");
        foreach (var file in Directory.GetFiles("./Worlds/newWorld")) {
            File.Delete(file);
        }   
    }
    
    [Test]
    public void AbleToGetChunkGenereated() {
        Chunk chunk = GetBlockGeneratedChunk();
        Assert.That(chunk.chunkState, Is.EqualTo(ChunkState.BLOCKGENERATED));
    }

    private Chunk GetBlockGeneratedChunk(Vector3D<int> position = default) {
        return ChunkManagerTools.GetBlockGeneratedChunk(chunkManagerEmpty, chunkLoader, position);
    }
    
    [Test]
    public void TestChunkStorageHaveOpenFolder() {
        ChunkStorage chunkStorage = new ChunkStorage("./Worlds/newWorld");
        Assert.IsNotNull(chunkStorage);
    }

    [Test]
    public void TestCreatingChunk() {
        Chunk chunk = GetBlockGeneratedChunk(new Vector3D<int>(23 * Chunk.CHUNK_SIZE, 0 , 0));
        Assert.False(chunkManagerEmpty.chunkStorage.IsChunkExistInMemory(chunk.position));
        chunkStorage.SaveChunk(chunk);
        Assert.True(chunkManagerEmpty.chunkStorage.IsChunkExistInMemory(chunk.position));
    }
    
    
   
    
    [Test]
    public void TestCreatingChunkFollowNorm() {
        Chunk chunk = GetBlockGeneratedChunk(new (12* Chunk.CHUNK_SIZE, 0,0));

        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE];
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    blocks[i, y, z] = chunk.GetBlockData(new Vector3D<int>(i, y, z));
                }
            }
        }
        Assert.False(chunkStorage.IsChunkExistInMemory(chunk.position));
        chunkStorage.SaveChunk(chunk);
        Assert.True(chunkStorage.IsChunkExistInMemory(chunk.position));
        
        using FileStream fs = File.Open(chunkStorage.PathToChunk(chunk.position), FileMode.Open);
        using ZLibStream zs = new ZLibStream(fs, CompressionMode.Decompress, false);
        using BinaryReader br = new BinaryReader(zs);
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
        
        int nbBytePerBlock = ChunkStorage.Log8Ceil(nbBlock);
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int indexPalette = 0;
                    for (int j = 0; j < nbBytePerBlock; j++) {
                        indexPalette += br.ReadByte() << (j * 8);
                    }
                    Assert.That(blocksData[indexPalette], Is.EqualTo(blocks[i, y, z]));
                }
            }
        }
    }

    [Test]
    public void TestCreatedChunkIsAbleToSaveAndRecoverData() {
        Chunk chunk = GetBlockGeneratedChunk();
        chunk.SetBlock(0,0,0, "metal");

        chunkStorage.SaveChunk(chunk);
        
        chunkManagerEmpty.RemoveChunk(Vector3D<int>.Zero);
        
        Chunk chunk2 = GetBlockGeneratedChunk();

        
        Assert.True(chunk2.GetBlock(Vector3D<int>.Zero).name.Equals("metal"));
    }


    [Test]
    public void TestEmptyChunkLoadAndSave() {
        Vector3D<int> chunkPosition = new Vector3D<int>(0, (int)(Chunk.CHUNK_SIZE * 1000), 0);
        Chunk chunk = GetBlockGeneratedChunk(chunkPosition);
        chunkStorage.SaveChunk(chunk);
        
        chunkManagerEmpty.RemoveChunk(chunkPosition);
        Chunk chunk2 = GetBlockGeneratedChunk(chunkPosition);

        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.True(chunk2.GetBlock(new Vector3D<int>(x,y,z)).name.Equals(BlockFactory.AIR_BLOCK));
                }
            }
        }

    }


    
    
    [Test]
    public void TestCalculNbBytePerBlock() {
        int nbBlock = 1;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(1));
        

        nbBlock = 55;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(1));

        nbBlock = 255;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(1));

        nbBlock = 256;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(2));
        
        nbBlock = 65535;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(2));

        nbBlock = 65536;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(3));
        nbBlock = 16777215;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(3));
        nbBlock = 16777216;
        Assert.That(ChunkStorage.Log8Ceil(nbBlock), Is.EqualTo(4));
    }


    [Test]
    public void TestSaveChunkFullOfCobble() {
        Chunk chunk = GetBlockGeneratedChunk(new Vector3D<int>(0, -32, 0));

        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.That(chunk.GetBlock(x,y,z).name, Is.EqualTo("stone"));
                }
            }
        }
        
        chunkStorage.SaveChunk(chunk);
        
        chunkManagerEmpty.RemoveChunk(new Vector3D<int>(0, -32, 0));
        
        Chunk chunk2 = GetBlockGeneratedChunk(new Vector3D<int>(0, -32, 0));
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.That(chunk2.GetBlock(x,y,z).name, Is.EqualTo("stone"));
                }
            }
        }
    }
    
}