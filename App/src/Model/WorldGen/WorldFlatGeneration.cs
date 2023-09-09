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
    
    
    public void GenerateTerrain(Vector3D<int> position, BlockData[,,] blocks)
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
                    blocks[(int)x,localY,(int)z] = blockFactory.GetBlockData("grass");
                    for (int g = localY - 1; g >= 0 && g >= localY - 4; g--)
                    {
                        blocks[(int)x,g,(int)z] = blockFactory.GetBlockData("stone");
                    }
                    for (int g = localY - 5; g >= 0; g--)
                    {
                        blocks[(int)x,g,(int)z] = blockFactory.GetBlockData("stone");
                    }
                }
                else if (globalY >= position.Y + Chunk.CHUNK_SIZE)
                {
                    for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    {
                        blocks[j, y,i] = blockFactory.GetBlockData("stone");
                    }
                }
            }
        }
        
    }

    
}