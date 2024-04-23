using MinecraftCloneSilk.Model.ChunkManagement;
using Silk.NET.Maths;
using System.Diagnostics;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkStorageStrategy : ChunkStrategy
{
    public ChunkStorageStrategy(Chunk chunk) : base(chunk) {
    }

    public override ChunkState GetChunkStateOfStrategy() => ChunkState.EMPTY;


    public override void Init() {
        chunk.chunkState = ChunkState.STORAGELOADING;
    }

    public override void Load() {
        chunk.chunkStateInStorage = chunk.chunkStorage.GetChunkStateInStorage(chunk.position);
        System.Diagnostics.Debug.Assert(!ChunkStateTools.IsChunkIsLoading(chunk.chunkStateInStorage), "bad chunk state in storage 💾");
        if(chunk.chunkStateInStorage > ChunkState.EMPTY)
            chunk.chunkStorage.LoadChunk(chunk);
        
        if(chunk.chunkStateInStorage == ChunkState.BLOCKGENERATED) UpdateChunkFaces();
    }

    public override void Finish() {
        chunk.chunkState = chunk.chunkStateInStorage;
        chunk.SetChunkState(chunk.chunkState);
        chunk.FinishChunkState();
    }


    public override BlockData GetBlockData(Vector3D<int> localPosition) {
        throw new InvalidOperationException("try to access to block data but the chunk is empty");
    }

    public override void SetBlock(int x, int y, int z, string name) {
        throw new InvalidOperationException("try to set the block data but the chunk is empty");
    }

    public override Block GetBlock(int x, int y, int z) {
        throw new Exception("try to get Block of a empty chunk");
    } 
}