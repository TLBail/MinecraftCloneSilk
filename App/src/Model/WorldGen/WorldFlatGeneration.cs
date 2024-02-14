using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.WorldGen;

public class WorldFlatGeneration : IWorldGenerator
{
    public const int GROUND_LEVEL = 0;
    
    private static BlockFactory blockFactory = null!;
    private static bool isInit = false;
    
    private static void Init()
    {
        if(!isInit) blockFactory = BlockFactory.GetInstance();
        isInit = true;
    }
    

    public WorldFlatGeneration()
    {
        if(!isInit) Init(); 
    }
    
    
    public void GenerateTerrain(Vector3D<int> position, IChunkData lazyChunkData)
    {

        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                double x = (double)j / ((double)Chunk.CHUNK_SIZE);
                double z = (double)i / ((double)Chunk.CHUNK_SIZE);

                int globalY = GROUND_LEVEL;
                x *= Chunk.CHUNK_SIZE;
                z *= Chunk.CHUNK_SIZE;

                if (globalY >= position.Y && globalY < position.Y + Chunk.CHUNK_SIZE)
                {
                    int localY = (int)(globalY % Chunk.CHUNK_SIZE);
                    if (localY < 0)
                        localY = (int)(Chunk.CHUNK_SIZE + localY);
                    lazyChunkData.SetBlock(  blockFactory.GetBlockData("grass"), (int)x,localY,(int)z);
                    for (int g = localY - 1; g >= 0 && g >= localY - 4; g--)
                    {
                        lazyChunkData.SetBlock(blockFactory.GetBlockData("stone"), (int)x,g,(int)z);
                    }
                    for (int g = localY - 5; g >= 0; g--)
                    {
                        lazyChunkData.SetBlock(blockFactory.GetBlockData("stone"), (int)x,g,(int)z);
                    }
                }
                else if (globalY >= position.Y + Chunk.CHUNK_SIZE)
                {
                    for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    {
                        lazyChunkData.SetBlock(blockFactory.GetBlockData("stone"), j, y,i);
                    }
                }
            }
        }
        
    }

    public bool HaveTreeOnThisCoord(int positionX, int positionZ) {
        return false;
    }
}