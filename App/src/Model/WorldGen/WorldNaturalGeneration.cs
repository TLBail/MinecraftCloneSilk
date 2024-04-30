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

    private FastNoiseLite caveNoiseGenerator;
    private FastNoiseLite caveNoiseGenerator2;
    
    /**
     * humidityNoiseGenerator
     * define :
     * - if the biome have a lot of tree or not
     * - if the biome have sand as ground or not
     */
    private FastNoiseLite humidityNoiseGenerator;
    
    private FastNoiseLite diamondNoiseGenerator;
    

    public WorldNaturalGeneration(int seed = 1534) {
        this.seed = seed;
        noiseGenerator = new FastNoiseLite(seed);
        noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noiseGenerator.SetFractalType(FastNoiseLite.FractalType.FBm);
        noiseGenerator.SetFractalOctaves(6);
        noiseGenerator.SetFractalLacunarity(2);
        noiseGenerator.SetFractalGain(0.9f);
        noiseGenerator.SetFrequency(0.0015f);
        
        caveNoiseGenerator = new FastNoiseLite(seed + 1);
        caveNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        caveNoiseGenerator.SetFrequency(0.015f);
        caveNoiseGenerator2 = new FastNoiseLite(seed + 324135);
        caveNoiseGenerator2.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        caveNoiseGenerator2.SetFrequency(0.025f);


        
        conantinalnessNoiseGenerator = new FastNoiseLite(seed - 1);
        conantinalnessNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        conantinalnessNoiseGenerator.SetFrequency(0.0002f);
        
        amplitudeNoiseGenerator = new FastNoiseLite(seed * 2);
        amplitudeNoiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        amplitudeNoiseGenerator.SetFrequency(0.0002f);
        
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

    private float GetThresholdAir(int globalX, int globalY, int globalZ, float amplitudeNoise) {
        float cotenitalness = conantinalnessNoiseGenerator.GetNoise(globalX, globalZ - 200);
        cotenitalness = Math.Abs(cotenitalness) * 1000;
        cotenitalness -= 80;
        globalY -= (int)cotenitalness;
        
        // define the amplitude of the noise  lower = more flat
        
        float amplitude = (Math.Abs(amplitudeNoise) * 800) + 50; //amplitube must be >0 
        
        
        float threasholdAir = -0.2f;
        threasholdAir += globalY / amplitude;
        return threasholdAir;
    }
    
    public void GenerateTerrain(Vector3D<int> position, BlockData[,,] blocks) {
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = Chunk.CHUNK_SIZE - 1; y >= 0; y--) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int globalY = y + position.Y;
                    int globalX = x + position.X;
                    int globalZ = z + position.Z;
                    float noise = noiseGenerator.GetNoise(globalX, globalY, globalZ);
                    float amplitudeNoise = amplitudeNoiseGenerator.GetNoise(globalX, globalY, globalZ);
                    float threasholdAir = GetThresholdAir(globalX, globalY, globalZ, amplitudeNoise);
                    
                    
                    bool isAir = noise <= threasholdAir;   
                    
                    if(isAir && globalY <= 0) {
                        blocks[ x,y,z] = water;
                        continue;
                    }
                    if(isAir) continue;
                    
                    
                    float caveNoise = caveNoiseGenerator.GetNoise(globalX, globalY, globalZ);
                    float caveNoise2 = caveNoiseGenerator2.GetNoise(globalX + 10000, globalY + 10000, globalZ + 10000);
                    
                    //make a hole in the surface if near a cave 
                    if(((noise - threasholdAir < 0.11) && caveNoise >= 0.3 && caveNoise2 >= 0.8f)) continue; 
                    
                               
                    if (noise - threasholdAir > 0.1) { // far from the surface => make a cave
                        bool caveAir = (caveNoise >= 0.6f) || (caveNoise >= 0.3 && caveNoise2 >= 0.2f) ;
                        if (!caveAir) {
                            noise = diamondNoiseGenerator.GetNoise(globalX, globalY, globalZ);
                            float threasholdDiamond = -0.8f;
                            if (noise <= threasholdDiamond) {
                                blocks[ x,y,z] = diamond;
                            } else {
                                blocks[ x,y,z] = stone;
                            }
                        }
                        continue;
                        
                    }
              
                    if (Math.Abs(threasholdAir - noise) < 0.02) { // near surface  =>add terrain decoration
                        if(position.Y + y < -5) {
                            blocks[ x,y,z] = stone;
                        } else if (position.Y + y < 5) {
                            blocks[ x,y,z] = sand;
                        } else {
                            if(IsDesert(globalX, globalY, globalZ)) {
                                blocks[ x,y,z] = sand;
                            } else {
                                bool upperBlockIsAir = noiseGenerator.GetNoise(globalX, globalY + 1, globalZ) <= 
                                                       GetThresholdAir(globalX, globalY + 1, globalZ, 
                                                           amplitudeNoiseGenerator.GetNoise(globalX, globalY+ 1, globalZ));
                                if (upperBlockIsAir) {
                                    blocks[ x,y,z] = grass;
                                } else {
                                    blocks[ x,y,z] = dirt;
                                }

                            }
                        }
                    } else { // far from air
                        blocks[ x, y, z] = stone;
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
        if (IsDesert(humidity, amplitudeNoiseGenerator.GetNoise(positionX,positionY, positionZ))) {
            threshold += 0.1f;
            threshold -= humidity / 5;
        } else {
            threshold -= humidity / 5;
        }
        
        return noise > threshold;
        
    }
    
    public bool IsDesert(int positionX,int positionY, int positionZ) {
        float humidity = humidityNoiseGenerator.GetNoise(positionX, positionZ);
        float amplitude = amplitudeNoiseGenerator.GetNoise(positionX,positionY, positionZ);
        return IsDesert(humidity, amplitude);
    }
    
    public bool IsDesert(float humidity, float amplitude) {
        return humidity < 0f && Math.Abs(amplitude) < 0.5f;
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