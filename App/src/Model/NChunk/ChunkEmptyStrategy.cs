using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkEmptyStrategy : ChunkStrategy
{
    public ChunkEmptyStrategy(Chunk chunk) : base(chunk) {
    }

    public override ChunkState GetChunkStateOfStrategy() => ChunkState.EMPTY;


    public override BlockData GetBlockData(Vector3D<int> position) {
        throw new Exception("try to access to block data but the chunk is empty");
    }

    public override void SetBlock(int x, int y, int z, string name) {
        throw new Exception("try to access to block data but the chunk is empty");
    }

    public override Block GetBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a empty chunk");
    }


    public override void Finish() {
        chunk.chunkState = ChunkState.EMPTY;
    }
}