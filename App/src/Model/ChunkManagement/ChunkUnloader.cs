using System.Collections;
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

    private IEnumerator<Vector3D<int>>? enumeratorPositionToUnload;
    
    
    public ChunkUnloader(ChunkManager chunkManager, ChunkPool chunkPool, IChunkStorage chunkStorage) {
        this.chunkManager = chunkManager;
        this.chunkPool = chunkPool;
        this.chunkStorage = chunkStorage;
        this.enumeratorPositionToUnload = null;
    }

    public void SetCenterOfUnload(Vector3D<int> centerChunk, int radiusOfUnloading) {
        enumeratorPositionToUnload = new ChunkToUnloadEnumerator(chunkManager.chunks.Keys, centerChunk, radiusOfUnloading).GetEnumerator();
    }


    [Logger.Timer]
    public void Update() {
        if (enumeratorPositionToUnload is null) return;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        Dictionary<Vector3D<int>, ChunkState> minimumChunkStateOfNeighborsCache = new ();
        while(enumeratorPositionToUnload.MoveNext() && stopwatch.ElapsedMilliseconds < 5) {
            TryToUnloadChunk(enumeratorPositionToUnload.Current, minimumChunkStateOfNeighborsCache);
        }
        stopwatch.Restart();
        
    }

    public bool TryToUnloadChunk(Vector3D<int> position, Dictionary<Vector3D<int>, ChunkState>? minimumChunkStateOfNeighborsCache = null) {
        Chunk chunkToUnload = chunkManager.chunks[position];
        if(chunkToUnload.IsRequiredByChunkLoader() || chunkToUnload.IsRequiredByChunkSaver()) return false;
        ChunkState minimumChunkStateOfChunk = GetMinimumChunkStateOfChunk(position, minimumChunkStateOfNeighborsCache);
        if (chunkToUnload.chunkState == ChunkState.DRAWABLE) { // Todo change by chunkState > BlockGenerated ?
            chunkToUnload.SetChunkState(ChunkState.BLOCKGENERATED);
            chunkToUnload.FinishChunkState();
        }

        if (chunkToUnload.chunkState == ChunkState.LIGHTING) {
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
    
    private ChunkState GetMinimumChunkStateOfChunk(Vector3D<int> position, Dictionary<Vector3D<int>, ChunkState>? minimumChunkStateOfNeighborsCache = null) {
        ChunkState chunkState = ChunkState.EMPTY;
        Chunk chunk;
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> positionChunkToTest = position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE);
            if (minimumChunkStateOfNeighborsCache is not null &&  minimumChunkStateOfNeighborsCache.TryGetValue(positionChunkToTest, out ChunkState minimumChunkStateOfNeighbor) &&
                minimumChunkStateOfNeighbor > chunkState) {
                chunkState = minimumChunkStateOfNeighbor;
            } else if (  chunkManager.chunks.TryGetValue(positionChunkToTest, out chunk) &&
                         chunk.GetMinimumChunkStateOfNeighbors() > chunkState) {
                chunkState = chunk.GetMinimumChunkStateOfNeighbors();
            }
        }
        return chunkState;
    }

    internal void ForceUnloadChunk(ChunkLoader chunkLoader, Chunk chunkToUnload) {
        if (chunkToUnload.IsRequiredByChunkLoader()) {
            chunkLoader.UpdateUntilChunkLoaded(chunkToUnload);
        }
        Debug.Assert(!chunkToUnload.IsRequiredByChunkLoader());
        if (chunkToUnload.IsRequiredByChunkSaver()) {
            throw new Exception("fuck");
        }
        
        
        if (chunkToUnload.chunkState == ChunkState.DRAWABLE) {
            chunkToUnload.SetChunkState(ChunkState.BLOCKGENERATED);
            chunkToUnload.FinishChunkState();
        }
        
        if (chunkToUnload.blockModified) {
            chunkStorage.SaveChunk(chunkToUnload);
        }
        
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> positionChunkToTest = chunkToUnload.position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE);
            if (chunkManager.chunks.TryGetValue(positionChunkToTest, out Chunk neighbor) &&
                         neighbor.GetMinimumChunkStateOfNeighbors() > ChunkState.EMPTY) {
                ForceUnloadChunk(chunkLoader, neighbor);
            }
        }
        
        if (chunkManager.chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunkToUnload.position, chunkToUnload))) {
            chunkPool.ReturnChunk(chunkToUnload);
        } 
    }


    private class ChunkToUnloadEnumerator(ICollection<Vector3D<int>> chunkPos, Vector3D<int> center, int radius) : IEnumerable<Vector3D<int>>
    {
        
        public IEnumerator<Vector3D<int>> GetEnumerator() {
            foreach (Vector3D<int> position in chunkPos) {
                if (Vector3D.Distance(position, center) > radius * Chunk.CHUNK_SIZE) {
                    yield return position;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}