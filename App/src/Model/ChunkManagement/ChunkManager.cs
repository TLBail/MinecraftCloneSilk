using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model.Lighting;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkManager : IChunkManager
{
    internal readonly ConcurrentDictionary<Vector3D<int>, Chunk> chunks;
    public IWorldGenerator worldGenerator { get; set; }
    private object lockChunksToUpdate = new object();
    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private ChunkPool chunkPool;
    private ChunkLoader chunkLoader;
    private ChunkUnloader chunkUnloader;

    public Vector3D<int> centerChunk = new Vector3D<int>(-1);

    public ChunkManager(int radius, IWorldGenerator worldGenerator, IChunkStorage chunkStorage, IChunkLightManager chunkLightManager) {
        chunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2,
            radius * radius * radius);
        this.worldGenerator = worldGenerator;
        chunkLoader = new ChunkLoader(ChunkLoader.ChunkLoaderMode.ASYNC);
        chunkPool = new ChunkPool(this, worldGenerator, chunkStorage, chunkLightManager);
        chunkUnloader = new ChunkUnloader(this, chunkPool, chunkStorage);
    }

    [Logger.Timer]
    public void Update(double deltatime) {
        chunkLoader.Update(); 
        chunkUnloader.Update();
        List<Chunk> chunksToUpdateCopy;
        lock (lockChunksToUpdate) {
            chunksToUpdateCopy = new List<Chunk>(this.chunksToUpdate);
        }
        Debug.Assert(!chunksToUpdateCopy.Any((chunk) => chunk is null));
        foreach (Chunk chunk in chunksToUpdateCopy) {
            Debug.Assert(chunk is not null, "chunk must be not null");
            Debug.Assert(chunk.chunkState >= ChunkState.DRAWABLE, $"try to update a chunk:{chunk} with a lower state than the minimum");
            chunk.Update(deltatime);
        }
    }

    public int Count() => chunks.Count;
    public ICollection<Chunk> GetChunksList() => chunks.Values;
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);

    public void Clear() {
        while(chunkLoader.HaveTasks()) chunkLoader.Update();
        
        List<Chunk> chunksCopy = new List<Chunk>(chunks.Values);
        foreach (Chunk chunk in chunksCopy) {
            chunkUnloader.ForceUnloadChunk(chunk);
        }
        //Assert chunks is empty
        Debug.Assert(chunks.Count == 0);
        
    }

    public Chunk GetChunk(Vector3D<int> position) {
        if (!chunks.TryGetValue(position, out Chunk? chunk)) {
            chunk = chunks.GetOrAdd(position, chunkPool.Get);
        }
        return chunk;
    }

    public Chunk GetBlockGeneratedChunk(Vector3D<int> position) {
        return ChunkManagerTools.GetBlockGeneratedChunk(this, chunkLoader, position);
    }
    
    public bool ContainChunk(Vector3D<int> position) {
        return chunks.ContainsKey(position);
    }

    public void AddChunkToUpdate(Chunk chunk) {
        Debug.Assert(chunk is not null, "try to add a null chunk to update");
        lock (lockChunksToUpdate) {
            chunksToUpdate.Add(chunk);
        }
    }

    public void RemoveChunkToUpdate(Chunk chunk) {
        lock (lockChunksToUpdate) {
            chunksToUpdate.Remove(chunk);
        }
    }

    [Logger.Timer]
    public void LoadChunkAroundACenter(Vector3D<int> newCenterChunk, int radius) {
        if (newCenterChunk == centerChunk) return;
        centerChunk = newCenterChunk;
        List<Vector3D<int>> positionsRelevant = new List<Vector3D<int>>();
        var rootChunk = centerChunk + new Vector3D<int>((int)(-radius * Chunk.CHUNK_SIZE));
        for (var x = 0; x < 2 * radius; x++)
        for (var y = 0; y < 2 * radius; y++)
        for (var z = 0; z < 2 * radius; z++) {
            var key = rootChunk + new Vector3D<int>((int)(x * Chunk.CHUNK_SIZE), (int)(y * Chunk.CHUNK_SIZE),
                (int)(z * Chunk.CHUNK_SIZE));
            if(Vector3D.Distance(centerChunk, key) > radius * Chunk.CHUNK_SIZE) continue;
            if (IsChunkRelevantForLoading(key)) {
                positionsRelevant.Add(key);
            }
        }
        AddChunksToLoad(positionsRelevant);

        chunkUnloader.SetCenterOfUnload(centerChunk, radius + 2);
    }
    
    
    private bool IsChunkRelevantForLoading(Vector3D<int> position) {
        if (!chunks.TryGetValue(position, out Chunk? chunk)) {
            return true;
        }
        if (chunk.IsRequiredByChunkLoader()) return false;
        return chunk.chunkState < ChunkState.DRAWABLE;
    }
    

    public void AddChunkToLoad(Vector3D<int> position) {
        position = World.GetChunkPosition(position);
        AddChunksToLoad(new List<Vector3D<int>> { position });
    }

    [Logger.Timer]
    public void AddChunksToLoad(List<Vector3D<int>> positions) {
        foreach (Vector3D<int> position in positions) {
            Chunk chunk = GetChunk(position);
            chunkLoader.AddChunkToQueue(chunk);
        }
    }
    public bool TryToUnloadChunk(Vector3D<int> position) => chunkUnloader.TryToUnloadChunk(position);

    public void ToImGui() {
        ImGui.Text("Chunk Manager: ");
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