using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkTerrainGeneratedStrategy : ChunkStrategy
{
    
    public ChunkTerrainGeneratedStrategy(Chunk chunk) : base(chunk) { }

    public override async Task init() {
        if (chunk.chunkState != ChunkState.EMPTY) {
            chunk.chunkStrategy = new ChunkEmptyStrategy(chunk); 
            await chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        generateTerrain();
        chunk.chunkState = ChunkState.Generatedterrain;
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.Generatedterrain;
    
    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    private void generateTerrain()
    {
        chunk.worldGenerator.generateTerrain(chunk.position, chunk.blocks);
    }
}