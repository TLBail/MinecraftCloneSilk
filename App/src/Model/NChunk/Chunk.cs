using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.ChunkManagement;
using MinecraftCloneSilk.Model.Lighting;
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
    internal IChunkLightManager chunkLightManager;

    internal Line? debugRay;
    internal bool debugMode = false;

    public Chunk[]? chunksNeighbors;
    
    internal ChunkFace? chunkFace;

    public ChunkState chunkState { get; internal set; }
    public ChunkState chunkStateInStorage { get; internal set; } = ChunkState.EMPTY;
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;

    internal ChunkStrategy chunkStrategy;

    internal static BlockFactory? blockFactory;

    private int requiredByChunkSaver = 0;
    internal bool blockModified = false;
    private AABBCube aabbCube;

    public List<ChunkLoadingTask> chunkTaskOfChunk = new();
    
    public event Action OnChunkFinishLoading;
    public event Action OnChunkFinishSaving;

    public Chunk(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator, IChunkStorage chunkStorage, IChunkLightManager chunkLightManager) {
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        this.chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.chunkStorage = chunkStorage;
        this.chunkLightManager = chunkLightManager;
        this.position = position;
        this.aabbCube = new AABBCube(new Vector3(position.X, position.Y, position.Z), new Vector3(position.X + Chunk.CHUNK_SIZE, position.Y + Chunk.CHUNK_SIZE, position.Z + Chunk.CHUNK_SIZE));
        this.chunkStateInStorage = ChunkState.UNKNOW;
    }

    public static unsafe void InitStaticMembers(Shader? newCubeShader, BlockFactory? newBlockFactory, GL? gl = null) {
        RegionBuffer.cubeShader = newCubeShader;
        blockFactory = newBlockFactory;
        if(gl is not null)RegionBuffer.InitComputeShader(gl, newBlockFactory);
    }


    /**
     *  test if the chunk is capable of loading else it will add the task to the chunkLoader
     *  @return true if the chunk is capable of loading directly
     *  @return false if the chunk is not capable of loading directly
     */
    public bool TryToSetChunkState(ChunkLoader chunkLoader, ChunkLoadingTask chunkLoadingTask) {
        System.Diagnostics.Debug.Assert(chunkState < chunkLoadingTask.wantedChunkState);

        if (chunkStateInStorage == ChunkState.UNKNOW) {
            if(chunkLoadingTask.wantedChunkState == ChunkState.STORAGELOADED) {
                return true;
            }
            System.Diagnostics.Debug.Assert(chunkState == ChunkState.EMPTY, "chunkState == ChunkState.EMPTY");
            ChunkWaitingTask chunkWaitingTaskStorage = new ChunkWaitingTask(chunkLoadingTask, 1);
            bool added = chunkLoader.NewJob(new ChunkLoadingTask(this, ChunkState.STORAGELOADED, chunkWaitingTaskStorage));
            if(!added) throw new Exception("chunkLoadingTaskStorage not added");
            return false;
        }
        
        switch (chunkLoadingTask.wantedChunkState) {
            case ChunkState.EMPTY:
                return true;
            case ChunkState.GENERATEDTERRAIN:
                return true;
            case ChunkState.BLOCKGENERATED:
                return AddTask(chunkLoader, chunkLoadingTask, ChunkState.GENERATEDTERRAIN, ChunkState.GENERATEDTERRAIN);
            case ChunkState.LIGHTING:
                Chunk topChunk = chunkManager.GetChunk(position + new Vector3D<int>(0, CHUNK_SIZE, 0));
                if (topChunk.chunkState < ChunkState.BLOCKGENERATED) {
                    ChunkWaitingTask chunkWaitingTask = new ChunkWaitingTask(chunkLoadingTask, 1);
                    ChunkLoadingTask? alreadyExistingTask = chunkLoader.FindTask(topChunk, ChunkState.BLOCKGENERATED);
                    if (alreadyExistingTask is not null) {
                        alreadyExistingTask.parents.Add(chunkWaitingTask);
                    } else {
                        bool added =
                            chunkLoader.NewJob(new ChunkLoadingTask(topChunk, ChunkState.BLOCKGENERATED, chunkWaitingTask));
                        if (!added) throw new Exception("topChunk not added concurrent access error");
                    }

                    return false;
                }
                
                if (!LightCalculator.IsChunkOkToGenerateLightBelow(topChunk)) {
                    ChunkWaitingTask chunkWaitingTask = new ChunkWaitingTask(chunkLoadingTask, 1);
                    ChunkLoadingTask? alreadyExistingTask = chunkLoader.FindTask(topChunk, ChunkState.LIGHTING);
                    if (alreadyExistingTask is not null) {
                        alreadyExistingTask.parents.Add(chunkWaitingTask);
                    } else {
                        bool added =
                            chunkLoader.NewJob(new ChunkLoadingTask(topChunk, ChunkState.LIGHTING, chunkWaitingTask));
                        if (!added) throw new Exception("topChunk not added concurrent access error");
                    }

                    return false;
                }

                return AddTask(chunkLoader, chunkLoadingTask, ChunkState.BLOCKGENERATED, ChunkState.BLOCKGENERATED);

            case ChunkState.DRAWABLE:
                return AddTask(chunkLoader, chunkLoadingTask, ChunkState.BLOCKGENERATED, ChunkState.LIGHTING);
            default:
                throw new ArgumentException("ChunkState not found");
        }
    }
    public static Span<BlockData> GetBlockSpan(BlockData[,,] blocks) {
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(blocks);
        return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, BlockData>(ref reference), blocks.Length);
    }



    private bool AddTask(ChunkLoader chunkLoader, ChunkLoadingTask chunkLoadingTask, ChunkState chunkStateGoalNeighboor, ChunkState chunkStateGoal) {   
        ChunkWaitingTask chunkWaitingTask = new ChunkWaitingTask(chunkLoadingTask, FaceExtendedConst.FACES.Count);
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> positionChunkToLoad = position + (FaceExtendedOffset.GetOffsetOfFace(face) * CHUNK_SIZE);
            Chunk neighborChunk = chunkManager.GetChunk(positionChunkToLoad);
            if (neighborChunk.chunkState >= chunkStateGoalNeighboor) {
                chunkWaitingTask.counter--;
                continue;
            }
                    
            ChunkLoadingTask? alreadyExistingTask = chunkLoader.FindTask(neighborChunk, chunkStateGoalNeighboor);
            if (alreadyExistingTask is not null) {
                alreadyExistingTask.parents.Add(chunkWaitingTask); 
            } else {
                bool added = chunkLoader.NewJob(new ChunkLoadingTask(neighborChunk, chunkStateGoalNeighboor, chunkWaitingTask));
                if(!added) chunkWaitingTask.counter--;
            }
        }
        if (chunkState != chunkStateGoal) {
            ChunkLoadingTask? alreadyExistingTask = chunkLoader.FindTask(this, chunkStateGoal);
            if (alreadyExistingTask is not null) {
                alreadyExistingTask.parents.Add(chunkWaitingTask);
                chunkWaitingTask.counter++;
            } else {
                bool added = chunkLoader.NewJob(new ChunkLoadingTask(this, chunkStateGoal, chunkWaitingTask));
                if(added) chunkWaitingTask.counter++;
            }
        }
        return chunkWaitingTask.counter <= 0;
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
    public BlockData GetBlockData(Vector3D<int> position) => chunkStrategy.GetBlockData(position);
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

    public bool IsRequiredByChunkLoader() => chunkTaskOfChunk.Count > 0;
    public void AddRequiredByChunkLoader(ChunkLoadingTask chunkLoadingTask) => chunkTaskOfChunk.Add(chunkLoadingTask);

    public void RemoveRequiredByChunkLoader(ChunkLoadingTask chunkLoadingTask) {
        chunkTaskOfChunk.Remove(chunkLoadingTask);   
        if(chunkTaskOfChunk.Count == 0) {
            OnChunkFinishLoading?.Invoke();
        }
    }
    
    public bool IsRequiredByChunkSaver() => requiredByChunkSaver > 0;
    public void AddRequiredByChunkSaver() => Interlocked.Increment(ref requiredByChunkSaver);

    public void RemoveRequiredByChunkSaver() {
        Interlocked.Decrement(ref requiredByChunkSaver);
        if(requiredByChunkSaver == 0) {
            OnChunkFinishSaving?.Invoke();
        }
    }
    
    public AABBCube GetAABBCube() => aabbCube;
    
    public void Reset(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator, IChunkStorage chunkStorage, IChunkLightManager chunkLightManager) {
        System.Diagnostics.Debug.Assert(!IsRequiredByChunkLoader(), "is required by chunk loader");
        System.Diagnostics.Debug.Assert(!IsRequiredByChunkSaver(), "is required by chunk unloader");
        System.Diagnostics.Debug.Assert(!blockModified, "block is modified but not saved");
        
        if(IsRequiredByChunkLoader()) {
            throw new Exception("Chunk is still required by chunk loader");
        }
        this.position = position;
        this.chunkManager = chunkManager;
        this.worldGenerator = worldGenerator;
        this.aabbCube = new AABBCube(new Vector3(position.X, position.Y, position.Z), new Vector3(position.X + Chunk.CHUNK_SIZE, position.Y + Chunk.CHUNK_SIZE, position.Z + Chunk.CHUNK_SIZE));
        this.chunkStateInStorage = ChunkState.UNKNOW;
        this.chunkStorage = chunkStorage;
        this.chunkLightManager = chunkLightManager;
        Debug(false);
        chunksNeighbors = new Chunk[6];
        chunkState = DEFAULTSTARTINGCHUNKSTATE;
        this.chunkStrategy = new ChunkEmptyStrategy(this);
        blockModified = false;
        chunkFace = null;
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    }
    

    public override string ToString() {
        string chunkFaceString = this.chunkFace is not null ? ChunkFaceUtils.toString((ChunkFace)this.chunkFace!) : "";
        return $"Chunk {position.X} {position.Y} {position.Z} chunkState: {chunkState} \n" +
               $"faces {chunkFaceString}";
    }

    public override int GetHashCode() {
        return position.GetHashCode();
    }

}