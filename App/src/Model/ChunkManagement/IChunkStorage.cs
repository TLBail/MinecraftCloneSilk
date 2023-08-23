using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public interface IChunkStorage
{
    ChunkState GetChunkStateInStorage(Vector3D<int> position);
    void SaveChunk(Chunk chunk);
    void SaveChunks(List<Chunk> chunks);
    bool isChunkExistInMemory(Vector3D<int> position);
    void LoadChunk(Chunk chunk);
    void LoadChunks(List<Chunk> chunks);
}