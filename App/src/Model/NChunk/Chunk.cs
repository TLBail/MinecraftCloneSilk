using System.Numerics;
using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.RegionDrawing;
using MinecraftCloneSilk.Model.Storage;
using MinecraftCloneSilk.Model.WorldGen;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;

namespace MinecraftCloneSilk.Model.NChunk;

public class Chunk
{
    public Vector3D<int> position;
    internal BlockData[,,] blocks;

    public const int CHUNK_SIZE = 16;

    internal IChunkManager chunkManager;
    internal IWorldGenerator worldGenerator;
    internal IChunkStorage chunkStorage;

    internal Line? debugRay;
    internal bool debugMode = false;

    public Chunk[]? chunksNeighbors;

    public ChunkState chunkState { get; internal set; }
    public ChunkState chunkStateInStorage = ChunkState.EMPTY;
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;

    internal ChunkStrategy chunkStrategy;

    internal static BlockFactory? blockFactory;

    private int requiredByChunkLoader = 0;
    private int requiredByChunkUnloader = 0;
    internal bool blockModified = false;
    private AABBCube aabbCube;


    public Chunk(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        this.chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        this.position = position;
        this.aabbCube = new AABBCube(new Vector3(position.X, position.Y, position.Z), new Vector3(position.X + Chunk.CHUNK_SIZE, position.Y + Chunk.CHUNK_SIZE, position.Z + Chunk.CHUNK_SIZE));
        this.chunkStateInStorage = ChunkState.UNKNOW;
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    }

    public static unsafe void InitStaticMembers(Shader? newCubeShader, BlockFactory? newBlockFactory, GL? gl = null) {
        RegionBuffer.cubeShader = newCubeShader;
        blockFactory = newBlockFactory;
        if(gl is not null)RegionBuffer.InitComputeShader(gl, newBlockFactory);
    }


    public ChunkWaitingTask? TryToSetChunkState(ChunkLoader chunkLoader, ChunkLoadingTask chunkLoadingTask) {
        if (chunkState >= chunkLoadingTask.wantedChunkState) {
            return null;
        }

        if (chunkStateInStorage == ChunkState.UNKNOW) {
            if(chunkLoadingTask.wantedChunkState == ChunkState.STORAGELOADED) {
                return null;
            }
           System.Diagnostics.Debug.Assert(chunkState == ChunkState.EMPTY, "chunkState == ChunkState.EMPTY");
           ChunkWaitingTask chunkWaitingTaskStorage = new ChunkWaitingTask(chunkLoadingTask, 1);
           bool added = chunkLoader.NewJob(new ChunkLoadingTask(this, ChunkState.STORAGELOADED, chunkWaitingTaskStorage));
           if(!added) throw new Exception("chunkLoadingTaskStorage not added");
           return chunkWaitingTaskStorage;
        }
        
        switch (chunkLoadingTask.wantedChunkState) {
            case ChunkState.EMPTY:
                return null;
            case ChunkState.GENERATEDTERRAIN:
                return null;
            case ChunkState.BLOCKGENERATED:
                ChunkWaitingTask chunkWaitingTaskBlockGenerated = new ChunkWaitingTask(chunkLoadingTask, FaceExtendedConst.FACES.Count);
                foreach (FaceExtended face in FaceExtendedConst.FACES) {
                    Vector3D<int> positionChunkToLoad = position + (FaceExtendedOffset.GetOffsetOfFace(face) * CHUNK_SIZE);
                    bool added = chunkLoader.NewJob(new ChunkLoadingTask(chunkManager.GetChunk(positionChunkToLoad), ChunkState.GENERATEDTERRAIN, chunkWaitingTaskBlockGenerated));
                    if (!added) chunkWaitingTaskBlockGenerated.counter--;
                }

                if (chunkState != ChunkState.GENERATEDTERRAIN) {
                    bool added = chunkLoader.NewJob(new ChunkLoadingTask(this, ChunkState.GENERATEDTERRAIN, chunkWaitingTaskBlockGenerated));
                    if(added) chunkWaitingTaskBlockGenerated.counter++;
                }
                return chunkWaitingTaskBlockGenerated.counter > 0 ? chunkWaitingTaskBlockGenerated : null;
            case ChunkState.LIGHTING:
                ChunkWaitingTask chunkWaitingTaskLighting = new ChunkWaitingTask(chunkLoadingTask, FaceExtendedConst.FACES.Count);
                foreach (FaceExtended face in FaceExtendedConst.FACES) {
                    Vector3D<int> positionChunkToLoad = position + (FaceExtendedOffset.GetOffsetOfFace(face) * CHUNK_SIZE);
                    bool added = chunkLoader.NewJob(new ChunkLoadingTask(chunkManager.GetChunk(positionChunkToLoad), ChunkState.BLOCKGENERATED, chunkWaitingTaskLighting));
                    if(!added) chunkWaitingTaskLighting.counter--;
                }
                if (chunkState != ChunkState.BLOCKGENERATED) {
                    bool added = chunkLoader.NewJob(new ChunkLoadingTask(this, ChunkState.BLOCKGENERATED, chunkWaitingTaskLighting));
                    if(added) chunkWaitingTaskLighting.counter++;
                }
                return chunkWaitingTaskLighting.counter > 0 ? chunkWaitingTaskLighting : null;
            case ChunkState.DRAWABLE:
                ChunkWaitingTask chunkWaitingTaskDrawable = new ChunkWaitingTask(chunkLoadingTask, FaceExtendedConst.FACES.Count);
                foreach (FaceExtended face in FaceExtendedConst.FACES) {
                    Vector3D<int> positionChunkToLoad = position + (FaceExtendedOffset.GetOffsetOfFace(face) * CHUNK_SIZE);
                    bool added = chunkLoader.NewJob(new ChunkLoadingTask(chunkManager.GetChunk(positionChunkToLoad), ChunkState.BLOCKGENERATED, chunkWaitingTaskDrawable));
                    if(!added) chunkWaitingTaskDrawable.counter--;
                }
                if (chunkState != ChunkState.LIGHTING) {
                    bool added = chunkLoader.NewJob(new ChunkLoadingTask(this, ChunkState.LIGHTING, chunkWaitingTaskDrawable));
                    if(added) chunkWaitingTaskDrawable.counter++;
                }
                return chunkWaitingTaskDrawable.counter > 0 ? chunkWaitingTaskDrawable : null;
            default:
                throw new ArgumentException("ChunkState not found");
        }
    }

    public void SetChunkState(ChunkState newChunkState) {
        switch (newChunkState) {
            case ChunkState.EMPTY:
                chunkStrategy = new ChunkEmptyStrategy(this);
                return;
            case ChunkState.STORAGELOADED:
                chunkStrategy = new ChunkStorageStrategy(this);
                return;
            case ChunkState.GENERATEDTERRAIN:
                chunkStrategy = new ChunkTerrainGeneratedStrategy(this);
                break;
            case ChunkState.BLOCKGENERATED:
                if (chunkStrategy is ChunkDrawableStrategy chkdw) {
                    chkdw.Hide();
                }
                chunkStrategy = new ChunkBlockGeneratedStrategy(this);
                break;
            case ChunkState.LIGHTING:
                chunkStrategy = new ChunkLightingStrategy(this);
                break;
            case ChunkState.DRAWABLE:
                chunkStrategy = new ChunkDrawableStrategy(this);
                break;
        }
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
    
    public AABBCube GetAABBCube() => aabbCube;
    
    public void Reset(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        if(isRequiredByChunkLoader()) {
            throw new Exception("Chunk is still required by chunk loader");
        }
        this.position = position;
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.aabbCube = new AABBCube(new Vector3(position.X, position.Y, position.Z), new Vector3(position.X + Chunk.CHUNK_SIZE, position.Y + Chunk.CHUNK_SIZE, position.Z + Chunk.CHUNK_SIZE));
        this.chunkStateInStorage = ChunkState.UNKNOW;
        this.chunkStorage = chunkStorage;
        Debug(false);
        chunksNeighbors = new Chunk[6];
        chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        blockModified = false;
        Array.Clear(blocks);
    }
    

    public override string ToString() {
        return $"Chunk {position.X} {position.Y} {position.Z} chunkState: {chunkState} \n";
    }

    public override int GetHashCode() {
        return position.GetHashCode();
    }

}