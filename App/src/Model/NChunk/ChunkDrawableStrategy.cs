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

    public int nbVertex { get; private set; }


    public ChunkDrawableStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.BLOCKGENERATED) {
            throw new Exception("failed to init chunkDrawableStrategy because the chunk is not BLOCKGENERATED");
        }
    }


    public static void InitStaticMembers(Texture cubeTexture, Game game) {
        if (chunkBufferObjectManager == null)
            chunkBufferObjectManager = new ChunkBufferObjectManager(cubeTexture, game);
    }

    public override void Init() {
        setupNeighbors();
    }

    public override void Load() {
        InitVertices();
    }

    public override void Finish() {
        chunk.chunkState = ChunkState.DRAWABLE;
        chunk.chunkManager.AddChunkToUpdate(chunk);
    }


    protected virtual void setupNeighbors() {
        chunk.chunksNeighbors = new Chunk[6];
        foreach (Face face in Enum.GetValues(typeof(Face))) {
            Chunk newChunk = chunk.chunkManager.GetChunk(chunk.position + (FaceOffset.GetOffsetOfFace(face) * Chunk.CHUNK_SIZE));
            System.Diagnostics.Debug.Assert(newChunk.chunkState >= MinimumChunkStateOfNeighbors(), "try to setup a chunk with a lower state than the minimum"); 
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }

    public override ChunkState GetChunkStateOfStrategy() => ChunkState.DRAWABLE;


    public override void UpdateChunkVertex() {
        UpdateCubeVertices();
        if (!openGlSetup && nbVertex == 0) {
            needToUpdateChunkVertices = false;
            return;
        }

        needToSendVertices = true;
        needToUpdateChunkVertices = false;
    }

    public override void Update(double deltaTime) {
        if (nbVertex > 0 && !openGlSetup) SetOpenGl();
        if (openGlSetup && needToSendVertices) SendCubeVertices();
        if (needToUpdateChunkVertices) UpdateChunkVertex();
    }


    public override void SetBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = Chunk.blockFactory!.GetBlockIdByName(name);

        UpdateBlocksAround(x, y, z);
        needToUpdateChunkVertices = true;
    }

    public override ChunkState MinimumChunkStateOfNeighbors() => ChunkState.BLOCKGENERATED;

    private void InitVertices() {
        UpdateCubeVertices();
        if (nbVertex == 0) {
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

    private void UpdateCubeVertices() {
        Vector3D<float> positionFloat =
            new Vector3D<float>(chunk.position.X, chunk.position.Y, chunk.position.Z);
        List<CubeVertex> vertices = new List<CubeVertex>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData block = chunk.blocks[x, y, z];
                    if (block.id == 0 || Chunk.blockFactory!.GetBlockNameById(block.id)
                            .Equals(BlockFactory.AIR_BLOCK)) continue;
                    FaceFlag faces = GetFaces(x, y, z);
                    if (faces > 0) {
                        Chunk.blockFactory.blocksReadOnly[block.id].textureBlock!.AddCubeVerticesToList(vertices, faces, new Vector3D<float>(x, y, z), positionFloat);
                    }
                }
            }
        }

        nbVertex = vertices.Count;
    }

    private FaceFlag GetFaces(int x, int y, int z) {
        FaceFlag faceFlag = FaceFlag.EMPTY;
        if (IsBlockTransparent(x - 1, y, z)) {
            //X
            faceFlag |= FaceFlag.RIGHT;
        }

        if (IsBlockTransparent(x + 1, y, z)) {
            faceFlag |= FaceFlag.LEFT;
        }

        if (IsBlockTransparent(x, y - 1, z)) {
            // Y
            faceFlag |= FaceFlag.BOTTOM;
        }

        if (IsBlockTransparent(x, y + 1, z)) {
            faceFlag |= FaceFlag.TOP;
        }

        if (IsBlockTransparent(x, y, z - 1)) {
            // Z
            faceFlag |= FaceFlag.BACK;
        }

        if (IsBlockTransparent(x, y, z + 1)) {
            faceFlag |= FaceFlag.FRONT;
        }

        return faceFlag;
    }

    private bool IsBlockTransparent(int x, int y, int z) {
        BlockData blockData;
        if (y < 0) {
            blockData = chunk.chunksNeighbors![(int)Face.BOTTOM]
                .GetBlockData(new Vector3D<int>(x, y + (int)Chunk.CHUNK_SIZE, z));
        } else if (y >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors![(int)Face.TOP]
                .GetBlockData(new Vector3D<int>(x, y - (int)Chunk.CHUNK_SIZE, z));
        } else if (x < 0) {
            blockData = chunk.chunksNeighbors![(int)Face.LEFT]
                .GetBlockData(new Vector3D<int>(x + (int)Chunk.CHUNK_SIZE, y, z));
        } else if (x >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors![(int)Face.RIGHT]
                .GetBlockData(new Vector3D<int>(x - (int)Chunk.CHUNK_SIZE, y, z));
        } else if (z < 0) {
            blockData = chunk.chunksNeighbors![(int)Face.BACK]
                .GetBlockData(new Vector3D<int>(x, y, z + (int)Chunk.CHUNK_SIZE));
        } else if (z >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors![(int)Face.FRONT]
                .GetBlockData(new Vector3D<int>(x, y, z - (int)Chunk.CHUNK_SIZE));
        } else {
            blockData = chunk.blocks[x, y, z];
        }

        return blockData.id == 0 || Chunk.blockFactory!.IsBlockTransparent(blockData);
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
    }

    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(1, 1, 0);


    public override void Dispose() {
        if (visible) {
            Hide();
        }

        chunk.chunkManager.RemoveChunkToUpdate(chunk);
    }


    static ChunkDrawableStrategy() {
        Face[] faces = (Face[])Enum.GetValues(typeof(Face));
        DependatesChunkOffset = new Vector3D<int>[faces.Length];
        for (int i = 0; i < faces.Length; i++) {
            DependatesChunkOffset[i] = FaceOffset.GetOffsetOfFace(faces[i]) * Chunk.CHUNK_SIZE;
        }
    }

    public static readonly Vector3D<int>[] DependatesChunkOffset;
}