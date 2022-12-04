using DotnetNoise;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public class WorldNaturalGeneration : WorldGenerator
{
    private FastNoise noiseGenerator;
    public static int seed = 1234;
    private static BlockFactory blockFactory;

    public WorldNaturalGeneration()
    {
        noiseGenerator = new FastNoise(seed);
        if(blockFactory == null) blockFactory = BlockFactory.getInstance();
    }
    
    
    public void generateTerrain(Vector3D<int> position, BlockData[,,] blocks)
    {

        for (int i = 0; i < Chunk.Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.Chunk.CHUNK_SIZE; j++) {
                double x = (double)j / ((double)Chunk.Chunk.CHUNK_SIZE);
                double z = (double)i / ((double)Chunk.Chunk.CHUNK_SIZE);

                int globalY =calculateGlobalY(position, x, z);
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

    public struct GenerationParameter
    {
        public float freq;
        public float amp;

        public GenerationParameter(float freq, float amp)
        {
            this.freq = freq;
            this.amp = amp;
        }
    }

    public static List<GenerationParameter> generationParameters = new List<GenerationParameter>()
    {
        new (10, 1000),        //plateau global
        new (0.1f, 25),       //moyen variation
        new (0.01f, 3)        //petit variation
    };
    
    private int calculateGlobalY(Vector3D<int> position, double x, double z)
    {
        float baseX = (float)((position.X / Chunk.Chunk.CHUNK_SIZE) + x);
        float baseZ = (float)((position.Z / Chunk.Chunk.CHUNK_SIZE) + z);

        float y = 0;
        foreach (GenerationParameter parameter in generationParameters) {
            y += noiseGenerator.GetPerlin( baseX / parameter.freq, baseZ / parameter.freq) * parameter.amp;
        }
        int i = (int)MathF.Floor(y);
        return i;

    }

    
}