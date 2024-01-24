using BenchmarkDotNet.Attributes;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;

namespace UnitTest;

[TestFixture]
public class ChunkFaceTest
{
    private BlockFactory blockFactory;

    [OneTimeSetUp]
    public void setup() {
        blockFactory = BlockFactory.GetInstance();
    }

    [Test]
    public void testFlags() {
        ChunkFace chunkFace = ChunkFace.TOPOPAQUE | ChunkFace.BOTTOMTRANSPARENT | ChunkFace.LEFTOPAQUE | ChunkFace.RIGHTTRANSPARENT | ChunkFace.FRONTOPAQUE | ChunkFace.BACKTRANSPARENT ;
        chunkFace ^= ChunkFace.TOPOPAQUE;
        Assert.That(chunkFace, Is.EqualTo(ChunkFace.BOTTOMTRANSPARENT | ChunkFace.LEFTOPAQUE | ChunkFace.RIGHTTRANSPARENT | ChunkFace.FRONTOPAQUE | ChunkFace.BACKTRANSPARENT));
    }
    
    [Test]
    public void GetChunkFaceFlagsTest_AllOpaque()
    {
        // Arrange
        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    blocks[x, y, z] = new BlockData { id = 1 }; // suppose que id=1 est opaque
                }
            }
        }

        // Act
        ChunkFace result = ChunkFaceUtils.GetChunkFaceFlags(blockFactory, blocks);

        // Assert
        Assert.That(result, Is.EqualTo(ChunkFaceUtils.ALLOPAQUE));
        Assert.IsTrue(ChunkFaceUtils.IsOpaque(result));
        Assert.IsTrue((result & ChunkFace.EMPTYCHUNK) == 0);
    }
    
    [Test]
    public void GetChunkFaceFlagsTest_AllTransparent()
    {
        // Arrange
        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        // Act
        ChunkFace result = ChunkFaceUtils.GetChunkFaceFlags(blockFactory, blocks);

        // Assert
        Assert.True(ChunkFaceUtils.IsTransparent(result));
        Assert.True((result & ChunkFace.EMPTYCHUNK) == 0);
    }


    [Test]
    public void GetChunkFaceAllTransparentAndFoliage() {
        
        // Arrange
        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        blocks[0, 0, 0] = new BlockData { id = blockFactory.GetBlockIdByName("foliage") }; // suppose que id=1 est opaque
        // Act
        ChunkFace result = ChunkFaceUtils.GetChunkFaceFlags(blockFactory, blocks);

        // Assert
        Assert.True(ChunkFaceUtils.IsTransparent(result));
        Assert.True((result & ChunkFace.EMPTYCHUNK) == 0);
    }
}