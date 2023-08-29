using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.RegionDrawing;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
// ReSharper disable All

namespace MinecraftCloneSilk.Model.NChunk;

public class Chunk : IDisposable
{
    public Vector3D<int> position;
    internal BlockData[,,] blocks;

    public const int CHUNK_SIZE = 16;

    internal IChunkManager chunkManager;
    internal IWorldGenerator worldGenerator;

    internal Line? debugRay;
    internal bool debugMode = false;

    public Chunk[]? chunksNeighbors;

    public ChunkState chunkState { get; internal set; }
    public ChunkState wantedChunkState;
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;

    internal ChunkStrategy chunkStrategy;

    internal static BlockFactory? blockFactory;

    private int requiredByChunkLoader = 0;
    private int requiredByChunkUnloader = 0;
    private bool disposed = false;
    internal bool blockModified = false;
    


    public Chunk(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator) {
        this.chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.wantedChunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.position = position;
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    }

    public static unsafe void InitStaticMembers(Shader? newCubeShader, BlockFactory? newBlockFactory, GL? gl = null) {
        RegionBuffer.cubeShader = newCubeShader;
        blockFactory = newBlockFactory;
        if(gl is not null)RegionBuffer.InitComputeShader(gl, newBlockFactory);
    }

    public void SetChunkState(ChunkState newChunkState) {
        switch (newChunkState) {
            case ChunkState.EMPTY:
                chunkStrategy = new ChunkEmptyStrategy(this);
                return;
            case ChunkState.GENERATEDTERRAIN:
                chunkStrategy = new ChunkTerrainGeneratedStrategy(this);
                break;
            case ChunkState.BLOCKGENERATED:
                if (chunkState == ChunkState.DRAWABLE) {
                    chunkStrategy.Dispose();
                }
                chunkStrategy = new ChunkBlockGeneratedStrategy(this);
                break;
            case ChunkState.DRAWABLE:
                chunkStrategy = new ChunkDrawableStrategy(this);
                break;
        }
        this.chunkState = newChunkState;
    }
    
    public void InitChunkState() => chunkStrategy.Init();
    public void LoadChunkState() => chunkStrategy.Load();
    public void FinishChunkState() => chunkStrategy.Finish();
    public BlockData GetBlockData(Vector3D<int> localPosition) => chunkStrategy.GetBlockData(localPosition);
    public ChunkState GetMinimumChunkStateOfNeighbors() =>  chunkStrategy.MinimumChunkStateOfNeighbors();
    public Block GetBlock(Vector3D<int> blockPosition) => GetBlock(blockPosition.X, blockPosition.Y, blockPosition.Z);
    public Block GetBlock(int x, int y, int z) => chunkStrategy.GetBlock(x, y, z);

    public void SetBlock(int x, int y, int z, string name) {
        blockModified = true;
        chunkStrategy.SetBlock(x, y, z, name);
    }

    public void UpdateChunkVertex() => chunkStrategy.UpdateChunkVertex();
    public void Debug(bool? setDebug = null) => chunkStrategy.Debug(setDebug);
    public void Update(double deltaTime) => chunkStrategy.Update(deltaTime);

    public bool isRequiredByChunkLoader() => requiredByChunkLoader > 0;
    public void addRequiredByChunkLoader() => Interlocked.Increment(ref requiredByChunkLoader);
    public void removeRequiredByChunkLoader() => Interlocked.Decrement(ref requiredByChunkLoader);
    
    public bool isRequiredByChunkUnloader() => requiredByChunkUnloader > 0;
    public void addRequiredByChunkUnloader() => Interlocked.Increment(ref requiredByChunkUnloader);
    public void removeRequiredByChunkUnloader() => Interlocked.Decrement(ref requiredByChunkUnloader);
    
    public void Reset(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator) {
        if(isRequiredByChunkLoader()) {
            throw new Exception("Chunk is still required by chunk loader");
        }
        this.position = position;
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        Debug(false);
        chunksNeighbors = new Chunk[6];
        chunkState = DEFAULTSTARTINGCHUNKSTATE;
        wantedChunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        disposed = false;
        blockModified = false;
        Array.Clear(blocks);
    }
    

    public override string ToString() {
        return $"Chunk {position.X} {position.Y} {position.Z} chunkState: {chunkState} \n";
    }

    public override int GetHashCode() {
        return position.GetHashCode();
    }

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