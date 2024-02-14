using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.WorldGen;

public class WorldNaturalGeneration : IWorldGenerator
{
    private FastNoiseLite noiseGenerator;
    public int seed;
    private static BlockFactory? blockFactory;
    private BlockData water;
    private BlockData sand;
    private BlockData grass;
    private BlockData stone;
    private BlockData diamond;
    
    private FastNoiseLite conantinalnessNoiseGenerator;
    private FastNoiseLite amplitudeNoiseGenerator;
    
    private FastNoiseLite treeNoiseGenerator;
    private FastNoiseLite treeProbabilityNoiseGenerator;
    
    private FastNoiseLite diamondNoiseGenerator;
    

    public WorldNaturalGeneration(int seed = 1234) {
        this.seed = seed;
        noiseGenerator = new FastNoiseLite(seed);
        noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noiseGenerator.SetFractalType(FastNoiseLite.FractalType.FBm);
        noiseGenerator.SetFractalOctaves(6);
        noiseGenerator.SetFractalLacunarity(2);
        noiseGenerator.SetFractalGain(0.9f);
        noiseGenerator.SetFrequency(0.0015f);
        
        conantinalnessNoiseGenerator = new FastNoiseLite(seed);
        conantinalnessNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        conantinalnessNoiseGenerator.SetFrequency(0.0002f);
        
        amplitudeNoiseGenerator = new FastNoiseLite(seed * 2);
        amplitudeNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        amplitudeNoiseGenerator.SetFrequency(0.0005f);
        
        treeNoiseGenerator = new FastNoiseLite(seed * 3);
        treeNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        treeNoiseGenerator.SetFrequency(0.4f);
        
        treeProbabilityNoiseGenerator = new FastNoiseLite(seed * 4);
        treeProbabilityNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        treeProbabilityNoiseGenerator.SetFrequency(0.0005f);

        diamondNoiseGenerator = new FastNoiseLite(seed * 4);
        diamondNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        diamondNoiseGenerator.SetFrequency(0.2f);
        
        
        if(blockFactory == null) blockFactory = BlockFactory.GetInstance();
        
        water = blockFactory.GetBlockData("water");
        sand = blockFactory.GetBlockData("sand");
        grass = blockFactory.GetBlockData("grass");
        stone = blockFactory.GetBlockData("stone");
        diamond = blockFactory.GetBlockData("diamond");
        
    }

    private bool IsAirBlock(int globalX, int globalY, int globalZ) {
        float cotenitalness = conantinalnessNoiseGenerator.GetNoise(globalX, globalY, globalZ);
        cotenitalness = Math.Abs(cotenitalness) * 500;
        cotenitalness -= 50;
        globalY -= (int)cotenitalness;
        
        float amplitudeNoise = amplitudeNoiseGenerator.GetNoise(globalX, globalY, globalZ);
        float amplitude = 100.0f; // define the amplitude of the noise  lower = more flat
        amplitude += amplitudeNoise * 200;
        
        float noise = noiseGenerator.GetNoise(globalX, globalY, globalZ);
        float threasholdAir = -0.2f;
        threasholdAir += globalY / amplitude;
        return noise <= threasholdAir;
    }
    
    public void GenerateTerrain(Vector3D<int> position, IChunkData lazyChunkData)
    {
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = Chunk.CHUNK_SIZE - 1; y >= 0; y--) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int globalY = y + position.Y;
                    if (globalY < -60) {
                        float noise = noiseGenerator.GetNoise(position.X + x, globalY, position.Z + z);
                        float threasholdAir = -0.2f;
                        
                        
                        if (noise >= threasholdAir) {
                            noise = diamondNoiseGenerator.GetNoise(position.X + x, globalY, position.Z + z);
                            float threasholdDiamond = -0.8f;
                            if (noise <= threasholdDiamond) {
                                lazyChunkData.SetBlock(diamond, x,y,z);
                            } else {
                                lazyChunkData.SetBlock(stone, x,y,z);
                            }
                        }
                        continue;
                        
                    }
                    
                    bool isAir = IsAirBlock(position.X + x,position.Y + y,position.Z + z);   
                    
                    if(isAir && globalY <= 0) {
                        lazyChunkData.SetBlock(water, x,y,z);
                        continue;
                    }
                    
                    if(isAir) continue;
                    if(y == Chunk.CHUNK_SIZE - 1) {
                        bool upperIsAir = IsAirBlock(position.X + x,position.Y + y + 1,position.Z + z);
                        if(upperIsAir) {
                            if (y + position.Y < 4) {
                                lazyChunkData.SetBlock(sand, x,y,z);
                            } else {
                                lazyChunkData.SetBlock(grass, x, y, z);
                            }
                        } else {
                            lazyChunkData.SetBlock(stone, x,y,z);
                        }
                    } else {
                        if (y < Chunk.CHUNK_SIZE - 1 && lazyChunkData.GetBlock(x, y + 1, z).id == 0) {
                            if (globalY < 4) {
                                lazyChunkData.SetBlock(sand, x,y,z);
                            } else {
                                lazyChunkData.SetBlock(grass, x, y, z);
                            }
                        } else {
                            lazyChunkData.SetBlock(stone, x,y,z);
                        }
                    }
                        
                }
            }
        }
    }

    public bool HaveTreeOnThisCoord(int positionX, int positionZ) {
        float noise = treeNoiseGenerator.GetNoise(positionX, positionZ);
        noise = Math.Abs(noise);
        
        return noise > (0.7f + treeProbabilityNoiseGenerator.GetNoise(positionX, positionZ) / 5);
        
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