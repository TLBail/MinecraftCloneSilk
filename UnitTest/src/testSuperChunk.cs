using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.RegionDrawing;

namespace UnitTest;

public class testSuperChunk
{

    [Test]
    public unsafe void testSuperChunkMethod() {
        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE];
        int index = 0;
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++) {
                    blocks[i, j, k] = new BlockData(index++);
                }
            }
        }
        BlockData[,,] superChunks = new BlockData[RegionBuffer.SUPER_CHUNK_SIZE,RegionBuffer.SUPER_CHUNK_SIZE,RegionBuffer.SUPER_CHUNK_SIZE];
        

        fixed(BlockData* blocksPtr = blocks, superChunksPtr = superChunks) {
            Span<BlockData> superChunksSpan = new Span<BlockData>(superChunksPtr, RegionBuffer.SUPER_CHUNK_SIZE * RegionBuffer.SUPER_CHUNK_SIZE * RegionBuffer.SUPER_CHUNK_SIZE);
            Span<BlockData> blocksSpan = new Span<BlockData>(blocksPtr, Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE);
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    int offsetSuperChunk = (x + 1) * RegionBuffer.SUPER_CHUNK_SIZE * RegionBuffer.SUPER_CHUNK_SIZE + (y + 1) * RegionBuffer.SUPER_CHUNK_SIZE + 1;
                    int offsetBlocks = x * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE + y * Chunk.CHUNK_SIZE;
                    blocksSpan.Slice(offsetBlocks, Chunk.CHUNK_SIZE).CopyTo(superChunksSpan[offsetSuperChunk..]);
                }
            }
            
       
            Console.WriteLine(superChunks.Length);
        }
    }

    [Test]
    public unsafe void testSuperChunkMethodFor() {
        BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE,Chunk.CHUNK_SIZE];
        int index = 0;
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                for (int k = 0; k < Chunk.CHUNK_SIZE; k++) {
                    blocks[i, j, k] = new BlockData(index++);
                }
            }
        }
        BlockData[,,] superChunk = new BlockData[RegionBuffer.SUPER_CHUNK_SIZE,RegionBuffer.SUPER_CHUNK_SIZE,RegionBuffer.SUPER_CHUNK_SIZE];

        for(int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for(int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for(int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    superChunk[x + 1, y + 1, z + 1] = blocks[x,y,z];
                }
            }
        }
        Console.WriteLine(superChunk);

    }
}