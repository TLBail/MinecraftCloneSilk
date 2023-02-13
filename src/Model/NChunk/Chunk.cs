using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.NChunk;

public class Chunk : IDisposable
{
    public Vector3D<int> position;
    internal readonly BlockData[,,] blocks;
    internal object blocksLock = new object();
    
    public static readonly uint CHUNK_SIZE = 16;

    internal IChunkManager chunkManager;
    internal WorldGenerator worldGenerator;

    internal List<DebugRay> debugRays = new List<DebugRay>();
    internal bool debugMode = false;

    internal static BlockFactory blockFactory;

    internal Chunk?[] chunksNeighbors = new Chunk[6];
    internal object chunksNeighborsLock = new object();
    
    public ChunkState chunkState { get; internal set; }
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;
    internal ChunkStrategy chunkStrategy;
    internal object chunkStrategyLock = new object();
    internal static Shader cubeShader;
    internal GL Gl;
    
    public Chunk(Vector3D<int> position, IChunkManager chunkManager, WorldGenerator worldGenerator) {
        this.chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.position = position;
        if(blockFactory == null) blockFactory = BlockFactory.getInstance();
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        Gl = Game.getInstance().getGL();
        if(Gl != null) initStaticMembers();
    }
    
    private void initStaticMembers()
    {
        if (cubeShader == null) {
            cubeShader = new Shader(Gl, "./Shader/3dPosOneTextUni/VertexShader.hlsl",
                "./Shader/3dPosOneTextUni/FragmentShader.hlsl");
            cubeShader.Use();
            cubeShader.SetUniform("texture1", 0);
        }
    }
    
    public void setMinimumWantedChunkState(ChunkState wantedChunkState) {
        if (wantedChunkState > this.chunkState) { 
                setWantedChunkState(wantedChunkState);
        }   
    }

    public void setWantedChunkState(ChunkState wantedChunkState) {
        lock (chunkStrategy) {
            switch (wantedChunkState) {
                case ChunkState.EMPTY:
                    if (chunkStrategy is not ChunkEmptyStrategy) {
                        chunkStrategy = new ChunkEmptyStrategy(this);
                        chunkStrategy.init();
                    }
                    return;
                case ChunkState.Generatedterrain:
                    if (chunkStrategy is not ChunkTerrainGeneratedStrategy) {
                        chunkStrategy = new ChunkTerrainGeneratedStrategy(this);
                        chunkStrategy.init();
                    }
                    break;
                case ChunkState.BLOCKGENERATED:
                    if (chunkStrategy is not ChunkBlockGeneratedStrategy) {
                        chunkStrategy = new ChunkBlockGeneratedStrategy(this);
                        chunkStrategy.init();
                    }
                    break;
                case ChunkState.DRAWABLE:
                    if (chunkStrategy is not ChunkDrawableStrategy) {
                        chunkStrategy = new ChunkDrawableStrategy(this);
                        chunkStrategy.init();
                    }
                    break;
            }   
        }
    }

    public BlockData getBlockData(Vector3D<int> localPosition) {
        return chunkStrategy.getBlockData(localPosition);
    }

    public ChunkState getMinimumChunkStateOfNeighbors() => chunkStrategy.minimumChunkStateOfNeighbors();
    public Block getBlock(Vector3D<int> blockPosition) => getBlock(blockPosition.X, blockPosition.Y, blockPosition.Z);
    public Block getBlock(int x, int y, int z) => chunkStrategy.getBlock(x, y, z);
    public void setBlock(int x, int y, int z, string name) => chunkStrategy.setBlock(x, y, z, name);
    public void updateChunkVertex() => chunkStrategy.updateChunkVertex();
    public void debug(bool? setDebug = null) => chunkStrategy.debug(setDebug);
    public void Update(double deltaTime) => chunkStrategy.update(deltaTime);
    public void Draw(GL Gl, double deltaTime) => chunkStrategy.draw(Gl, deltaTime);
    public void reset(Vector3D<int> position, IChunkManager chunkManager, WorldGenerator worldGenerator) {
        this.position = position;
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        debug(false);
        chunksNeighbors = new Chunk[6];
        chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);

        for (int x = 0; x < CHUNK_SIZE; x++) {
            for (int y = 0; y < CHUNK_SIZE; y++) {
                for (int z = 0; z < CHUNK_SIZE; z++) {
                    blocks[x, y, z] = default(BlockData);
                }
            }
        }
    }
    
    protected bool disposed = false;
    public void Dispose() {
        Dispose(true);  
        GC.SuppressFinalize(this);
    } 
    ~Chunk() => Dispose(false);

    protected virtual void Dispose(bool disposing) {
        if (!disposed) {
            if (disposing) {
                chunkStrategy.Dispose();
            }
            disposed = true;
        }
    }

}