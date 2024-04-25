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
    private BlockData dirt;
    
    private FastNoiseLite conantinalnessNoiseGenerator;
    private FastNoiseLite amplitudeNoiseGenerator;
    
    
    private FastNoiseLite treeNoiseGenerator;
    
    /**
     * humidityNoiseGenerator
     * define :
     * - if the biome have a lot of tree or not
     * - if the biome have sand as ground or not
     */
    private FastNoiseLite humidityNoiseGenerator;
    
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
        
        humidityNoiseGenerator = new FastNoiseLite(seed * 4);
        humidityNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        humidityNoiseGenerator.SetFrequency(0.0005f);

        diamondNoiseGenerator = new FastNoiseLite(seed * 4);
        diamondNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        diamondNoiseGenerator.SetFrequency(0.2f);
        
        
        if(blockFactory == null) blockFactory = BlockFactory.GetInstance();
        
        water = blockFactory.GetBlockData("water");
        sand = blockFactory.GetBlockData("sand");
        grass = blockFactory.GetBlockData("grass");
        stone = blockFactory.GetBlockData("stone");
        diamond = blockFactory.GetBlockData("diamond");
        dirt = blockFactory.GetBlockData("dirt");
        
    }

    private float GetThresholdAir(int globalX, int globalY, int globalZ) {
        float cotenitalness = conantinalnessNoiseGenerator.GetNoise(globalX, globalY, globalZ);
        cotenitalness = Math.Abs(cotenitalness) * 500;
        cotenitalness -= 50;
        globalY -= (int)cotenitalness;
        
        float amplitudeNoise = amplitudeNoiseGenerator.GetNoise(globalX, globalY, globalZ);
        float amplitude = 100.0f; // define the amplitude of the noise  lower = more flat
        amplitude += amplitudeNoise * 200;
        
        float threasholdAir = -0.2f;
        threasholdAir += globalY / amplitude;
        return threasholdAir;
    }
    
    public void GenerateTerrain(Vector3D<int> position, IChunkData chunkData) {
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = Chunk.CHUNK_SIZE - 1; y >= 0; y--) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int globalY = y + position.Y;
                    float noise = noiseGenerator.GetNoise(position.X + x, globalY, position.Z + z);
                    float threasholdAir = GetThresholdAir(position.X + x, globalY, position.Z + z);
                    
                    if (globalY < -60) {
                        threasholdAir = -0.2f;
                        
                        
                        if (noise >= threasholdAir) {
                            noise = diamondNoiseGenerator.GetNoise(position.X + x, globalY, position.Z + z);
                            float threasholdDiamond = -0.8f;
                            if (noise <= threasholdDiamond) {
                                chunkData.SetBlock(diamond, x,y,z);
                            } else {
                                chunkData.SetBlock(stone, x,y,z);
                            }
                        }
                        continue;
                        
                    }
                    
                    bool isAir = noise <= threasholdAir;   
                    
                    if(isAir && globalY <= 0) {
                        chunkData.SetBlock(water, x,y,z);
                        continue;
                    }
                    
                    if(isAir) continue;
                    
                    bool upperBlockIsAir = noiseGenerator.GetNoise(position.X + x, globalY + 1, position.Z + z) <= 
                                           GetThresholdAir(position.X + x, globalY + 1, position.Z + z);
                    
                    
                    if(upperBlockIsAir) { // have air upper
                        if(position.Y + y < -5) {
                            chunkData.SetBlock(stone, x,y,z);
                        } else if (position.Y + y < 5) {
                            chunkData.SetBlock(sand, x,y,z);
                        } else { 
                            if(IsDesert(position.X + x, position.Z +z)) {
                                chunkData.SetBlock(sand, x,y,z);
                            } else {
                                chunkData.SetBlock(grass, x,y,z);
                            }
                        }
                    } else {
                        if (Math.Abs(threasholdAir - noise) < 0.02) { // near air
                            if(position.Y + y < -5) {
                                chunkData.SetBlock(stone, x,y,z);
                            } else if (position.Y + y < 5) {
                                chunkData.SetBlock(sand, x,y,z);
                            } else {
                                if(IsDesert(position.X + x, position.Z +z)) {
                                    chunkData.SetBlock(sand, x,y,z);
                                } else {
                                    chunkData.SetBlock(dirt, x,y,z);
                                }
                            }
                        } else { // far from air
                            chunkData.SetBlock(stone, x, y, z);
                        }
                    }
                        
                }
            }
        }
    }

    public bool HaveTreeOnThisCoord(int positionX,int positionY, int positionZ) {
        if (positionY <= 5) return false;
        float noise = treeNoiseGenerator.GetNoise(positionX, positionZ);
        noise = Math.Abs(noise);

        float humidity = humidityNoiseGenerator.GetNoise(positionX, positionZ);
        float threshold = 0.7f;
        if (IsDesert(humidity)) {
            threshold += 0.1f;
            threshold -= humidity / 5;
        } else {
            threshold -= humidity / 5;
        }
        
        return noise > threshold;
        
    }
    
    public bool IsDesert(int positionX, int positionZ) {
        float humidity = humidityNoiseGenerator.GetNoise(positionX, positionZ);
        return IsDesert(humidity);
    }
    public bool IsDesert(float humidity) {
        return humidity < 0f;
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