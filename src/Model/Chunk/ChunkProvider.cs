using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Chunk;

public interface ChunkProvider
{
    public Chunk getChunk(Vector3D<int> position);
}