namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkLightingStrategy : ChunkStrategy
{
    public ChunkLightingStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.BLOCKGENERATED) {
            throw new Exception("failed to init chunkDrawableStrategy because the chunk is not BLOCKGENERATED");
        }
    }

    public override ChunkState GetChunkStateOfStrategy() => ChunkState.LIGHTING;
    public override void SetBlock(int x, int y, int z, string name) {
        chunk.chunkData.SetBlock(x, y, z,Chunk.blockFactory!.GetBlockData(name));
    }

    public override void Init() {
        chunk.chunkState = ChunkState.LIGHTLOADING;
    }

    public override void Load() {
        if(chunk.chunkData.IsOnlyOneBlock()) {
            BlockData block = chunk.chunkData.GetBlock();
            block.data1 = 15;
           chunk.chunkData.SetBlock(block); 
        } else {
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                        chunk.chunkData.GetBlocks()[x, y, z].data1 = 15;
                    }
                }
            }
        }
    }


    public override void Finish() {
        chunk.chunkState = ChunkState.LIGHTING;
    }
}