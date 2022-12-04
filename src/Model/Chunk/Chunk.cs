using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.Chunk;

public class Chunk : IDisposable
{
    public readonly Vector3D<int> position;
    internal readonly BlockData[,,] blocks;

    public static readonly uint CHUNK_SIZE = 16;

    internal ChunkProvider chunkProvider;
    internal WorldGenerator worldGenerator;

    public bool displayable { get; private set; }

    private List<DebugRay> debugRays = new List<DebugRay>();
    private bool debugMode = false;

    internal static BlockFactory blockFactory;

    internal Chunk?[] chunksNeighbors = new Chunk[6];
    
    public ChunkState chunkState { get; internal set; }
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;
    internal ChunkStrategy chunkStrategy;
    internal static Shader cubeShader;
    internal GL Gl;
    
    public Chunk(Vector3D<int> position, ChunkProvider chunkProvider, WorldGenerator worldGenerator) {
        this.chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        this.chunkProvider = chunkProvider;
        this.worldGenerator = worldGenerator;
        this.position = position;
        this.displayable = false;
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
    
    public async Task setMinimumWantedChunkState(ChunkState wantedChunkState) {
        if (wantedChunkState > this.chunkState) {
            await setWantedChunkState(wantedChunkState);
        } 
    }

    public async Task setWantedChunkState(ChunkState wantedChunkState) {
        switch (wantedChunkState) {
            case ChunkState.EMPTY:
                if (chunkStrategy is not ChunkEmptyStrategy) {
                    chunkStrategy = new ChunkEmptyStrategy(this);
                    await chunkStrategy.init();
                }
                return;
            case ChunkState.Generatedterrain:
                if (chunkStrategy is not ChunkTerrainGeneratedStrategy) {
                    chunkStrategy = new ChunkTerrainGeneratedStrategy(this);
                    await chunkStrategy.init();
                }
                break;
            case ChunkState.BLOCKGENERATED:
                if (chunkStrategy is not ChunkBlockGeneratedStrategy) {
                    chunkStrategy = new ChunkBlockGeneratedStrategy(this);
                    await chunkStrategy.init();
                }
                break;
            case ChunkState.DRAWABLE:
                if (chunkStrategy is not ChunkDrawableStrategy) {
                    chunkStrategy = new ChunkDrawableStrategy(this);
                    await chunkStrategy.init();
                }
                break;
        }
    }
    public BlockData getBlockData(Vector3D<int> localPosition) => chunkStrategy.getBlockData(localPosition);
    public async Task<Block> getBlock(Vector3D<int> blockPosition) =>await  getBlock(blockPosition.X, blockPosition.Y, blockPosition.Z);
    public async Task<Block> getBlock(int x, int y, int z) => await chunkStrategy.getBlock(x, y, z);
    public void setBlock(int x, int y, int z, string name) => chunkStrategy.setBlock(x, y, z, name);
    public void updateChunkVertex() => chunkStrategy.updateChunkVertex();
    
    public void debug(bool? setDebug = null)
    {
        debugMode = setDebug ?? !debugMode;

        
        if (!debugMode) {
            foreach (var debugRay in debugRays) {
                debugRay.remove();
            }
            debugRays.Clear();
        }
        else {
            //base
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y -0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + Chunk.CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f , position.Z +  Chunk.CHUNK_SIZE - 0.5f)));

            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y  - 0.5f, position.Z +  Chunk.CHUNK_SIZE- 0.5f)));
            
            //top base

            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + Chunk.CHUNK_SIZE - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X - 0.5f , position.Y + CHUNK_SIZE - 0.5f , position.Z +  Chunk.CHUNK_SIZE - 0.5f)));

            
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y +CHUNK_SIZE - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            
            //between
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X  - 0.5f, position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z- 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z + CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z+ CHUNK_SIZE - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X - 0.5f , position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z+ CHUNK_SIZE - 0.5f)));
        }

    }

  

    public void Update(double deltaTime) => chunkStrategy.update(deltaTime);


    public void Draw(GL Gl, double deltaTime) => chunkStrategy.draw(Gl, deltaTime);


    internal Vector3D<int> getChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)(MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Y/ Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE)
        );

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