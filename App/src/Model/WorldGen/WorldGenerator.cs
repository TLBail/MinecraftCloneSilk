using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.WorldGen;

public interface IWorldGenerator
{
    public void GenerateTerrain(Vector3D<int> chunkPosition, IChunkData chunkData);
    bool HaveTreeOnThisCoord(int positionX,int positionY, int positionZ);
    
    bool IsDesert(int positionX,int positionY, int positionZ);
}