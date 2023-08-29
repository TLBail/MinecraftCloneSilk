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
    private ChunkLoader? chunkLoader;
    private IChunkStorage chunkStorage;
    private List<Chunk> chunksToUnload;

    public ChunkManager(int radius, IWorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        chunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2,
            radius * radius * radius);
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        chunkPool = new ChunkPool(this, worldGenerator, chunkStorage);
        chunksToUnload = new List<Chunk>();
    }

    [Logger.Timer]
    public void Update(double deltatime) {
        if (chunkLoader == null) {
            if(chunksToUnload.Count > 0) {
                chunkStorage.SaveChunks(chunksToUnload);
                chunkPool.ReturnChunks(chunksToUnload);
                chunksToUnload.Clear();
            }
        }else if(chunkLoader.Update()) {
            chunkLoader = null;
        }
        foreach (Chunk chunk in chunksToUpdate) {
            chunk.Update(deltatime);
        }
    }

    public int Count() => chunks.Count;
    public List<Chunk> GetChunksList() => new List<Chunk>(chunks.Values);
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);


    public void Clear() {
        List<Chunk> chunksCopy = new List<Chunk>(chunks.Values);
        List<Chunk> chunkRemaining = new List<Chunk>();
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

    
    [Logger.Timer]
    public void UpdateRelevantChunks(List<Vector3D<int>> chunkRelevant) {
        List<Vector3D<int>> chunkNotContainInChunks = new List<Vector3D<int>>();
        foreach (Vector3D<int> position in chunkRelevant) {
            if (!chunks.TryGetValue(position, out Chunk? chunk) || chunk.chunkState < ChunkState.DRAWABLE) {
                chunkNotContainInChunks.Add(position);
            }
        }

        AddChunksToLoad(chunkNotContainInChunks);
        IEnumerable<Vector3D<int>> chunksToDeletePosition = chunks.Keys.Except(chunkRelevant);
        foreach (var chunkToDeletePosition in chunksToDeletePosition) TryToUnloadChunk(chunkToDeletePosition);
    }

    public void AddChunkToLoad(Vector3D<int> position) {
        position = World.GetChunkPosition(position);
        AddChunksToLoad(new List<Vector3D<int>> { position });
    }

    [Logger.Timer]
    public void AddChunksToLoad(List<Vector3D<int>> positions) {
        if (this.chunkLoader != null) {
            Console.WriteLine("warning try to load chunks while chunkLoader is not null");
            return;
        } 
        Stack<ChunkLoadingTask> chunksToLoad = new Stack<ChunkLoadingTask>(positions.Count);
        foreach (Vector3D<int> position in positions) {
            var chunk = GetChunk(position);
                chunksToLoad.Push(new ChunkLoadingTask(chunk, ChunkState.DRAWABLE));
        }
        chunkLoader = new ChunkLoader(chunkStorage);
        chunkLoader.AddChunks(ChunkLoader.GetChunkDependent(this,chunksToLoad));
    }


    public bool TryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunks[position];
        if(chunkToUnload.isRequiredByChunkLoader()) return false;
        ChunkState minimumChunkStateOfChunk = GetMinimumChunkStateOfChunk(position);
        if (chunkToUnload.chunkState == ChunkState.DRAWABLE) {
            chunkToUnload.SetChunkState(ChunkState.BLOCKGENERATED);
        }
        if(minimumChunkStateOfChunk > ChunkState.EMPTY) return false;
        
        if (chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(position, chunkToUnload))) {
            chunkToUnload.addRequiredByChunkUnloader(); 
            chunksToUnload.Add(chunkToUnload);
            return true;
        } else {
            Debug.Assert(false,"race condition while deleting chunk");
            return false;
        }
    }

    private void ForceUnloadChunk(Chunk chunkToUnload) {
        if(chunkToUnload.isRequiredByChunkLoader()) return;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunkToUnload.position, chunkToUnload));
        chunksToUnload.Add(chunkToUnload);
    }
    
    private ChunkState GetMinimumChunkStateOfChunk(Vector3D<int> position) {
        ChunkState chunkState = ChunkState.EMPTY;
        Chunk chunk;
        if (chunks.TryGetValue(position + new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0), out chunk) &&
            chunk.GetMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.GetMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0), out chunk) &&
            chunk.GetMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.GetMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, (int)Chunk.CHUNK_SIZE, 0), out chunk) &&
            chunk.GetMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.GetMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, -(int)Chunk.CHUNK_SIZE, 0), out chunk) &&
            chunk.GetMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.GetMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, 0, (int)Chunk.CHUNK_SIZE), out chunk) &&
            chunk.GetMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.GetMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, 0, -(int)Chunk.CHUNK_SIZE), out chunk) &&
            chunk.GetMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.GetMinimumChunkStateOfNeighbors();

        return chunkState;
    }

    public void ToImGui() {
        ImGui.Text("number of chunks " + chunks.Count);
        ImGui.Text("number of chunks in pool " + chunkPool.Count());
        if (ImGui.Button("reload Chunks")) {
            Clear();
        }

        if (ImGui.CollapsingHeader("chunks", ImGuiTreeNodeFlags.Bullet)) {
            if (ImGui.BeginChild("chunksRegion", new Vector2(0, 300), false, ImGuiWindowFlags.HorizontalScrollbar)) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
                foreach (Chunk chunk in GetChunksList()) {
                    ImGui.Text("chunk " + chunk.position + " " + chunk.chunkState);
                }

                ImGui.PopStyleVar();
            }

            ImGui.EndChild();
        }
    }


    
}