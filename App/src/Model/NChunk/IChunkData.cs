namespace MinecraftCloneSilk.Model.NChunk;

public interface IChunkData
{
    public BlockData GetBlock(int x = 0, int y = 0, int z = 0);
    public BlockData[,,] GetBlocks();

    public Span<BlockData> GetBlocksSpan();
    public void SetBlock(BlockData block, int x = 0, int y = 0, int z = 0);


    public void SetBlocks(BlockData blockData);

    public bool IsOnlyOneBlock();

    public void Reset();
 
}