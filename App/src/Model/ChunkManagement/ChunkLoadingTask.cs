using MinecraftCloneSilk.Model.NChunk;

namespace MinecraftCloneSilk.Model;

public record struct ChunkLoadingTask(Chunk chunk, ChunkState wantedChunkState);
