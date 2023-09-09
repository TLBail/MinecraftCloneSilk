using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.WorldGen;

public class WorldNaturalGeneration : IWorldGenerator
{
    private FastNoiseLite noiseGenerator;
    public static int seed = 1234;
    private static BlockFactory? blockFactory;
    private BlockData water;
    private BlockData sand;
    private BlockData grass;
    private BlockData stone;
    

    public WorldNaturalGeneration()
    {
        noiseGenerator = new FastNoiseLite(seed);
        noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noiseGenerator.SetFractalType(FastNoiseLite.FractalType.FBm);
        noiseGenerator.SetFractalOctaves(6);
        noiseGenerator.SetFractalLacunarity(2);
        noiseGenerator.SetFractalGain(0.9f);
        noiseGenerator.SetFrequency(0.0015f);
        
        
        if(blockFactory == null) blockFactory = BlockFactory.GetInstance();
        
        water = blockFactory.GetBlockData("water");
        sand = blockFactory.GetBlockData("sand");
        grass = blockFactory.GetBlockData("grass");
        stone = blockFactory.GetBlockData("stone");
        
    }

    private bool IsAirBlock(int globalX, int globalY, int globalZ) {
        float noise = noiseGenerator.GetNoise(globalX, globalY, globalZ);
        float threasholdAir = -0.2f;
        threasholdAir += globalY / 200.0f;
        return noise <= threasholdAir;
    }
    
    public void GenerateTerrain(Vector3D<int> position, BlockData[,,] blocks)
    {
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = Chunk.CHUNK_SIZE - 1; y >= 0; y--) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    
                    bool isAir = IsAirBlock(position.X + x,position.Y + y,position.Z + z);   
                    
                    if(isAir && position.Y + y <= 0) {
                        blocks[x,y,z] = water;
                        continue;
                    }
                    
                    if(isAir) continue;
                    if(y == Chunk.CHUNK_SIZE - 1) {
                        bool upperIsAir = IsAirBlock(position.X + x,position.Y + y + 1,position.Z + z);
                        if(upperIsAir) {
                            if (y + position.Y < 4) {
                                blocks[x,y,z] = sand;
                            } else {
                                blocks[x, y, z] = grass;
                            }
                        } else {
                            blocks[x,y,z] = stone;
                        }
                    } else {
                        if (y < Chunk.CHUNK_SIZE - 1 &&  blocks[x, y + 1, z].id == 0) {
                            if (y + position.Y < 4) {
                                blocks[x,y,z] = sand;
                            } else {
                                blocks[x, y, z] = grass;
                            }
                        } else {
                            blocks[x,y,z] = stone;
                        }
                    }
                        
                }
            }
        }

        
        
        
        /*
        for (int i = 0; i < Chunk.CHUNK_SIZE; i++) {
            for (int j = 0; j < Chunk.CHUNK_SIZE; j++) {
                double x = (double)j / ((double)Chunk.CHUNK_SIZE);
                double z = (double)i / ((double)Chunk.CHUNK_SIZE);
    
                x *= Chunk.CHUNK_SIZE;
                z *= Chunk.CHUNK_SIZE;
    
                if (globalY >= position.Y && globalY < position.Y + Chunk.CHUNK_SIZE)
                {
                    int localY = (int)(globalY % Chunk.CHUNK_SIZE);
                    if (localY < 0)
                        localY = (int)(Chunk.CHUNK_SIZE + localY);
                    blocks[(int)x,localY,(int)z] = blockFactory!.GetBlockData("grass");
                    for (int g = localY - 1; g >= 0 && g >= localY - 4; g--)
                    {
                        blocks[(int)x,g,(int)z] = blockFactory!.GetBlockData("stone");
                    }
                    for (int g = localY - 5; g >= 0; g--)
                    {
                        blocks[(int)x,g,(int)z] = blockFactory!.GetBlockData("stone");
                    }
                }
                else if (globalY >= position.Y + Chunk.CHUNK_SIZE)
                {
                    for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                    {
                        blocks[j, y,i] = blockFactory!.GetBlockData("stone");
                    }
                }
            }
        }
        */
        
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
        new (0.001f, 100),        //plateau global
        new (0.01f, 25),       //moyen variation
    };



}