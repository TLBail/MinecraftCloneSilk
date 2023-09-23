using System.Numerics;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.RegionDrawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkDrawableStrategy : ChunkStrategy
{
    private bool visible = false;
    private bool openGlSetup = false;
    private bool needToSendVertices = false;
    private bool needToUpdateChunkVertices = false;

    private static ChunkBufferObjectManager? chunkBufferObjectManager;

    private ChunkFace? chunkFace; 
    
    public static void InitStaticMembers( ChunkBufferObjectManager chunkBufferObjectManager) {
        ChunkDrawableStrategy.chunkBufferObjectManager = chunkBufferObjectManager;
    }
    public ChunkDrawableStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.LIGHTING) {
            throw new Exception("failed to init chunkDrawableStrategy because the chunk is not LIGHTING");
        }
    }

    public override ChunkState GetChunkStateOfStrategy() => ChunkState.DRAWABLE;
    public override void Init() {
        setupNeighbors();
    }

    public override void Load() {
        InitChunkFaces();
    }

    public override void Finish() {
        chunk.chunkState = ChunkState.DRAWABLE;
        chunk.chunkManager.AddChunkToUpdate(chunk);
    }

    private bool isChunkEmpty() => (chunkFace! & ChunkFace.EMPTYCHUNK) == ChunkFace.EMPTYCHUNK;

    protected virtual void setupNeighbors() {
        chunk.chunksNeighbors = new Chunk[6];
        foreach (Face face in Enum.GetValues(typeof(Face))) {
            Chunk newChunk = chunk.chunkManager.GetChunk(chunk.position + (FaceOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE));
            System.Diagnostics.Debug.Assert(newChunk.chunkState >= MinimumChunkStateOfNeighbors(), "try to setup a chunk with a lower state than the minimum"); 
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }



    public override void UpdateChunkVertex() {
        chunkFace = ChunkFaceUtils.GetChunkFaceFlags(Chunk.blockFactory!, chunk.blocks);
        if (!openGlSetup && isChunkEmpty()) {
            needToUpdateChunkVertices = false;
            return;
        }
        needToSendVertices = true;
        needToUpdateChunkVertices = false;
    }

    public override void Update(double deltaTime) {
        if (!isChunkEmpty() && !openGlSetup) SetOpenGl();
        if (openGlSetup && needToSendVertices) SendCubeVertices();
        if (needToUpdateChunkVertices) UpdateChunkVertex();
    }


    public override void SetBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = Chunk.blockFactory!.GetBlockIdByName(name);

        UpdateBlocksAround(x, y, z);
        needToUpdateChunkVertices = true;
    }

    public override ChunkState MinimumChunkStateOfNeighbors() => ChunkState.BLOCKGENERATED;

    private void InitChunkFaces() {
        chunkFace = ChunkFaceUtils.GetChunkFaceFlags(Chunk.blockFactory!, chunk.blocks);
        if (isChunkEmpty()) {
            return;
        }
        needToSendVertices = true;
    }


    private void SetOpenGl() {
        chunkBufferObjectManager!.AddChunkToRegion(chunk);
        visible = true;
        openGlSetup = true;
    }



    private void SendCubeVertices() {
        chunkBufferObjectManager!.NeedToUpdateChunk(chunk);
        needToSendVertices = false;
    }

 


    internal void UpdateBlocksAround(int x, int y, int z) {
        if (x == 0) chunk.chunksNeighbors![(int)Face.LEFT].UpdateChunkVertex();
        if (x == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors![(int)Face.RIGHT].UpdateChunkVertex();

        if (y == 0) chunk.chunksNeighbors![(int)Face.BOTTOM].UpdateChunkVertex();
        if (y == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors![(int)Face.TOP].UpdateChunkVertex();

        if (z == 0) chunk.chunksNeighbors![(int)Face.BACK].UpdateChunkVertex();
        if (z == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors![(int)Face.FRONT].UpdateChunkVertex();
    }

    public void Hide() {
        if (!visible) return;
        visible = false;
        chunkBufferObjectManager!.RemoveChunk(chunk);
        chunk.chunkManager.RemoveChunkToUpdate(chunk);
    }

    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(1, 1, 0);

    static ChunkDrawableStrategy() {
        Face[] faces = (Face[])Enum.GetValues(typeof(Face));
        DependatesChunkOffset = new Vector3D<int>[faces.Length];
        for (int i = 0; i < faces.Length; i++) {
            DependatesChunkOffset[i] = FaceOffset.GetOffsetOfFace(faces[i]) * Chunk.CHUNK_SIZE;
        }
    }

    public static readonly Vector3D<int>[] DependatesChunkOffset;
}