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
    private ChunkLoader? chunkLoader;

    private record struct ChunkLoadingTask(Chunk chunk, ChunkState wantedChunkState);

    public ChunkManager(int radius, WorldGenerator worldGenerator, ChunkStorage chunkStorage) {
        chunks = new ConcurrentDictionary<Vector3D<int>, Chunk>(Environment.ProcessorCount * 2,
            radius * radius * radius);
        this.worldGenerator = worldGenerator;
        chunkPool = new ChunkPool(this, worldGenerator, chunkStorage);
    }

    public void update(double deltatime) {
        if(chunkLoader != null && chunkLoader.update()) {
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

        chunkLoader = new ChunkLoader();
        List<Chunk> chunkLoadOrder = new List<Chunk>(chunksToLoad.Count);
        while (chunksToLoad.TryPop(out ChunkLoadingTask chunkTask)) {
            
            if (chunkTask.chunk.wantedChunkState < chunkTask.wantedChunkState) {
                chunkTask.chunk.wantedChunkState = chunkTask.wantedChunkState;
            }

            if (!chunkTask.chunk.isRequiredByChunkLoader) {
                chunkTask.chunk.isRequiredByChunkLoader = true;
                chunkLoadOrder.Add(chunkTask.chunk);
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
    }


    public bool tryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = chunks[position];
        if(chunkToUnload.isRequiredByChunkLoader) return false;
        chunks.TryRemove(new KeyValuePair<Vector3D<int>, Chunk>(position, chunkToUnload));
        chunkToUnload.Dispose();
        chunkToUnload.save();
        chunkPool.returnChunk(chunkToUnload);
        return true;
    }

    private void forceUnloadChunk(Chunk chunkToUnload) {
        if(chunkToUnload.isRequiredByChunkLoader) return;
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
        

        private enum ChunkLoaderState
        {
            SETUPCHUNKINMEMORY,
            LOADCHUNKINMEMORY,
            FINISHCHUNKINMEMORY,
            SETUPTERRAIN,
            LOADTERRAIN,
            FINISHTERRAIN,
            SETUPBLOCK,
            LOADBLOCK,
            FINISHBLOCK,
            SETUPDRAW,
            LOADDRAW,
            FINISHDRAW,
        }
        public List<Chunk> chunksList;
        public List<Chunk> chunkToLoadTerrain;
        public List<Chunk> chunkToLoadBlock;
        public List<Chunk> chunkToDraw;
        public List<ChunkLoadingTask> chunkInMemory;
        private ChunkLoaderState chunkLoaderState;
        private Task task;
        
        public ChunkLoader() {
            chunkToDraw = new List<Chunk>();
            chunkToLoadBlock = new List<Chunk>();
            chunkToLoadTerrain = new List<Chunk>();
            chunkInMemory = new List<ChunkLoadingTask>();
            chunkLoaderState  = ChunkLoaderState.SETUPCHUNKINMEMORY;
        }

        public void addChunk(Chunk chunk) {
            if(chunk.chunkState >= chunk.wantedChunkState) return;
            ChunkState chunkStateInMemory = chunk.chunkStorage.getChunkStateInMemory(chunk.position);
            if(chunkStateInMemory > chunk.chunkState) chunkInMemory.Add(new( chunk, chunkStateInMemory));
            if (chunkStateInMemory < ChunkState.GENERATEDTERRAIN && chunk.chunkState < ChunkState.GENERATEDTERRAIN && chunk.wantedChunkState >= ChunkState.GENERATEDTERRAIN)
                chunkToLoadTerrain.Add(chunk);
            if (chunkStateInMemory < ChunkState.BLOCKGENERATED && chunk.chunkState < ChunkState.BLOCKGENERATED && chunk.wantedChunkState >= ChunkState.BLOCKGENERATED)
                chunkToLoadBlock.Add(chunk);
            if (chunk.chunkState < ChunkState.DRAWABLE && chunk.wantedChunkState >= ChunkState.DRAWABLE)
                chunkToDraw.Add(chunk);
        }

        public bool update() {
            return multiThreadLoading();
        }


        private bool singleThreadLoading() {
            switch (chunkLoaderState) {
                case ChunkLoaderState.SETUPCHUNKINMEMORY:
                    foreach (ChunkLoadingTask chunk in chunkInMemory) {
                        chunk.chunk.setChunkState(chunk.wantedChunkState);
                        chunk.chunk.loadChunkState();
                        chunk.chunk.finishChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.SETUPTERRAIN;
                    return false;
                case ChunkLoaderState.SETUPTERRAIN:
                    foreach (Chunk chunk in chunkToLoadTerrain) {
                        chunk.setChunkState(ChunkState.GENERATEDTERRAIN);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADTERRAIN;
                    return false;
                case ChunkLoaderState.LOADTERRAIN:
                    foreach (Chunk chunk in chunkToLoadTerrain) {
                        chunk.loadChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.FINISHTERRAIN;
                    return false;
                case ChunkLoaderState.FINISHTERRAIN:
                    foreach (Chunk chunk in chunkToLoadTerrain) {
                        chunk.finishChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.SETUPBLOCK;
                    return false;
                case ChunkLoaderState.SETUPBLOCK:
                    foreach (Chunk chunk in chunkToLoadBlock) {
                        chunk.setChunkState(ChunkState.BLOCKGENERATED);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADBLOCK;
                    return false;
                case ChunkLoaderState.LOADBLOCK:
                    foreach (Chunk chunk in chunkToLoadBlock) {
                        chunk.loadChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.FINISHBLOCK;
                    return false;
                case ChunkLoaderState.FINISHBLOCK:
                    foreach (Chunk chunk in chunkToLoadBlock) {
                        chunk.finishChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.SETUPDRAW;
                    return false;
                case ChunkLoaderState.SETUPDRAW:
                    foreach (Chunk chunk in chunkToDraw) {
                        chunk.setChunkState(ChunkState.DRAWABLE);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADDRAW;
                    return false;
                case ChunkLoaderState.LOADDRAW:
                    foreach (Chunk chunk in chunkToDraw) {
                        chunk.loadChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.FINISHDRAW;
                    return false;
                case ChunkLoaderState.FINISHDRAW:
                    foreach (Chunk chunk in chunkToDraw) {
                        chunk.finishChunkState();
                    }
                    foreach (Chunk chunk in chunksList) {
                        chunk.isRequiredByChunkLoader = false;
                    }
                    return true;
                
                default:
                    throw new Exception("chunkLoaderState not handled");
            }
        }
        
        private bool multiThreadLoading() {
            switch (chunkLoaderState) {
                case ChunkLoaderState.SETUPCHUNKINMEMORY:
                    foreach (ChunkLoadingTask chunk in chunkInMemory) {
                        chunk.chunk.setChunkState(chunk.wantedChunkState);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADCHUNKINMEMORY;
                    task = Parallel.ForEachAsync(chunkInMemory, async (chunk, token) =>
                    {
                        chunk.chunk.loadChunkState();
                    }).ContinueWith((task) =>
                    {
                        if(task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHCHUNKINMEMORY;
                    });
                    return false;
                case ChunkLoaderState.LOADCHUNKINMEMORY:
                    if (task.IsFaulted) throw task.Exception;
                    return false;
                case ChunkLoaderState.FINISHCHUNKINMEMORY:
                    foreach (ChunkLoadingTask chunkLoadingTask in chunkInMemory) {
                        chunkLoadingTask.chunk.finishChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.SETUPTERRAIN;
                    return false;
                case ChunkLoaderState.SETUPTERRAIN:
                    foreach (Chunk chunk in chunkToLoadTerrain) {
                        chunk.setChunkState(ChunkState.GENERATEDTERRAIN);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADTERRAIN;
                    task = Parallel.ForEachAsync(chunkToLoadTerrain, async (chunk, token) =>
                    {
                        chunk.loadChunkState();
                    }).ContinueWith((task =>
                    {
                        if(task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHTERRAIN;
                    }));
                    return false;
                case ChunkLoaderState.LOADTERRAIN:
                    if(task.IsFaulted) throw task.Exception;
                    return false;
                case ChunkLoaderState.FINISHTERRAIN:
                    foreach (Chunk chunk in chunkToLoadTerrain) {
                        chunk.finishChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.SETUPBLOCK;
                    return false;
                case ChunkLoaderState.SETUPBLOCK:
                    foreach (Chunk chunk in chunkToLoadBlock) {
                        chunk.setChunkState(ChunkState.BLOCKGENERATED);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADBLOCK;
                    task = Parallel.ForEachAsync(chunkToLoadBlock, async (chunk, token) =>
                    {
                        chunk.loadChunkState();
                    }).ContinueWith((task =>
                    {
                        if(task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHBLOCK;
                    }));
                    return false;
                case ChunkLoaderState.LOADBLOCK:
                    if(task.IsFaulted) throw task.Exception;
                    return false;
                case ChunkLoaderState.FINISHBLOCK:
                    foreach (Chunk chunk in chunkToLoadBlock) {
                        chunk.finishChunkState();
                    }
                    chunkLoaderState = ChunkLoaderState.SETUPDRAW;
                    return false;
                case ChunkLoaderState.SETUPDRAW:
                    foreach (Chunk chunk in chunkToDraw) {
                        chunk.setChunkState(ChunkState.DRAWABLE);
                    }
                    chunkLoaderState = ChunkLoaderState.LOADDRAW;
                    task = Parallel.ForEachAsync(chunkToDraw, async (chunk, token) =>
                    {
                        chunk.loadChunkState();
                    }).ContinueWith((task =>
                    {
                        if(task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHDRAW;
                    }));
                    return false;
                case ChunkLoaderState.LOADDRAW:
                    if(task.IsFaulted) throw task.Exception;
                    return false;
                case ChunkLoaderState.FINISHDRAW:
                    foreach (Chunk chunk in chunkToDraw) {
                        chunk.finishChunkState();
                    }
                    foreach (Chunk chunk in chunksList) {
                        chunk.isRequiredByChunkLoader = false;
                    }
                    return true;
                
                default:
                    throw new Exception("chunkLoaderState not handled");
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