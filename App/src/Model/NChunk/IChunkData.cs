namespace MinecraftCloneSilk.Model.NChunk;

public interface IChunkData
{
    public BlockData GetBlock(in int x = 0,in int y = 0,in int z = 0);
    public BlockData[,,] GetBlocks();

    public Span<BlockData> GetBlocksSpan();
    public void SetBlock(in BlockData block,in int x = 0,in int y = 0,in int z = 0);


    public void SetBlocks(in BlockData blockData);

    public bool IsOnlyOneBlock();

    public void Reset();
 
}