using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.Storage;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;

namespace MinecraftCloneSilk.Model.NChunk;

public class Chunk : IDisposable
{
    public Vector3D<int> position;
    internal BlockData[,,] blocks;
    internal object blocksLock = new object();

    public const uint CHUNK_SIZE = 16;

    internal IChunkManager chunkManager;
    internal WorldGenerator worldGenerator;
    internal ChunkStorage chunkStorage;

    internal Line? debugRay;
    internal bool debugMode = false;

    internal Chunk[] chunksNeighbors;
    internal object chunksNeighborsLock = new object();

    public ChunkState chunkState { get; internal set; }
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;

    internal ChunkStrategy chunkStrategy;
    internal object chunkStrategyLock = new object();

    internal static Shader cubeShader;
    internal static BlockFactory blockFactory;

    private bool disposed = false;
    internal bool blockModified = false;
    
    public Chunk(Vector3D<int> position, IChunkManager chunkManager, WorldGenerator worldGenerator, ChunkStorage chunkStorage) {
        this.chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        this.position = position;
        if (blockFactory == null) blockFactory = BlockFactory.getInstance();
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    }

    public static void initStaticMembers(GL Gl, Shader chunkShader) {
        Chunk.cubeShader = chunkShader;
    }

    public void setMinimumWantedChunkState(ChunkState wantedChunkState) {
        if (wantedChunkState > this.chunkState) {
            setWantedChunkState(wantedChunkState);
        }
    }

    public void setWantedChunkState(ChunkState wantedChunkState) {
        lock (chunkStrategyLock) {
            switch (wantedChunkState) {
                case ChunkState.EMPTY:
                    if (chunkStrategy is not ChunkEmptyStrategy) {
                        chunkStrategy = new ChunkEmptyStrategy(this);
                        chunkStrategy.init();
                    }

                    return;
                case ChunkState.GENERATEDTERRAIN:
                    if (chunkStrategy is not ChunkTerrainGeneratedStrategy) {
                        chunkStrategy = new ChunkTerrainGeneratedStrategy(this);
                        chunkStrategy.init();
                    }

                    break;
                case ChunkState.BLOCKGENERATED:
                    if (chunkState == ChunkState.DRAWABLE) {
                        chunkStrategy.Dispose();
                        chunkStrategy = new ChunkBlockGeneratedStrategy(this);
                        chunkState = ChunkState.BLOCKGENERATED;
                        break;
                    }

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

    public void setBlock(int x, int y, int z, string name) {
        blockModified = true;
        chunkStrategy.setBlock(x, y, z, name);
    }

    public void updateChunkVertex() => chunkStrategy.updateChunkVertex();
    public void debug(bool? setDebug = null) => chunkStrategy.debug(setDebug);
    public void Update(double deltaTime) => chunkStrategy.update(deltaTime);
    
    public ReadOnlySpan<CubeVertex> getVertices() => chunkStrategy.getVertices();

    public void reset(Vector3D<int> position, IChunkManager chunkManager, WorldGenerator worldGenerator) {
        this.position = position;
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        debug(false);
        chunksNeighbors = new Chunk[6];
        chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        disposed = false;
        blockModified = false;
        for (int x = 0; x < CHUNK_SIZE; x++) {
            for (int y = 0; y < CHUNK_SIZE; y++) {
                for (int z = 0; z < CHUNK_SIZE; z++) {
                    blocks[x, y, z] = default(BlockData);
                }
            }
        }
    }

    public void save() => chunkStorage.SaveChunk(this);

    public override string ToString() {
        return $"Chunk {position.X} {position.Y} {position.Z} chunkState: {chunkState} \n";
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