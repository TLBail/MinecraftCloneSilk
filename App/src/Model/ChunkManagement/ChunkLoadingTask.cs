using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public class ChunkLoadingTask
{
    public Chunk chunk { get; set; } 
    public ChunkState wantedChunkState { get; set; }
    public List<ChunkWaitingTask> parents { get; set; } = new();

    public ChunkLoadingTask(Chunk chunk, ChunkState wantedChunkState, ChunkWaitingTask? parent = null) {
        this.chunk = chunk;
        this.wantedChunkState = wantedChunkState;
        if(parent is not null) parents.Add(parent);
    }
}
