namespace MinecraftCloneSilk.Model.NChunk;

public interface IChunkData
{
    public BlockData GetBlock(in int x,in int y,in int z);
    public BlockData[,,] GetBlocks();

    public Span<BlockData> GetBlocksSpan();
    public void SetBlock(in BlockData block,in int x = 0,in int y = 0,in int z = 0);


    public void SetBlocks(in BlockData blockData);


    public void Reset();
 
}