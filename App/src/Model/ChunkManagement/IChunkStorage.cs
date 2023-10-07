using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.ChunkManagement;

public interface IChunkStorage
{
    ChunkState GetChunkStateInStorage(Vector3D<int> position);
    void SaveChunkAsync(Chunk chunk);
    void SaveChunk(Chunk chunk);
    void SaveChunks(List<Chunk> chunks);
    bool IsChunkExistInMemory(Vector3D<int> position);
    void LoadChunk(Chunk chunk);
    void LoadChunks(List<Chunk> chunks);
}