using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public interface IChunkManager
{
    public Chunk getChunk(Vector3D<int> position);
    public void addChunkToDraw(Chunk chunk);
    public void addChunkToUpdate(Chunk chunk);
}