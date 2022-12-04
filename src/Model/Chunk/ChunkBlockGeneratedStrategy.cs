using System.Text.RegularExpressions;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkBlockGeneratedStrategy : ChunkStrategy
{
    private const string PATHTOWORLDSAVE = "./Worlds/newWorld";
    
    public override ChunkState getChunkStateOfStrategy() => ChunkState.BLOCKGENERATED;
    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
    }

    public ChunkBlockGeneratedStrategy(Chunk chunk) : base(chunk) {    }

    public override async Task init() {
        
        //check if we have chunk in memory
        //if yes load it
        // if not check if chunk is in generated terrain and generate if not
        if (isChunkExistInMemory()) {
            Console.WriteLine("exist " + chunk.position);
            loadBlocks();
        }
        
        if (chunk.chunkState != ChunkState.Generatedterrain) {
            chunk.chunkStrategy = new ChunkTerrainGeneratedStrategy(chunk);
            await chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        await updateNeighboorChunkState(ChunkState.Generatedterrain);
        chunk.chunkState = ChunkState.BLOCKGENERATED;
    }

    private string pathToChunk() =>  PATHTOWORLDSAVE + "/" + chunk.position.X + "  " + chunk.position.Y  + "  " + chunk.position.Z;
    private bool isChunkExistInMemory() {
        return Directory.Exists(PATHTOWORLDSAVE) && File.Exists(pathToChunk());
    }
    
    private void loadBlocks() {
        
    }

    
}