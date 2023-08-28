using MinecraftCloneSilk.Model;

namespace UnitTest;

public class testSuperChunk
{

    [Test]
    public unsafe void testSuperChunkMethod() {
        BlockData[,,] blocks = new BlockData[16,16,16];
        int index = 0;
        for (int i = 0; i < 16; i++) {
            for (int j = 0; j < 16; j++) {
                for (int k = 0; k < 16; k++) {
                    blocks[i, j, k] = new BlockData(index++);
                }
            }
        }
        BlockData[,,] superChunks = new BlockData[18,18,18];
        

        fixed(BlockData* blocksPtr = blocks, superChunksPtr = superChunks) {
            Span<BlockData> superChunksSpan = new Span<BlockData>(superChunksPtr, 18 * 18 * 18);
            Span<BlockData> blocksSpan = new Span<BlockData>(blocksPtr, 16 * 16 * 16);
            for (int x = 0; x < 16; x++) {
                for (int y = 0; y < 16; y++) {
                    int offsetSuperChunk = (x + 1) * 18 * 18 + (y + 1) * 18 + 1;
                    int offsetBlocks = x * 16 * 16 + y * 16;
                    blocksSpan.Slice(offsetBlocks, 16).CopyTo(superChunksSpan[offsetSuperChunk..]);
                }
            }
            
       
            Console.WriteLine(superChunks.Length);
        }
    }

    [Test]
    public unsafe void testSuperChunkMethodFor() {
        BlockData[,,] blocks = new BlockData[16,16,16];
        int index = 0;
        for (int i = 0; i < 16; i++) {
            for (int j = 0; j < 16; j++) {
                for (int k = 0; k < 16; k++) {
                    blocks[i, j, k] = new BlockData(index++);
                }
            }
        }
        BlockData[,,] superChunk = new BlockData[18,18,18];

        for(int x = 0; x < 16; x++) {
            for(int y = 0; y < 16; y++) {
                for(int z = 0; z < 16; z++) {
                    superChunk[x + 1, y + 1, z + 1] = blocks[x,y,z];
                }
            }
        }
        Console.WriteLine(superChunk);

    }
}