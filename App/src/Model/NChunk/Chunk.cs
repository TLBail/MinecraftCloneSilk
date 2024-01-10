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
    public ChunkState chunkStateInStorage { get; internal set; } = ChunkState.EMPTY;
    public const ChunkState DEFAULTSTARTINGCHUNKSTATE = ChunkState.EMPTY;

    internal ChunkStrategy chunkStrategy;

    internal static BlockFactory? blockFactory;

    private int requiredByChunkSaver = 0;
    internal bool blockModified = false;
    private AABBCube aabbCube;

    public List<ChunkLoadingTask> chunkTaskOfChunk = new();

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
                return addTask(chunkLoader, chunkLoadingTask, ChunkState.GENERATEDTERRAIN, ChunkState.GENERATEDTERRAIN);
            case ChunkState.LIGHTING:
                return addTask(chunkLoader, chunkLoadingTask, ChunkState.BLOCKGENERATED, ChunkState.BLOCKGENERATED);
            case ChunkState.DRAWABLE:
                return addTask(chunkLoader, chunkLoadingTask, ChunkState.BLOCKGENERATED, ChunkState.LIGHTING);
            default:
                throw new ArgumentException("ChunkState not found");
        }
    }


    private bool addTask(ChunkLoader chunkLoader, ChunkLoadingTask chunkLoadingTask, ChunkState chunkStateGoalNeighboor, ChunkState chunkStateGoal) {   
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

    public bool IsRequiredByChunkLoader() => chunkTaskOfChunk.Count > 0;
    public void AddRequiredByChunkLoader(ChunkLoadingTask chunkLoadingTask) => chunkTaskOfChunk.Add(chunkLoadingTask);
    public void RemoveRequiredByChunkLoader(ChunkLoadingTask chunkLoadingTask) => chunkTaskOfChunk.Remove(chunkLoadingTask);
    
    public bool IsRequiredByChunkSaver() => requiredByChunkSaver > 0;
    public void AddRequiredByChunkSaver() => Interlocked.Increment(ref requiredByChunkSaver);
    public void RemoveRequiredByChunkSaver() => Interlocked.Decrement(ref requiredByChunkSaver);
    
    public AABBCube GetAABBCube() => aabbCube;
    
    public void Reset(Vector3D<int> position, IChunkManager chunkManager, IWorldGenerator worldGenerator, IChunkStorage chunkStorage) {
        System.Diagnostics.Debug.Assert(!IsRequiredByChunkLoader(), "is required by chunk loader");
        System.Diagnostics.Debug.Assert(!IsRequiredByChunkSaver(), "is required by chunk unloader");
        if(IsRequiredByChunkLoader()) {
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