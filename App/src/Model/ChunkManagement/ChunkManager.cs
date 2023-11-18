using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkManager : IChunkManager
{
    private readonly ConcurrentDictionary<Vector3D<int>, Chunk> chunks;
    public IWorldGenerator worldGenerator { get; set; }
    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private ChunkPool chunkPool;
    private ChunkLoader chunkLoader;
    private IChunkStorage chunkStorage;

    public ChunkManager(int radius, IWorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        chunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2,
            radius * radius * radius);
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.ASYNC);
        chunkPool = new ChunkPool(this, worldGenerator, chunkStorage);
    }

    [Logger.Timer]
    public void Update(double deltatime) {
        chunkLoader.Update(); 
        foreach (Chunk chunk in new List<Chunk>(chunksToUpdate)) {
            Debug.Assert(chunk.chunkState >= ChunkState.DRAWABLE, $"try to update a chunk:{chunk} with a lower state than the minimum");
            chunk.Update(deltatime);
        }
    }

    public int Count() => chunks.Count;
    public List<Chunk> GetChunksList() => new List<Chunk>(chunks.Values);
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);


    public void Clear() {
        List<Chunk> chunksCopy = new List<Chunk>(chunks.Values);
        foreach (Chunk chunk in chunksCopy) {
            ForceUnloadChunk(chunk);
        }
    }

    public Chunk GetChunk(Vector3D<int> position) {
        if (!chunks.TryGetValue(position, out Chunk? chunk)) {
            chunk = chunks.GetOrAdd(position, chunkPool.Get);
        }
        return chunk;
    }
    
    public bool ContainChunk(Vector3D<int> position) {
        return chunks.ContainsKey(position);
    }

    public void AddChunkToUpdate(Chunk chunk) {
        chunksToUpdate.Add(chunk);
    }

    public void RemoveChunkToUpdate(Chunk chunk) {
        chunksToUpdate.Remove(chunk);
    }
    
    public bool RemoveChunk(Chunk chunk) {
        return chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunk.position, chunk));
    }

    
    [Logger.Timer]
    public void UpdateRelevantChunks(List<Vector3D<int>> positionsRelevant) {
        List<Vector3D<int>> chunksToLoad = new List<Vector3D<int>>();
        foreach (Vector3D<int> position in positionsRelevant) {
            if (!chunks.TryGetValue(position, out Chunk? chunk) || chunk.chunkState < ChunkState.DRAWABLE) {
                chunksToLoad.Add(position);
            }
        }
        AddChunksToLoad(chunksToLoad);
        TryToUnloadChunks(chunks.Keys.Except(positionsRelevant));
    }
    
    
    [Logger.Timer]
    private void TryToUnloadChunks(IEnumerable<Vector3D<int>> positions) {
        foreach (Vector3D<int> position in positions) {
            TryToUnloadChunk(position);
        }
    }

    public void AddChunkToLoad(Vector3D<int> position) {
        position = World.GetChunkPosition(position);
        AddChunksToLoad(new List<Vector3D<int>> { position });
    }

    [Logger.Timer]
    public void AddChunksToLoad(List<Vector3D<int>> positions) {
        Chunk[] chunksToLoad = new Chunk[positions.Count];
        Parallel.For(0, positions.Count, i => {
            chunksToLoad[i] = GetChunk(positions[i]);
        });
        AddChunksToQueue(chunksToLoad);
    }

    [Logger.Timer]
    private void AddChunksToQueue(Chunk[] chunksToLoad){
        foreach (Chunk chunk in chunksToLoad) {
            chunkLoader.AddChunkToQueue(chunk);
        }
    }


    public bool TryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunks[position];
        if(chunkToUnload.IsRequiredByChunkLoader() || chunkToUnload.IsRequiredByChunkUnloader()) return false;
        ChunkState minimumChunkStateOfChunk = GetMinimumChunkStateOfChunk(position);
        if (chunkToUnload.chunkState == ChunkState.DRAWABLE) {
            chunkToUnload.SetChunkState(ChunkState.BLOCKGENERATED);
            chunkToUnload.FinishChunkState();
        }
        if(minimumChunkStateOfChunk > ChunkState.EMPTY) return false;
        
        if (chunkToUnload.blockModified) {
            chunkToUnload.AddRequiredByChunkUnloader();
            chunkStorage.SaveChunkAsync(chunkToUnload);
            return false;
        }
        
        
        if (chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(position, chunkToUnload))) {
            chunkPool.ReturnChunk(chunkToUnload);
            return true;
        } else {
            Debug.Assert(false,"race condition while deleting chunk");
            return false;
        }
    }

    private void ForceUnloadChunk(Chunk chunkToUnload) {
        if(chunkToUnload.IsRequiredByChunkLoader()) return;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunkToUnload.position, chunkToUnload));
    }
    
    private ChunkState GetMinimumChunkStateOfChunk(Vector3D<int> position) {
        ChunkState chunkState = ChunkState.EMPTY;
        Chunk chunk;
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            if (chunks.TryGetValue(position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE), out chunk) &&
                chunk.GetMinimumChunkStateOfNeighbors() > chunkState) {
                chunkState = chunk.GetMinimumChunkStateOfNeighbors();
            }
        }
        return chunkState;
    }

    public void ToImGui() {
        ImGui.Text($"number of chunkTasks {chunkLoader.chunkTasks.Count}");
        
        
        ImGui.Text($"number of chunks {chunks.Count}");
        ImGui.Text($"number of chunks in pool {chunkPool.Count()}");
        if (ImGui.Button("reload Chunks")) {
            Clear();
        }

        if (ImGui.CollapsingHeader("chunks", ImGuiTreeNodeFlags.Bullet)) {
            if (ImGui.BeginChild("chunksRegion", new Vector2(0, 300), false, ImGuiWindowFlags.HorizontalScrollbar)) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
                foreach (Chunk chunk in GetChunksList()) {
                    ImGui.Text($"chunk {chunk.position} {chunk.chunkState}");
                }

                ImGui.PopStyleVar();
            }

            ImGui.EndChild();
        }
    }


    
}