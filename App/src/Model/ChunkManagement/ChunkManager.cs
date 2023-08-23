using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk.Model;

public class ChunkManager : IChunkManager, IDisposable
{
    private readonly ConcurrentDictionary<Vector3D<int>, Chunk> chunks;
    public WorldGenerator worldGenerator { get; set; }
    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private ChunkPool chunkPool;
    private ChunkLoader? chunkLoader;
    private IChunkStorage chunkStorage;
    private List<Chunk> chunksToUnload;

    public ChunkManager(int radius, WorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        chunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2,
            radius * radius * radius);
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        chunkPool = new ChunkPool(this, worldGenerator, chunkStorage);
        chunksToUnload = new List<Chunk>();
    }

    [Logger.Timer]
    public void update(double deltatime) {
        if (chunkLoader == null) {
            if(chunksToUnload.Count > 0) {
                chunkStorage.SaveChunks(chunksToUnload);
                chunksToUnload.Clear();
            }
        }else if(chunkLoader.update()) {
            chunkLoader = null;
        }
        foreach (Chunk chunk in chunksToUpdate) {
            chunk.Update(deltatime);
        }
    }

    public int count() => chunks.Count;
    public List<Chunk> getChunksList() => new List<Chunk>(chunks.Values);
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);


    public void clear() {
        List<Chunk> chunksCopy = new List<Chunk>(chunks.Values);
        List<Chunk> chunkRemaining = new List<Chunk>();
        foreach (Chunk chunk in chunksCopy) {
            forceUnloadChunk(chunk);
        }
    }

    public Chunk getChunk(Vector3D<int> position) {
        Chunk chunk = chunks.GetOrAdd(position, chunkPool.get);
        return chunk;
    }

    public void addChunkToUpdate(Chunk chunk) {
        chunksToUpdate.Add(chunk);
    }

    public void removeChunkToUpdate(Chunk chunk) {
        chunksToUpdate.Remove(chunk);
    }

    public void updateRelevantChunks(List<Vector3D<int>> chunkRelevant) {
        List<Vector3D<int>> chunkNotContainInChunks = new List<Vector3D<int>>();
        foreach (Vector3D<int> position in chunkRelevant) {
            if (!chunks.TryGetValue(position, out Chunk chunk) || chunk.chunkState < ChunkState.DRAWABLE) {
                chunkNotContainInChunks.Add(position);
            }
        }

        addChunksToLoad(chunkNotContainInChunks);
        IEnumerable<Vector3D<int>> chunksToDeletePosition = chunks.Keys.Except(chunkRelevant);
        foreach (var chunkToDeletePosition in chunksToDeletePosition) tryToUnloadChunk(chunkToDeletePosition);
    }

    public void addChunkToLoad(Vector3D<int> position) {
        position = World.getChunkPosition(position);
        addChunksToLoad(new List<Vector3D<int>> { position });
    }

    public void addChunksToLoad(List<Vector3D<int>> positions) {
        if(this.chunkLoader != null) return; 
        Stack<ChunkLoadingTask> chunksToLoad = new Stack<ChunkLoadingTask>(positions.Count);
        foreach (Vector3D<int> position in positions) {
            var chunk = getChunk(position);
            if (chunk.chunkState < ChunkState.DRAWABLE)
                chunksToLoad.Push(new ChunkLoadingTask(chunk, ChunkState.DRAWABLE));
        }
        chunkLoader = new ChunkLoader(chunkStorage);
        chunkLoader.addChunks(ChunkManagerTools.getChunkDependent(this,chunksToLoad));
    }


    public bool tryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunks[position];
        if(chunkToUnload.isRequiredByChunkLoader) return false;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(position, chunkToUnload));
        chunkToUnload.Dispose();
        chunksToUnload.Add(chunkToUnload);
        return true;
    }

    private void forceUnloadChunk(Chunk chunkToUnload) {
        if(chunkToUnload.isRequiredByChunkLoader) return;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunkToUnload.position, chunkToUnload));
        chunkToUnload.Dispose();
        chunksToUnload.Add(chunkToUnload);
    }
    

    public void toImGui() {
        ImGui.Text("number of chunks " + chunks.Count);
        ImGui.Text("number of chunks in pool " + chunkPool.count());
        if (ImGui.Button("reload Chunks")) {
            clear();
        }

        if (ImGui.CollapsingHeader("chunks", ImGuiTreeNodeFlags.Bullet)) {
            if (ImGui.BeginChild("chunksRegion", new Vector2(0, 300), false, ImGuiWindowFlags.HorizontalScrollbar)) {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));
                foreach (Chunk chunk in getChunksList()) {
                    ImGui.Text("chunk " + chunk.position + " " + chunk.chunkState);
                }

                ImGui.PopStyleVar();
            }

            ImGui.EndChild();
        }
    }

    public void Dispose() {
        chunkPool.Dispose();
    }


    
}