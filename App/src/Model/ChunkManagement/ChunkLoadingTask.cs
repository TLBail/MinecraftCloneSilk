using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public record struct ChunkLoadingTask(Chunk chunk, ChunkState wantedChunkState);
