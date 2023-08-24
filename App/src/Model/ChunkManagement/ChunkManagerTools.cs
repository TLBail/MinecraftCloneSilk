using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public static class ChunkManagerTools
{
    
    
    
    public static Chunk GetBlockGeneratedChunk(IChunkManager chunkManager,ChunkLoader chunkLoader, Vector3D<int> position) {
        Chunk chunk = chunkManager.GetChunk(position);
        Stack<ChunkLoadingTask> chunkLoadingTasks = new Stack<ChunkLoadingTask>();
        chunkLoadingTasks.Push(new ChunkLoadingTask(chunk, ChunkState.BLOCKGENERATED));
        chunkLoader.AddChunks(ChunkLoader.GetChunkDependent(chunkManager, chunkLoadingTasks));
        chunkLoader.SingleThreadLoading();
        return chunk;
    }


}