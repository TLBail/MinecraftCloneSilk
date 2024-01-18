using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public interface IChunkStorage : IDisposable
{
    /**
     * Get the state of the chunk in the storage if the chunk is not in the storage return ChunkState.EMPTY
     * @param position position of the chunk
     * @return state of the chunk in the storage if the chunk is not in the storage return ChunkState.EMPTY
     */
    ChunkState GetChunkStateInStorage(Vector3D<int> position);
    
    /**
     * Save the chunk in the storage asynchronously
     * expect to save the chunk in the storage asynchronously and chunk.blockModified to false
     * @param chunk chunk to save
     */
    void SaveChunkAsync(Chunk chunk);
    
    /**
     * expect to have after the chunks has been save chunk.blockModified to false
     * @param chunks chunks to save
     */
    void SaveChunk(Chunk chunk);
    
    /**
     * expect to have after the chunks has been save chunk.blockModified to false
     * @param chunks chunks to save
     */
    void SaveChunks(List<Chunk> chunks);
    bool IsChunkExistInMemory(Vector3D<int> position);
    void LoadChunk(Chunk chunk);
    void LoadChunks(List<Chunk> chunks);
}