using MinecraftCloneSilk.Model.NChunk;
using MinecraftCloneSilk.Model.Storage;

namespace MinecraftCloneSilk.Model;

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

    public List<Chunk> chunksList;
    public List<Chunk> chunkToLoadTerrain;
    public List<Chunk> chunkToLoadBlock;
    public List<Chunk> chunkToDraw;
    public List<ChunkLoadingTask> chunkInMemory;
    private ChunkLoaderState chunkLoaderState;
    private Task task;
    private IChunkStorage chunkStorage;

    public ChunkLoader(IChunkStorage chunkStorage) {
        this.chunkStorage = chunkStorage;
        reset();
    }

    public void addChunk(Chunk chunk) {
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

    public bool update() {
        return multiThreadLoading();
    }

    public void reset() {
        chunkToDraw = new List<Chunk>();
        chunkToLoadBlock = new List<Chunk>();
        chunkToLoadTerrain = new List<Chunk>();
        chunkInMemory = new List<ChunkLoadingTask>();
        chunkLoaderState = ChunkLoaderState.SETUPCHUNKINMEMORY;
    }


    public bool singleThreadLoading() {
        foreach (ChunkLoadingTask chunkTask in chunkInMemory) {
            chunkTask.chunk.setChunkState(chunkTask.wantedChunkState);
            chunkStorage.LoadChunk(chunkTask.chunk);
        }

        // case ChunkLoaderState.SETUPTERRAIN:
        foreach (Chunk chunk in chunkToLoadTerrain) {
            chunk.setChunkState(ChunkState.GENERATEDTERRAIN);
            chunk.initChunkState();
        }

        // case ChunkLoaderState.LOADTERRAIN:
        foreach (Chunk chunk in chunkToLoadTerrain) {
            chunk.loadChunkState();
        }

        // case ChunkLoaderState.FINISHTERRAIN:
        foreach (Chunk chunk in chunkToLoadTerrain) {
            chunk.finishChunkState();
        }

        // case ChunkLoaderState.SETUPBLOCK:
        foreach (Chunk chunk in chunkToLoadBlock) {
            chunk.setChunkState(ChunkState.BLOCKGENERATED);
            chunk.initChunkState();
        }

        // case ChunkLoaderState.LOADBLOCK:
        foreach (Chunk chunk in chunkToLoadBlock) {
            chunk.loadChunkState();
        }

        // case ChunkLoaderState.FINISHBLOCK:
        foreach (Chunk chunk in chunkToLoadBlock) {
            chunk.finishChunkState();
        }

        // case ChunkLoaderState.SETUPDRAW:
        foreach (Chunk chunk in chunkToDraw) {
            chunk.setChunkState(ChunkState.DRAWABLE);
            chunk.initChunkState();
        }

        // case ChunkLoaderState.LOADDRAW:
        foreach (Chunk chunk in chunkToDraw) {
            chunk.loadChunkState();
        }

        // case ChunkLoaderState.FINISHDRAW:
        foreach (Chunk chunk in chunkToDraw) {
            chunk.finishChunkState();
        }

        foreach (Chunk chunk in chunksList) {
            chunk.isRequiredByChunkLoader = false;
        }
    
        reset();
        return true;
    }

    public bool multiThreadLoading() {
        switch (chunkLoaderState) {
            case ChunkLoaderState.SETUPCHUNKINMEMORY:
                foreach (ChunkLoadingTask chunk in chunkInMemory) {
                    chunk.chunk.setChunkState(chunk.wantedChunkState);
                }

                chunkLoaderState = ChunkLoaderState.LOADCHUNKINMEMORY;
                task = Task.Factory.StartNew(() =>
                {
                    chunkStorage.LoadChunks(chunkInMemory.Select(chunk => chunk.chunk).ToList());
                }).ContinueWith((task) =>
                {
                    if (task.IsFaulted) throw task.Exception;
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
                    chunk.initChunkState();
                }
                chunkLoaderState = ChunkLoaderState.LOADTERRAIN;
                task = Parallel.ForEachAsync(chunkToLoadTerrain, async (chunk, token) => { chunk.loadChunkState(); })
                    .ContinueWith((task =>
                    {
                        if (task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHTERRAIN;
                    }));
                return false;
            case ChunkLoaderState.LOADTERRAIN:
                if (task.IsFaulted) throw task.Exception;
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
                    chunk.initChunkState();
                }

                chunkLoaderState = ChunkLoaderState.LOADBLOCK;
                task = Parallel.ForEachAsync(chunkToLoadBlock, async (chunk, token) => { chunk.loadChunkState(); })
                    .ContinueWith((task =>
                    {
                        if (task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHBLOCK;
                    }));
                return false;
            case ChunkLoaderState.LOADBLOCK:
                if (task.IsFaulted) throw task.Exception;
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
                    chunk.initChunkState();
                }

                chunkLoaderState = ChunkLoaderState.LOADDRAW;
                task = Parallel.ForEachAsync(chunkToDraw, async (chunk, token) => { chunk.loadChunkState(); })
                    .ContinueWith((task =>
                    {
                        if (task.IsFaulted) throw task.Exception;
                        chunkLoaderState = ChunkLoaderState.FINISHDRAW;
                    }));
                return false;
            case ChunkLoaderState.LOADDRAW:
                if (task.IsFaulted) throw task.Exception;
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