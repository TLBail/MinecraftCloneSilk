using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public interface WorldGenerator
{
    public void generateTerrain(Vector3D<int> chunkPosition, BlockData[,,] blocks);
}