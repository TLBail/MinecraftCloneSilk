using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public interface IChunkManager
{
    public Chunk GetChunk(Vector3D<int> position);
    public void AddChunkToUpdate(Chunk chunk);
    public void RemoveChunkToUpdate(Chunk chunk);
    bool ContainChunk(Vector3D<int> position);
}