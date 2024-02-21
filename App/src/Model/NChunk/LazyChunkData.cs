using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MinecraftCloneSilk.Model.NChunk;


/**
 * This class is Work in progress
 * it offer better memory management than the previous version
 * but is not thread safe
 * TODO: make it thread safe
 */

public class LazyChunkData : IChunkData
{
    private static ConcurrentBag<BlockData[,,]> blocksBag = new();
    
    private BlockData[,,]? blocks;
    private BlockData block = new BlockData();

    public BlockData GetBlock(in int x = 0,in int y = 0,in int z = 0) {
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
   
    public void SetBlock(in BlockData block,in int x = 0,in int y = 0,in int z = 0) {
        if(IsOnlyOneBlock() && block.id == this.block.id) return;
        if(IsOnlyOneBlock() && block.id != this.block.id) {
            InstanciateArray();
        } 
        blocks![x, y, z] = block;
    }
    
    public void SetBlocks(in BlockData blockData) {
        Debug.Assert(IsOnlyOneBlock());
        block = blockData;
    }
    
    public bool IsOnlyOneBlock() {
        return blocks is null;
    }

    private void InstanciateArray() {
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