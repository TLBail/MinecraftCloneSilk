using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public static class ChunkManagerTools
{
    public static List<Chunk> getChunkDependent(IChunkManager chunkManager, Stack<ChunkLoadingTask> chunksToLoad) {
        List<Chunk> chunkLoadOrder = new List<Chunk>(chunksToLoad.Count);
        while (chunksToLoad.TryPop(out ChunkLoadingTask chunkTask)) {
            
            if (chunkTask.chunk.wantedChunkState < chunkTask.wantedChunkState) {
                chunkTask.chunk.wantedChunkState = chunkTask.wantedChunkState;
            }

            if (!chunkTask.chunk.isRequiredByChunkLoader) {
                chunkTask.chunk.isRequiredByChunkLoader = true;
                chunkLoadOrder.Add(chunkTask.chunk);
            }

            Vector3D<int>[] dependantesChunkOffset =
                ChunkStrategy.getDependanteChunksOffsetOfAChunkState(chunkTask.wantedChunkState);

            for (int i = 0; i < dependantesChunkOffset.Length; i++) {
                Vector3D<int> position = dependantesChunkOffset[i] + chunkTask.chunk.position;
                Chunk chunk = chunkManager.getChunk(position);
                ChunkState wantedDependantChunkState =
                    ChunkStrategy.getMinimumChunkStateOfNeighbors(chunkTask.wantedChunkState);
                chunksToLoad.Push(new ChunkLoadingTask(chunk, wantedDependantChunkState));
            }
        }
        return chunkLoadOrder;
    }
    
    
    public static Chunk getBlockGeneratedChunk(IChunkManager chunkManager,ChunkLoader chunkLoader, Vector3D<int> position) {
        Chunk chunk = chunkManager.getChunk(position);
        Stack<ChunkLoadingTask> chunkLoadingTasks = new Stack<ChunkLoadingTask>();
        chunkLoadingTasks.Push(new ChunkLoadingTask(chunk, ChunkState.BLOCKGENERATED));
        chunkLoader.addChunks(getChunkDependent(chunkManager, chunkLoadingTasks));
        chunkLoader.singleThreadLoading();
        return chunk;
    }


}