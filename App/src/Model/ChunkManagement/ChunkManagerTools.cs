using System.Diagnostics;
using MinecraftCloneSilk.Logger;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public static class ChunkManagerTools
{
    
    
    
    public static Chunk GetBlockGeneratedChunk(IChunkManager chunkManager,ChunkLoader chunkLoader, Vector3D<int> position) {
        Chunk chunk = chunkManager.GetChunk(position);
        chunkLoader.AddChunkToQueue(chunk, ChunkState.BLOCKGENERATED);
        chunkLoader.LoadAllChunks();
        return chunk;
    }


}