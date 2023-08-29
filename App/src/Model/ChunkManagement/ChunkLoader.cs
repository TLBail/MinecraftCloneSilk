using System.Diagnostics;
using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkLoader
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

    private List<Chunk> chunksList = new() ;
    private List<Chunk> chunkToLoadTerrain = new();
    private List<Chunk> chunkToLoadBlock = new ();
    private List<Chunk> chunkToDraw = new();
    private List<ChunkLoadingTask> chunkInMemory = new();
    private ChunkLoaderState chunkLoaderState;
    private Task task = null!;
    private IChunkStorage chunkStorage;

    public ChunkLoader(IChunkStorage chunkStorage) {
        this.chunkStorage = chunkStorage;
        chunkLoaderState = ChunkLoaderState.SETUPCHUNKINMEMORY;
    }
    
    [Logger.Timer]
    public static List<Chunk> GetChunkDependent(IChunkManager chunkManager, Stack<ChunkLoadingTask> chunksToLoad) {
        Dictionary<Vector3D<int>, Chunk> chunkLoadOrder = new Dictionary<Vector3D<int>, Chunk>(chunksToLoad.Count);
        while (chunksToLoad.TryPop(out ChunkLoadingTask chunkTask)) {
            Debug.Assert(!chunkTask.chunk.isRequiredByChunkUnloader());
            if (!chunkLoadOrder.ContainsKey(chunkTask.chunk.position)) {
                chunkTask.chunk.addRequiredByChunkLoader();
                chunkLoadOrder.Add(chunkTask.chunk.position, chunkTask.chunk);
            }
            
            if (chunkTask.chunk.wantedChunkState < chunkTask.wantedChunkState) {
                chunkTask.chunk.wantedChunkState = chunkTask.wantedChunkState;
            }
            Vector3D<int>[] dependantesChunkOffset =
                ChunkStrategy.GetDependanteChunksOffsetOfAChunkState(chunkTask.wantedChunkState);

            for (int i = 0; i < dependantesChunkOffset.Length; i++) {
                Vector3D<int> position = dependantesChunkOffset[i] + chunkTask.chunk.position;
                Chunk chunk = chunkManager.GetChunk(position);
                ChunkState wantedDependantChunkState =
                    ChunkStrategy.GetMinimumChunkStateOfNeighbors(chunkTask.wantedChunkState);
                chunksToLoad.Push(new ChunkLoadingTask(chunk, wantedDependantChunkState));
            }
        }
        return chunkLoadOrder.Values.ToList();
    }
    
    
    public void AddChunk(Chunk chunk) {
        if (chunk.chunkState >= chunk.wantedChunkState) return;
        ChunkState chunkStateInMemory = chunkStorage.GetChunkStateInStorage(chunk.position);
        if (chunkStateInMemory > chunk.chunkState) chunkInMemory.Add(new(chunk, chunkStateInMemory));
        if (chunkStateInMemory < ChunkState.GENERATEDTERRAIN && chunk.chunkState < ChunkState.GENERATEDTERRAIN &&
            chunk.wantedChunkState >= ChunkState.GENERATEDTERRAIN)
            chunkToLoadTerrain.Add(chunk);
        if (chunkStateInMemory < ChunkState.BLOCKGENERATED && chunk.chunkState < ChunkState.BLOCKGENERATED &&
            chunk.wantedChunkState >= ChunkState.BLOCKGENERATED)
            chunkToLoadBlock.Add(chunk);
        if (chunk.chunkState < ChunkState.DRAWABLE && chunk.wantedChunkState >= ChunkState.DRAWABLE)
            chunkToDraw.Add(chunk);
    }

    public bool Update() {
        return SingleThreadLoading();
    }

    public void Reset() {
        chunkToDraw = new List<Chunk>();
        chunkToLoadBlock = new List<Chunk>();
        chunkToLoadTerrain = new List<Chunk>();
        chunkInMemory = new List<ChunkLoadingTask>();
        chunkLoaderState = ChunkLoaderState.SETUPCHUNKINMEMORY;
    }


    public bool SingleThreadLoading() {
        foreach (ChunkLoadingTask chunkTask in chunkInMemory) {
            chunkTask.chunk.SetChunkState(chunkTask.wantedChunkState);
            chunkStorage.LoadChunk(chunkTask.chunk);
        }

        // case ChunkLoaderState.SETUPTERRAIN:
        foreach (Chunk chunk in chunkToLoadTerrain) {
            chunk.SetChunkState(ChunkState.GENERATEDTERRAIN);
            chunk.InitChunkState();
        }

        // case ChunkLoaderState.LOADTERRAIN:
        foreach (Chunk chunk in chunkToLoadTerrain) {
            chunk.LoadChunkState();
        }

        // case ChunkLoaderState.FINISHTERRAIN:
        foreach (Chunk chunk in chunkToLoadTerrain) {
            chunk.FinishChunkState();
        }

        // case ChunkLoaderState.SETUPBLOCK:
        foreach (Chunk chunk in chunkToLoadBlock) {
            chunk.SetChunkState(ChunkState.BLOCKGENERATED);
            chunk.InitChunkState();
        }

        // case ChunkLoaderState.LOADBLOCK:
        foreach (Chunk chunk in chunkToLoadBlock) {
            chunk.LoadChunkState();
        }

        // case ChunkLoaderState.FINISHBLOCK:
        foreach (Chunk chunk in chunkToLoadBlock) {
            chunk.FinishChunkState();
        }

        // case ChunkLoaderState.SETUPDRAW:
        foreach (Chunk chunk in chunkToDraw) {
            chunk.SetChunkState(ChunkState.DRAWABLE);
            chunk.InitChunkState();
        }

        // case ChunkLoaderState.LOADDRAW:
        foreach (Chunk chunk in chunkToDraw) {
            chunk.LoadChunkState();
        }

        // case ChunkLoaderState.FINISHDRAW:
        foreach (Chunk chunk in chunkToDraw) {
            chunk.FinishChunkState();
        }

        foreach (Chunk chunk in chunksList) {
            chunk.removeRequiredByChunkLoader();
        }
    
        Reset();
        return true;
    }

    public bool MultiThreadLoading() {
        switch (chunkLoaderState) {
            case ChunkLoaderState.SETUPCHUNKINMEMORY:
                foreach (ChunkLoadingTask chunk in chunkInMemory) {
                    chunk.chunk.SetChunkState(chunk.wantedChunkState);
                }

                chunkLoaderState = ChunkLoaderState.LOADCHUNKINMEMORY;
                task = Task.Factory.StartNew(() =>
                {
                    chunkStorage.LoadChunks(chunkInMemory.Select(chunk => chunk.chunk).ToList());
                }).ContinueWith((task) =>
                {
                    if (task.IsFaulted) throw task.Exception!;
                    chunkLoaderState = ChunkLoaderState.FINISHCHUNKINMEMORY;
                });
                return false;
            case ChunkLoaderState.LOADCHUNKINMEMORY:
                if (task.IsFaulted) throw task.Exception!;
                return false;
            case ChunkLoaderState.FINISHCHUNKINMEMORY:
                foreach (ChunkLoadingTask chunkLoadingTask in chunkInMemory) {
                    chunkLoadingTask.chunk.FinishChunkState();
                }

                chunkLoaderState = ChunkLoaderState.SETUPTERRAIN;
                return false;
            case ChunkLoaderState.SETUPTERRAIN:
                foreach (Chunk chunk in chunkToLoadTerrain) {
                    chunk.SetChunkState(ChunkState.GENERATEDTERRAIN);
                    chunk.InitChunkState();
                }
                chunkLoaderState = ChunkLoaderState.LOADTERRAIN;
                task = Parallel.ForEachAsync(chunkToLoadTerrain, (chunk, token) =>
                    {
                        chunk.LoadChunkState();
                        return default;
                    })
                    .ContinueWith((task =>
                    {
                        if (task.IsFaulted) throw task.Exception!;
                        chunkLoaderState = ChunkLoaderState.FINISHTERRAIN;
                    }));
                return false;
            case ChunkLoaderState.LOADTERRAIN:
                if (task.IsFaulted) throw task.Exception!;
                return false;
            case ChunkLoaderState.FINISHTERRAIN:
                foreach (Chunk chunk in chunkToLoadTerrain) {
                    chunk.FinishChunkState();
                }

                chunkLoaderState = ChunkLoaderState.SETUPBLOCK;
                return false;
            case ChunkLoaderState.SETUPBLOCK:
                foreach (Chunk chunk in chunkToLoadBlock) {
                    chunk.SetChunkState(ChunkState.BLOCKGENERATED);
                    chunk.InitChunkState();
                }

                chunkLoaderState = ChunkLoaderState.LOADBLOCK;
                task = Parallel.ForEachAsync(chunkToLoadBlock,  (chunk, token) =>
                    {
                        chunk.LoadChunkState();
                        return default;
                    })
                    .ContinueWith((task =>
                    {
                        if (task.IsFaulted) throw task.Exception!;
                        chunkLoaderState = ChunkLoaderState.FINISHBLOCK;
                    }));
                return false;
            case ChunkLoaderState.LOADBLOCK:
                if (task.IsFaulted) throw task.Exception!;
                return false;
            case ChunkLoaderState.FINISHBLOCK:
                foreach (Chunk chunk in chunkToLoadBlock) {
                    chunk.FinishChunkState();
                }

                chunkLoaderState = ChunkLoaderState.SETUPDRAW;
                return false;
            case ChunkLoaderState.SETUPDRAW:
                foreach (Chunk chunk in chunkToDraw) {
                    chunk.SetChunkState(ChunkState.DRAWABLE);
                    chunk.InitChunkState();
                }

                chunkLoaderState = ChunkLoaderState.LOADDRAW;
                task = Parallel.ForEachAsync(chunkToDraw,  (chunk, token) =>
                    {
                        chunk.LoadChunkState();
                        return default;
                    })
                    .ContinueWith((task =>
                    {
                        if (task.IsFaulted) throw task.Exception!;
                        chunkLoaderState = ChunkLoaderState.FINISHDRAW;
                    }));
                return false;
            case ChunkLoaderState.LOADDRAW:
                if (task.IsFaulted) throw task.Exception!;
                return false;
            case ChunkLoaderState.FINISHDRAW:
                foreach (Chunk chunk in chunkToDraw) {
                    chunk.FinishChunkState();
                }

                foreach (Chunk chunk in chunksList) {
                    chunk.removeRequiredByChunkLoader();
                }

                return true;

            default:
                throw new Exception("chunkLoaderState not handled");
        }
    }


public void AddChunks(List<Chunk> chunksList) {
        Debug.Assert(this.chunksList.Count == 0);
        this.chunksList = chunksList;
        foreach (Chunk chunk in chunksList) {
            AddChunk(chunk);
        }
    }
}