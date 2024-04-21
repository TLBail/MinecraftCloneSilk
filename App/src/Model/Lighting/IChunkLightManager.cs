using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.Lighting;

public interface IChunkLightManager
{
    public SemaphoreSlim FullLightChunk(Chunk chunk);

    public SemaphoreSlim OnBlockSet(Chunk chunk, Vector3D<int> position, BlockData oldBlockData, BlockData newBlockData);
}