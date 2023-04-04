namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkTerrainGeneratedStrategy : ChunkStrategy
{
    
    public ChunkTerrainGeneratedStrategy(Chunk chunk) : base(chunk) { }

    public override void init() {
        if (chunk.chunkState != ChunkState.EMPTY) {
            chunk.chunkStrategy = new ChunkEmptyStrategy(chunk); 
            chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }

        if (chunk.chunkStorage.isChunkExistInMemory(chunk)) {
            chunk.chunkStorage.LoadBlocks(chunk);
        } else {
            generateTerrain();            
        }
        chunk.chunkState = ChunkState.GENERATEDTERRAIN;
    }

    
    public override ChunkState getChunkStateOfStrategy() => ChunkState.GENERATEDTERRAIN;
    
    public override void setBlock(int x, int y, int z, string name) {
        chunk.blockModified = true;
        lock (chunk.blocksLock) {
            chunk.blocks[x, y, z].id = Chunk.blockFactory.getBlockIdByName(name);
        }
    }
    
    

    public override Block getBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a terrain generated only chunk");
    }

    private void generateTerrain()
    {
        lock (chunk.blocksLock) {
            chunk.worldGenerator.generateTerrain(chunk.position, chunk.blocks);
        }
    }
}