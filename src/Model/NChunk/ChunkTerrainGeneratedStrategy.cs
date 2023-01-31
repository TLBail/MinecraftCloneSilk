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
        generateTerrain();
        chunk.chunkState = ChunkState.Generatedterrain;
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.Generatedterrain;
    
    public override void setBlock(int x, int y, int z, string name) {
        lock (chunk.blocksLock) {
            chunk.blocks[x, y, z].id = name.GetHashCode();
        }
    }

    private void generateTerrain()
    {
        lock (chunk.blocksLock) {
            chunk.worldGenerator.generateTerrain(chunk.position, chunk.blocks);
        }
    }
}