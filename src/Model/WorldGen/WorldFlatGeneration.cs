using DotnetNoise;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class WorldFlatGeneration : WorldGenerator
{
    public const int GROUND_LEVEL = 0;
    
    private static BlockFactory blockFactory;

    public WorldFlatGeneration()
    {
        if(blockFactory == null) blockFactory = BlockFactory.getInstance();
    }
    
    
    public void generateTerrain(Vector3D<int> position, BlockData[,,] blocks)
    {

        for (int i = 0; i < Chunk.Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.Chunk.CHUNK_SIZE; j++) {
                double x = (double)j / ((double)Chunk.Chunk.CHUNK_SIZE);
                double z = (double)i / ((double)Chunk.Chunk.CHUNK_SIZE);

                int globalY = GROUND_LEVEL;
                x *= Chunk.Chunk.CHUNK_SIZE;
                z *= Chunk.Chunk.CHUNK_SIZE;

                if (globalY >= position.Y && globalY < position.Y + Chunk.Chunk.CHUNK_SIZE)
                {
                    int localY = (int)(globalY % Chunk.Chunk.CHUNK_SIZE);
                    if (localY < 0)
                        localY = (int)(Chunk.Chunk.CHUNK_SIZE + localY);
                    blocks[(int)x,localY,(int)z] = blockFactory.buildData("grass");
                    for (int g = localY - 1; g >= 0 && g >= localY - 4; g--)
                    {
                        blocks[(int)x,g,(int)z] = blockFactory.buildData("stone");
                    }
                    for (int g = localY - 5; g >= 0; g--)
                    {
                        blocks[(int)x,g,(int)z] = blockFactory.buildData("stone");
                    }
                }
                else if (globalY >= position.Y + Chunk.Chunk.CHUNK_SIZE)
                {
                    for (int y = 0; y < Chunk.Chunk.CHUNK_SIZE; y++)
                    {
                        blocks[j, y,i] = blockFactory.buildData("stone");
                    }
                }
            }
        }
        
    }

    
}