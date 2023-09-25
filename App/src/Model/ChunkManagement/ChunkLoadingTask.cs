using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public record ChunkLoadingTask(Chunk chunk, ChunkState wantedChunkState, ChunkWaitingTask? parent);
