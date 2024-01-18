﻿using System.Numerics;
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
    private bool chunkAddedToRegionBuffer = false;
    private bool needToUpdateChunkBuffer = false;

    private static ChunkBufferObjectManager? chunkBufferObjectManager;

    private ChunkFace? chunkFace;
    private bool IsUpdating = false;
    
    public static void InitStaticMembers( ChunkBufferObjectManager chunkBufferObjectManager) {
        ChunkDrawableStrategy.chunkBufferObjectManager = chunkBufferObjectManager;
    }
    public ChunkDrawableStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.LIGHTING) {
            throw new Exception("failed to init chunkDrawableStrategy because the chunk is not LIGHTING");
        }
    }

    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(1, 1, 0);
    public override ChunkState MinimumChunkStateOfNeighbors() => ChunkState.BLOCKGENERATED;
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
        IsUpdating = true;
    }


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
        chunkFace = ChunkFaceUtils.GetChunkFaceFlags(Chunk.blockFactory!, chunk.chunkData);
        UpdateVerticesNextFrame();
    }

    public override void Update(double deltaTime) {
        if (!IsChunkVisible() && !chunkAddedToRegionBuffer) AddChunkToRegionBuffer();
        if (chunkAddedToRegionBuffer && needToUpdateChunkBuffer) UpdateChunkBuffer();
        if (IsChunkVisible() || (chunkAddedToRegionBuffer && !needToUpdateChunkBuffer)) {
            chunk.chunkManager.RemoveChunkToUpdate(chunk);
            IsUpdating = false;
        }
    }

    private void UpdateVerticesNextFrame() {
        needToUpdateChunkBuffer = true;
        if (!IsUpdating) {
            chunk.chunkManager.AddChunkToUpdate(chunk);
            IsUpdating = true;
        }
    }


    public override void SetBlock(int x, int y, int z, string name) {
        chunk.chunkData.SetBlock(x, y, z,Chunk.blockFactory!.GetBlockData(name));
        UpdateLight(x, y, z);
        UpdateBlocksAround(x, y, z);
        UpdateChunkVertex();
    }

    private void UpdateLight(int x, int y, int z) {
        if (chunk.chunkData.IsOnlyOneBlock()) {
            //Todo unfuck this
        } else {
            chunk.chunkData.GetBlocks()[x, y, z].data1 = 15;
        }
    }


    private void InitChunkFaces() {
        chunkFace = ChunkFaceUtils.GetChunkFaceFlags(Chunk.blockFactory!, chunk.chunkData);
        if (IsChunkVisible()) {
            return;
        }
        needToUpdateChunkBuffer = true;
    }


    private void AddChunkToRegionBuffer() {
        chunkBufferObjectManager!.AddChunkToRegion(chunk);
        visible = true;
        chunkAddedToRegionBuffer = true;
    }



    private void UpdateChunkBuffer() {
        chunkBufferObjectManager!.NeedToUpdateChunk(chunk);
        needToUpdateChunkBuffer = false;
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
        if(IsUpdating)chunk.chunkManager.RemoveChunkToUpdate(chunk);
        if (!visible) return;
        visible = false;
        chunkBufferObjectManager!.RemoveChunk(chunk);
    }

    private bool IsChunkVisible() => (chunkFace! & ChunkFace.EMPTYCHUNK) == ChunkFace.EMPTYCHUNK; // Todo check if the chunk is fully opaque
}