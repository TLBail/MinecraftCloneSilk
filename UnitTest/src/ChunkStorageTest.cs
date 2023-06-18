using System.IO.Compression;
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
        Directory.SetCurrentDirectory("./../../../../");
        Chunk.initStaticMembers(null, BlockFactory.getInstance());
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
    public void testCreatedChunkIsAbleToSaveAndRecoverData() {
        Chunk chunk = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        
        chunk.setBlock(0,0,0, "metal");

        chunk.save();
        
        chunkManagerEmpty.removeChunk(Vector3D<int>.Zero);
        Chunk chunk2 = chunkManagerEmpty.getChunk(Vector3D<int>.Zero);
        chunk2.setWantedChunkState(ChunkState.BLOCKGENERATED);
        
        Assert.True(chunk2.getBlock(Vector3D<int>.Zero).name.Equals("metal"));
    }


    [Test]
    public void testEmptyChunkLoadAndSave() {
        Vector3D<int> chunkPosition = new Vector3D<int>(0, (int)(Chunk.CHUNK_SIZE * 1000), 0);
        Chunk chunk = chunkManagerEmpty.getChunk(chunkPosition);
        chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        chunk.save();
        
        chunkManagerEmpty.removeChunk(chunkPosition);
        Chunk chunk2 = chunkManagerEmpty.getChunk(chunkPosition);
        chunk2.setWantedChunkState(ChunkState.BLOCKGENERATED);

        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.True(chunk2.getBlock(new Vector3D<int>(x,y,z)).name.Equals(BlockFactory.AIR_BLOCK));
                }
            }
        }

    }


    
    
    [Test]
    public void testCalculNbBytePerBlock() {
        int nbBlock = 1;
        Assert.AreEqual(1,ChunkStorage.Log8Ceil(nbBlock));
        

        nbBlock = 55;
        Assert.AreEqual(1,ChunkStorage.Log8Ceil(nbBlock));

        nbBlock = 255;
        Assert.AreEqual(1,ChunkStorage.Log8Ceil(nbBlock));

        nbBlock = 256;
        Assert.AreEqual(2,ChunkStorage.Log8Ceil(nbBlock));
        
        nbBlock = 65535;
        Assert.AreEqual(2,ChunkStorage.Log8Ceil(nbBlock));

        nbBlock = 65536;
        Assert.AreEqual(3,ChunkStorage.Log8Ceil(nbBlock));
        nbBlock = 16777215;
        Assert.AreEqual(3,ChunkStorage.Log8Ceil(nbBlock));
        nbBlock = 16777216;
        Assert.AreEqual(4,ChunkStorage.Log8Ceil(nbBlock));
    }


    [Test]
    public void testSaveChunkFullOfCobble() {
        Chunk chunk = chunkManagerEmpty.getChunk(new Vector3D<int>(0, -32, 0));
        chunk.setWantedChunkState(ChunkState.BLOCKGENERATED);
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.That(chunk.getBlock(x,y,z).name, Is.EqualTo("stone"));
                }
            }
        }
        
        chunk.save();
        
        chunkManagerEmpty.removeChunk(new Vector3D<int>(0, -32, 0));
        
        Chunk chunk2 = chunkManagerEmpty.getChunk(new Vector3D<int>(0, -32, 0));
        chunk2.setWantedChunkState(ChunkState.BLOCKGENERATED);
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    Assert.That(chunk2.getBlock(x,y,z).name, Is.EqualTo("stone"));
                }
            }
        }
    }
    
}