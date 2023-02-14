using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Model;

public class ChunkManager : IChunkManager, IDisposable
{
    
    private Dictionary<Vector3D<int>, Chunk> chunks;
    private object chunksLock = new object();
    public WorldGenerator worldGenerator { get;  set; }


    public struct ChunkLoadingTask
    {
        public Chunk chunk { get; set; }
        public ChunkState chunkState { get; set; }

        public ChunkLoadingTask(Chunk chunk, ChunkState chunkState) {
            this.chunk = chunk;
            this.chunkState = chunkState;
        }
    }
    
    private List<ChunkLoadingTask> chunksToLoad;
    private object chunksToLoadLock = new object();
    private Thread chunkLoaderThread;
    private bool runThread = true;
    private SemaphoreSlim semaphore;

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    private object chunksToUpdateLock = new object();
    private List<Chunk> chunksToDraw = new List<Chunk>();
    private object chunksToDrawLock = new object();

    private List<Chunk> chunksToUnload = new List<Chunk>();
    private object chunksToUnloadLock = new object();
    
    private ChunkPool chunkPool;

    public ChunkManager(int RADIUS, WorldGenerator worldGenerator) {
        chunks = new  Dictionary<Vector3D<int>, Chunk>(RADIUS * RADIUS * RADIUS);
        this.worldGenerator = worldGenerator;
        chunksToLoad = new List<ChunkLoadingTask>();
        semaphore = new SemaphoreSlim(0);
        chunkPool = new ChunkPool(this, worldGenerator);
        chunkLoaderThread = new Thread(chunkLoaderThreadRuntime);
        chunkLoaderThread.Start();
    }

    public void chunkLoaderThreadRuntime() {
        while (runThread) {
            semaphore.Wait();
            List<ChunkLoadingTask> chunksToLoadCopy;
            lock (chunksToLoadLock) {
                chunksToLoadCopy = new List<ChunkLoadingTask>(chunksToLoad);
                chunksToLoad.Clear();
            }
            foreach (ChunkLoadingTask task in chunksToLoadCopy) {
                task.chunk.setMinimumWantedChunkState(task.chunkState);
            }

            List<Chunk> chunksToUnloadCopy;
            lock (chunksToUnloadLock) {
                chunksToUnloadCopy = new List<Chunk>(chunksToUnload);
                chunksToUnload.Clear();
            }

            foreach (Chunk chunk in chunksToUnloadCopy) {
                chunk.save();
                chunkPool.returnChunk(chunk);
            }
            
        }
    }

    public void Draw(GL gl, double deltaTime) {
        lock (chunksToDrawLock) {
            foreach (var chunk in chunksToDraw) chunk.Draw(gl, deltaTime);
        }
    }
    
    
    public void update(double deltatime) {
        lock (chunksToUpdateLock) {
            foreach (Chunk chunk in chunksToUpdate) {
                chunk.Update(deltatime);
            }   
        }
    }

    public int count() => chunks.Count;
    public List<Chunk> getChunksList() => new List<Chunk>(chunks.Values);
    public bool ContainsKey(Vector3D<int> position) => chunks.ContainsKey(position);
    
    public void clear() {
        lock (chunksLock) {
            
            List<Chunk> chunksCopy = new List<Chunk>(chunks.Values);
            List<Chunk> chunkRemaining = new List<Chunk>();
            foreach (Chunk chunk in chunksCopy) {
                forceUnloadChunk(chunk);
            }
        }
    }
    
    public Chunk getChunk(Vector3D<int> position) {
        lock (chunksLock) {
            Chunk chunk = chunks.TryGetValue(position, out Chunk value) ? value : askForNewChunk(position);
            return chunk;
        }
    }

    private Chunk askForNewChunk(Vector3D<int> position) {
        Chunk chunk = chunkPool.get(position);
        chunks.Add(position, chunk);
        return chunk;
    }


    public void addChunkToDraw(Chunk chunk) {
        lock (chunksToDrawLock) {
            chunksToDraw.Add(chunk);
        }
    }

    public void addChunkToUpdate(Chunk chunk) {
        lock (chunksToUpdateLock) {
            chunksToUpdate.Add(chunk);
        }
    }

    public void removeChunkToUpdate(Chunk chunk) => chunksToUpdate.Remove(chunk);

    public void removeChunkToDraw(Chunk chunk) => chunksToDraw.Remove(chunk);

    public void updateRelevantChunks(List<Vector3D<int>> chunkRelevant) {
        lock (chunksLock) {
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
    }

    public void addChunkToLoad(Vector3D<int> position) {
        position = World.getChunkPosition(position);
        lock (chunksToLoadLock) {
            Chunk chunk;
            if (chunks.TryGetValue(position, out Chunk existingChunk)) {
                chunk = existingChunk;
            } else {
                chunk = chunkPool.get(position);
                chunks.Add(chunk.position, chunk);
            }
            ChunkLoadingTask chunkLoadingTask = new ChunkLoadingTask(chunk, ChunkState.DRAWABLE); 
            chunksToLoad.Add(chunkLoadingTask);
            if (semaphore.CurrentCount <= 0) semaphore.Release();
        }
    }

    public void addChunksToLoad(List<Vector3D<int>> positions) {
        lock (chunksToLoadLock) {
            foreach (Vector3D<int> position in positions) {
                Chunk chunk;
                if (chunks.TryGetValue(position, out Chunk existingChunk)) {
                    chunk = existingChunk;
                } else {
                    chunk = chunkPool.get(position);
                    chunks.Add(chunk.position, chunk);
                }
                ChunkLoadingTask chunkLoadingTask = new ChunkLoadingTask(chunk, ChunkState.DRAWABLE); 
                chunksToLoad.Add(chunkLoadingTask);
            }
        }
        if (semaphore.CurrentCount <= 0) semaphore.Release();
    }
    
    public bool tryToUnloadChunk(Vector3D<int> position) {
        Chunk chunkToUnload = null;
        lock (chunksToUnloadLock) {
            if (chunks.ContainsKey(position)) {
                chunkToUnload = chunks[position];
                if(chunkToUnload == null) return false;
                ChunkState minimumChunkState = getMinimumChunkStateOfChunk(position);
                if (minimumChunkState == ChunkState.EMPTY) {
                    chunks.Remove(position);
                    chunkToUnload.Dispose();
                    chunksToUnload.Add(chunkToUnload);
                    if (semaphore.CurrentCount <= 0) semaphore.Release();
                    return true;
                } else if(chunkToUnload.chunkState == ChunkState.DRAWABLE) {
                    chunkToUnload.setWantedChunkState(minimumChunkState);
                    return true;
                }
            }   
        }
        return false;
    }

    private void forceUnloadChunk(Chunk chunkToUnload) {
        lock (chunksToUnloadLock) {
            chunks.Remove(chunkToUnload.position);
            chunkToUnload.Dispose();
            chunksToUnload.Add(chunkToUnload);
            if (semaphore.CurrentCount <= 0) semaphore.Release();
        }
    }


    private ChunkState getMinimumChunkStateOfChunk(Vector3D<int> position) {
        ChunkState chunkState = ChunkState.EMPTY;
        Chunk chunk;
        if (chunks.TryGetValue(position + new Vector3D<int>((int)Chunk.CHUNK_SIZE, 0, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(-(int)Chunk.CHUNK_SIZE, 0, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, (int)Chunk.CHUNK_SIZE, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, -(int)Chunk.CHUNK_SIZE, 0), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, 0, (int)Chunk.CHUNK_SIZE), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        if (chunks.TryGetValue(position + new Vector3D<int>(0, 0, -(int)Chunk.CHUNK_SIZE), out chunk) && chunk.getMinimumChunkStateOfNeighbors() > chunkState)
            chunkState = chunk.getMinimumChunkStateOfNeighbors();
        
        return chunkState;
    }

    public void toImGui() {
        ImGui.Text("number of chunks " + chunks.Count);
        ImGui.Text("number of chunks in pool " + chunkPool.count());
        if (ImGui.Button("reload Chunks")) {
            clear();
        }

        if (ImGui.CollapsingHeader("chunks", ImGuiTreeNodeFlags.Bullet)) {
            if (ImGui.BeginChild("chunksRegion", new Vector2(0, 300), false,
                    ImGuiWindowFlags.HorizontalScrollbar)) {
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
        runThread = false;
        chunksToLoad.Clear();
        semaphore.Release();
        chunkLoaderThread.Join();
        chunkPool.Dispose();
    }

 
}