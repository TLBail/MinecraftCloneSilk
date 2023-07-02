namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkTerrainGeneratedStrategy : ChunkStrategy
{

    public ChunkTerrainGeneratedStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.EMPTY) {
            throw new Exception("try to create a ChunkTerrainGeneratedStrategy with a chunk that is not empty");
        }
    }

    private bool isChunkInMemory;
    
    public override void init() {
        isChunkInMemory = chunk.chunkStorage.isChunkExistInMemory(chunk.position);
    }

    public override void load() {

        if (isChunkInMemory) {
            chunk.chunkStorage.LoadBlocks(chunk);
        } else {
            generateTerrain();
        }
    }

    public override void finish() {
        chunk.chunkState = ChunkState.GENERATEDTERRAIN;
    }
    
    
    public override ChunkState getChunkStateOfStrategy() => ChunkState.GENERATEDTERRAIN;
    
    public override void setBlock(int x, int y, int z, string name) {
        chunk.blockModified = true;
        chunk.blocks[x, y, z].id = Chunk.blockFactory.getBlockIdByName(name);
    }
    
    

    public override Block getBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a terrain generated only chunk");
    }

    private void generateTerrain()
    {
        chunk.worldGenerator.generateTerrain(chunk.position, chunk.blocks);
    }
}