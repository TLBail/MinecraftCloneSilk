using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkData
{
    private static ConcurrentBag<BlockData[,,]> blocksBag = new();
    
    private BlockData[,,]? blocks;
    private BlockData block = new BlockData();

    public BlockData GetBlock() {
        Debug.Assert(IsOnlyOneBlock());
        return block;
    }
    
    public BlockData GetBlock(int x, int y, int z) {
        return IsOnlyOneBlock() ? block : blocks![x, y, z];
    }
    
    public BlockData[,,] GetBlocks() {
        if(IsOnlyOneBlock()) InstanciateArray();
        return blocks!;
    }
    
    public Span<BlockData> GetBlocksSpan() {
        if (IsOnlyOneBlock()) {
            throw new Exception("wtf you try to get span of only one block ?");
            InstanciateArray();
        }
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(blocks!);
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, BlockData>(ref reference), blocks!.Length);
    }
   
    public void SetBlock(int x, int y, int z, BlockData block) {
        if(IsOnlyOneBlock() && block.id == this.block.id) return;
        if(IsOnlyOneBlock() && block.id != this.block.id) {
            InstanciateArray();
        } 
        blocks![x, y, z] = block;
    }
    
    public void SetBlock(BlockData block) {
        Debug.Assert(IsOnlyOneBlock());
        this.block = block;
    }

    public void SetBlocks(BlockData blockData) {
        Debug.Assert(IsOnlyOneBlock());
        block = blockData;
    }
    
    public bool IsOnlyOneBlock() {
        return blocks is null;
    }

    public void InstanciateArray() {
        Debug.Assert(IsOnlyOneBlock());
        if (!blocksBag.TryTake(out blocks)) {
            blocks = new BlockData[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
        } 
        if (this.block.id != 0) {
            ref byte reference = ref MemoryMarshal.GetArrayDataReference(blocks!);
            Span<BlockData> span = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, BlockData>(ref reference), blocks!.Length);
            span.Fill(this.block); 
        }
    }

    public void Reset() {
        block = new BlockData();
        if (blocks is not null) {
            Array.Clear(blocks);
            blocksBag.Add(blocks);
            blocks = null;
        }
    }

}