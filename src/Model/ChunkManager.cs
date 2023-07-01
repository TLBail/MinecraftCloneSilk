using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
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
    private Stack<ChunkLoader> chunkLoaders;

    private record struct ChunkLoadingTask(Chunk chunk, ChunkState wantedChunkState);

    public ChunkManager(int radius, WorldGenerator worldGenerator, ChunkStorage chunkStorage) {
        chunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2,
            radius * radius * radius);
        this.worldGenerator = worldGenerator;
        chunkPool = new ChunkPool(this, worldGenerator, chunkStorage);
        chunkLoaders = new Stack<ChunkLoader>();
    }

    public void update(double deltatime) {
        chunkLoaders.TryPop(out ChunkLoader? chunkLoader);
        chunkLoader?.load();
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
        Stack<ChunkLoadingTask> chunksToLoad = new Stack<ChunkLoadingTask>(positions.Count);
        foreach (Vector3D<int> position in positions) {
            var chunk = getChunk(position);
            if (chunk.chunkState < ChunkState.DRAWABLE)
                chunksToLoad.Push(new ChunkLoadingTask(chunk, ChunkState.DRAWABLE));
        }

        ChunkLoader chunkLoader = new ChunkLoader();
        List<Chunk> chunkLoadOrder = new List<Chunk>(chunksToLoad.Count);
        while (chunksToLoad.TryPop(out ChunkLoadingTask chunkTask)) {
            chunkLoadOrder.Add(chunkTask.chunk);
            if (chunkTask.chunk.wantedChunkState < chunkTask.wantedChunkState) {
                chunkTask.chunk.wantedChunkState = chunkTask.wantedChunkState;
            }

            //récupère les chunks Dépendants de chunkLoadingTask.chunk
            Vector3D<int>[] dependantesChunkOffset =
                ChunkStrategy.getDependanteChunksOffsetOfAChunkState(chunkTask.wantedChunkState);

            for (int i = 0; i < dependantesChunkOffset.Length; i++) {
                Vector3D<int> position = dependantesChunkOffset[i] + chunkTask.chunk.position;
                Chunk chunk = getChunk(position);
                ChunkState wantedDependantChunkState =
                    ChunkStrategy.getMinimumChunkStateOfNeighbors(chunkTask.wantedChunkState);
                chunksToLoad.Push(new ChunkLoadingTask(chunk, wantedDependantChunkState));
            }
        }

        chunkLoader.addChunks(chunkLoadOrder);
        chunkLoaders.Push(chunkLoader);
    }


    public bool tryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunks[position];
        if(chunkToUnload.nbRequiredByChunkLoader > 0) return false;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(position, chunkToUnload));
        chunkToUnload.Dispose();
        chunkToUnload.save();
        chunkPool.returnChunk(chunkToUnload);
        return true;
    }

    private void forceUnloadChunk(Chunk chunkToUnload) {
        if(chunkToUnload.nbRequiredByChunkLoader > 0) return;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(chunkToUnload.position, chunkToUnload));
        chunkToUnload.Dispose();
        chunkToUnload.save();
        chunkPool.returnChunk(chunkToUnload);
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


    private class ChunkLoader
    {
        public List<Chunk> chunksList;
        public List<Chunk> chunkToLoadTerrain;
        public List<Chunk> chunkToLoadBlock;
        public List<Chunk> chunkToDraw;

        public ChunkLoader() {
            chunkToDraw = new List<Chunk>();
            chunkToLoadBlock = new List<Chunk>();
            chunkToLoadTerrain = new List<Chunk>();
        }

        public void addChunk(Chunk chunk) {
            chunk.nbRequiredByChunkLoader++;
            if (chunk.chunkState < ChunkState.GENERATEDTERRAIN && chunk.wantedChunkState >= ChunkState.GENERATEDTERRAIN)
                chunkToLoadTerrain.Add(chunk);
            if (chunk.chunkState < ChunkState.BLOCKGENERATED && chunk.wantedChunkState >= ChunkState.BLOCKGENERATED)
                chunkToLoadBlock.Add(chunk);
            if (chunk.chunkState < ChunkState.DRAWABLE && chunk.wantedChunkState >= ChunkState.DRAWABLE)
                chunkToDraw.Add(chunk);
        }

        public void load() {
            foreach (Chunk chunk in chunkToLoadTerrain) {
                chunk.setChunkState(ChunkState.GENERATEDTERRAIN);
            }
            foreach (Chunk chunk in chunkToLoadBlock) {
                chunk.setChunkState(ChunkState.BLOCKGENERATED);
            }
            foreach (Chunk chunk in chunkToDraw) {
                chunk.setChunkState(ChunkState.DRAWABLE);
            }
            foreach (Chunk chunk in chunksList) {
                chunk.nbRequiredByChunkLoader--;
            }
        }


        public void addChunks(List<Chunk> chunksList) {
            this.chunksList = chunksList;
            foreach (Chunk chunk in chunksList) {
                addChunk(chunk);
            }
        }
    }
}