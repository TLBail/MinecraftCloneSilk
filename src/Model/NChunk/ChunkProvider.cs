using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.NChunk;

public interface ChunkProvider
{
    public Chunk getChunk(Vector3D<int> position);
}