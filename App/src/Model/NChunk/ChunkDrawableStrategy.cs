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
        chunk.chunkState = ChunkState.DRAWLOADING;
        SetupNeighbors();
    }

    public override void Load() {
        InitChunkFaces();
    }

    public override void Finish() {
        chunk.chunkState = ChunkState.DRAWABLE;
        chunk.chunkManager.AddChunkToUpdate(chunk);
    }

    private bool IsChunkEmpty() => (chunkFace! & ChunkFace.EMPTYCHUNK) == ChunkFace.EMPTYCHUNK;

    protected virtual void SetupNeighbors() {
        chunk.chunksNeighbors = new Chunk[26];
        foreach (FaceExtended face in FaceExtendedConst.FACES) {
            Vector3D<int> positionNeibor = chunk.position + (FaceExtendedOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE);
            System.Diagnostics.Debug.Assert(chunk.chunkManager.ContainChunk(positionNeibor),
                "chunk must be already generated");
            Chunk newChunk = chunk.chunkManager.GetChunk(positionNeibor);
            System.Diagnostics.Debug.Assert(newChunk.chunkState >= MinimumChunkStateOfNeighbors(), "try to setup a chunk with a lower state than the minimum"); 
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }



    public override void UpdateChunkVertex() {
        chunkFace = ChunkFaceUtils.GetChunkFaceFlags(Chunk.blockFactory!, chunk.blocks);
        if (!openGlSetup && IsChunkEmpty()) {
            needToUpdateChunkVertices = false;
            return;
        }
        needToSendVertices = true;
        needToUpdateChunkVertices = false;
    }

    public override void Update(double deltaTime) {
        if (!IsChunkEmpty() && !openGlSetup) SetOpenGl();
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
        if (IsChunkEmpty()) {
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
        const int maxIndex = Chunk.CHUNK_SIZE - 1;
        if (x == 0) chunk.chunksNeighbors![(int)FaceExtended.LEFT].UpdateChunkVertex();
        if (x == maxIndex) chunk.chunksNeighbors![(int)Face.RIGHT].UpdateChunkVertex();

        if (y == 0) chunk.chunksNeighbors![(int)FaceExtended.BOTTOM].UpdateChunkVertex();
        if (y == maxIndex) chunk.chunksNeighbors![(int)Face.TOP].UpdateChunkVertex();

        if (z == 0) chunk.chunksNeighbors![(int)FaceExtended.BACK].UpdateChunkVertex();
        if (z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.FRONT].UpdateChunkVertex();
        
        
        if (x == 0 && y == 0) chunk.chunksNeighbors![(int)FaceExtended.LEFTBOTTOM].UpdateChunkVertex();
        if (x == maxIndex && y == 0) chunk.chunksNeighbors![(int)FaceExtended.RIGHTBOTTOM].UpdateChunkVertex();
        if (x == 0 && y == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.LEFTTOP].UpdateChunkVertex();
        if (x == maxIndex && y == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.RIGHTTOP].UpdateChunkVertex();
        
        
        if (x == 0 && z == 0) chunk.chunksNeighbors![(int)FaceExtended.LEFTBACK].UpdateChunkVertex();
        if (x == maxIndex && z == 0) chunk.chunksNeighbors![(int)FaceExtended.RIGHTBACK].UpdateChunkVertex();
        if (x == 0 && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.LEFTFRONT].UpdateChunkVertex();
        if (x == maxIndex && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.RIGHTFRONT].UpdateChunkVertex();
        
        if (y == 0 && z == 0) chunk.chunksNeighbors![(int)FaceExtended.BOTTOMBACK].UpdateChunkVertex();
        if (y == maxIndex && z == 0) chunk.chunksNeighbors![(int)FaceExtended.TOPBACK].UpdateChunkVertex();
        if (y == 0 && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.BOTTOMFRONT].UpdateChunkVertex();
        if (y == maxIndex && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.TOPFRONT].UpdateChunkVertex();

        if (x == 0 && y == 0 && z == 0 ) chunk.chunksNeighbors![(int)FaceExtended.LEFTBOTTOMBACK].UpdateChunkVertex();
        if (x == maxIndex && y == 0 && z == 0) chunk.chunksNeighbors![(int)FaceExtended.RIGHTBOTTOMBACK].UpdateChunkVertex();
        if (x == 0 && y == maxIndex && z == 0) chunk.chunksNeighbors![(int)FaceExtended.LEFTTOPBACK].UpdateChunkVertex();
        if (x == 0 && y == 0 && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.LEFTBOTTOMFRONT].UpdateChunkVertex();
        if (x == maxIndex && y == maxIndex && z == 0) chunk.chunksNeighbors![(int)FaceExtended.RIGHTTOPBACK].UpdateChunkVertex();
        if (x == maxIndex && y == 0 && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.RIGHTBOTTOMFRONT].UpdateChunkVertex();
        if (x == 0 && y == maxIndex && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.LEFTTOPFRONT].UpdateChunkVertex();
        if (x == maxIndex && y == maxIndex && z == maxIndex) chunk.chunksNeighbors![(int)FaceExtended.RIGHTTOPFRONT].UpdateChunkVertex();
    }

    public void Hide() {
        chunk.chunkManager.RemoveChunkToUpdate(chunk);
        if (!visible) return;
        visible = false;
        chunkBufferObjectManager!.RemoveChunk(chunk);
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