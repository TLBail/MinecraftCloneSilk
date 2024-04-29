using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkData : IChunkData
{
    private BlockData[,,] blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
    public BlockData GetBlock(int x = 0,int y = 0,int z = 0) {
        return blocks[x, y, z];
    }

    public BlockData[,,] GetBlocks() {
        return blocks;
    }

    public Span<BlockData> GetBlocksSpan() {
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(blocks!);
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, BlockData>(ref reference), blocks!.Length);
    }

    public static Span<BlockData> GetSpan(BlockData[,,] blocks) {
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(blocks!);
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, BlockData>(ref reference), blocks!.Length);
    }
    

    public void SetBlock(BlockData block,int x = 0,int y = 0,int z = 0) {
        blocks[x, y, z] = block;
    }

    public void SetBlocks(in BlockData blockData) {
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    blocks[x, y, z] = blockData;
                }
            }
        }
    }

    public void Reset() {
        blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
    }
}