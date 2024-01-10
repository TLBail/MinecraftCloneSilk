using System.Diagnostics;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkUnloader
{
    private ChunkManager chunkManager;
    private ChunkPool chunkPool;
    private IChunkStorage chunkStorage;
    
    public ChunkUnloader(ChunkManager chunkManager, ChunkPool chunkPool, IChunkStorage chunkStorage) {
        this.chunkManager = chunkManager;
        this.chunkPool = chunkPool;
        this.chunkStorage = chunkStorage;
    }
    
    public bool TryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunkManager.chunks[position];
        if(chunkToUnload.IsRequiredByChunkLoader() || chunkToUnload.IsRequiredByChunkSaver()) return false;
        ChunkState minimumChunkStateOfChunk = GetMinimumChunkStateOfChunk(position);
        if (chunkToUnload.chunkState == ChunkState.DRAWABLE) {
            chunkToUnload.SetChunkState(ChunkState.BLOCKGENERATED);
            chunkToUnload.FinishChunkState();
        }
        if(minimumChunkStateOfChunk > ChunkState.EMPTY) return false;
        
        if (chunkToUnload.blockModified) {
            chunkStorage.SaveChunkAsync(chunkToUnload);
            return false;
        }
        
        
        if (chunkManager.chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(position, chunkToUnload))) {
            chunkPool.ReturnChunk(chunkToUnload);
            return true;
        } else {
            throw new Exception("race condition on chunk unloading ");
        }
    }
    private ChunkState GetMinimumChunkStateOfChunk(Vector3D<int> position) {
        ChunkState chunkState = ChunkState.EMPTY;
        Chunk chunk;
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            if (chunkManager.chunks.TryGetValue(position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE), out chunk) &&
                chunk.GetMinimumChunkStateOfNeighbors() > chunkState) {
                chunkState = chunk.GetMinimumChunkStateOfNeighbors();
            }
        }
        return chunkState;
    }

    internal void ForceUnloadChunk(Chunk chunkToUnload) {
        if(chunkToUnload.IsRequiredByChunkLoader()) return;
        chunkManager.chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunkToUnload.position, chunkToUnload));
    }
}