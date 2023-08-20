using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
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

    private Action disposeAction;
    private List<CubeVertex> vertices;
    private static ChunkBufferObjectManager chunkBufferObjectManager;

    private int nbVertex = 0;


    public ChunkDrawableStrategy(Chunk chunk) : base(chunk) {
        if (chunk.chunkState != ChunkState.BLOCKGENERATED) {
            throw new Exception("failed to init chunkDrawableStrategy because the chunk is not BLOCKGENERATED");
        }
    }


    public static void InitStaticMembers(Texture cubeTexture, Game game) {
        if (chunkBufferObjectManager == null)
            chunkBufferObjectManager = new ChunkBufferObjectManager(cubeTexture, game);
    }

    public override void init() {
        setupNeighbors();
    }

    public override void load() {
        initVertices();
    }

    public override void finish() {
        chunk.chunkState = ChunkState.DRAWABLE;
        chunk.chunkManager.addChunkToUpdate(chunk);
    }


    protected virtual void setupNeighbors() {
        chunk.chunksNeighbors = new Chunk[6];
        foreach (Face face in Enum.GetValues(typeof(Face))) {
            Chunk newChunk = chunk.chunkManager.getChunk(chunk.position + (FaceOffset.getOffsetOfFace(face) * Chunk.CHUNK_SIZE));
            if(newChunk.chunkState < minimumChunkStateOfNeighbors()) {
                throw new Exception("try to setup a chunk with a lower state than the minimum");
            }
            chunk.chunksNeighbors[(int)face] = newChunk;
        }
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.DRAWABLE;


    public override void updateChunkVertex() {
        updateCubeVertices();
        if (!openGlSetup && nbVertex == 0) {
            needToUpdateChunkVertices = false;
            return;
        }

        needToSendVertices = true;
        needToUpdateChunkVertices = false;
    }

    public override void update(double deltaTime) {
        if (nbVertex > 0 && !openGlSetup) setOpenGl();
        if (openGlSetup && needToSendVertices) sendCubeVertices();
        if (needToUpdateChunkVertices) updateChunkVertex();
        disposeAction?.Invoke();
    }


    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = Chunk.blockFactory.getBlockIdByName(name);

        updateBlocksAround(x, y, z);
        needToUpdateChunkVertices = true;
    }

    public override ChunkState minimumChunkStateOfNeighbors() => ChunkState.BLOCKGENERATED;

    private void initVertices() {
        updateCubeVertices();
        if (nbVertex == 0) {
            return;
        }

        needToSendVertices = true;
    }


    private void setOpenGl() {
        chunkBufferObjectManager.addChunkToRegion(chunk);
        visible = true;
        openGlSetup = true;
    }


    public override ReadOnlySpan<CubeVertex> getVertices() {
        return vertices.ToArray();
    }

    private void sendCubeVertices() {
        chunkBufferObjectManager.needToUpdateChunk(chunk);
        needToSendVertices = false;
    }

    private void updateCubeVertices() {
        Vector3D<float> positionFloat =
            new Vector3D<float>(chunk.position.X, chunk.position.Y, chunk.position.Z);
        vertices = new List<CubeVertex>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData block = chunk.blocks[x, y, z];
                    if (block.id == 0 || Chunk.blockFactory.getBlockNameById(block.id)
                            .Equals(BlockFactory.AIR_BLOCK)) continue;
                    FaceFlag faces = getFaces(x, y, z);
                    if (faces > 0) {
                        vertices.AddRange(Chunk.blockFactory.blocksReadOnly[block.id].textureBlock
                            .getCubeVertices(faces, new Vector3D<float>(x, y, z), positionFloat));
                    }
                }
            }
        }

        nbVertex = vertices.Count;
    }

    private FaceFlag getFaces(int x, int y, int z) {
        FaceFlag faceFlag = FaceFlag.EMPTY;
        if (isBlockTransparent(x - 1, y, z)) {
            //X
            faceFlag |= FaceFlag.RIGHT;
        }

        if (isBlockTransparent(x + 1, y, z)) {
            faceFlag |= FaceFlag.LEFT;
        }

        if (isBlockTransparent(x, y - 1, z)) {
            // Y
            faceFlag |= FaceFlag.BOTTOM;
        }

        if (isBlockTransparent(x, y + 1, z)) {
            faceFlag |= FaceFlag.TOP;
        }

        if (isBlockTransparent(x, y, z - 1)) {
            // Z
            faceFlag |= FaceFlag.BACK;
        }

        if (isBlockTransparent(x, y, z + 1)) {
            faceFlag |= FaceFlag.FRONT;
        }

        return faceFlag;
    }

    private bool isBlockTransparent(int x, int y, int z) {
        BlockData blockData;
        if (y < 0) {
            blockData = chunk.chunksNeighbors[(int)Face.BOTTOM]
                .getBlockData(new Vector3D<int>(x, y + (int)Chunk.CHUNK_SIZE, z));
        } else if (y >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors[(int)Face.TOP]
                .getBlockData(new Vector3D<int>(x, y - (int)Chunk.CHUNK_SIZE, z));
        } else if (x < 0) {
            blockData = chunk.chunksNeighbors[(int)Face.LEFT]
                .getBlockData(new Vector3D<int>(x + (int)Chunk.CHUNK_SIZE, y, z));
        } else if (x >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors[(int)Face.RIGHT]
                .getBlockData(new Vector3D<int>(x - (int)Chunk.CHUNK_SIZE, y, z));
        } else if (z < 0) {
            blockData = chunk.chunksNeighbors[(int)Face.BACK]
                .getBlockData(new Vector3D<int>(x, y, z + (int)Chunk.CHUNK_SIZE));
        } else if (z >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors[(int)Face.FRONT]
                .getBlockData(new Vector3D<int>(x, y, z - (int)Chunk.CHUNK_SIZE));
        } else {
            blockData = chunk.blocks[x, y, z];
        }

        return blockData.id == 0 || Chunk.blockFactory.isBlockTransparent(blockData);
    }


    internal void updateBlocksAround(int x, int y, int z) {
        if (x == 0) chunk.chunksNeighbors[(int)Face.LEFT].updateChunkVertex();
        if (x == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.RIGHT].updateChunkVertex();

        if (y == 0) chunk.chunksNeighbors[(int)Face.BOTTOM].updateChunkVertex();
        ;
        if (y == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.TOP].updateChunkVertex();

        if (z == 0) chunk.chunksNeighbors[(int)Face.BACK].updateChunkVertex();
        if (z == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.FRONT].updateChunkVertex();
    }

    public void hide() {
        if (!visible) return;
        visible = false;
        chunkBufferObjectManager.removeChunk(chunk);
    }

    protected override Vector3D<float> ChunkStrategyColor() => new Vector3D<float>(1, 1, 0);


    public override void Dispose() {
        if (visible) {
            hide();
        }

        chunk.chunkManager.removeChunkToUpdate(chunk);
    }


    static ChunkDrawableStrategy() {
        Face[] faces = (Face[])Enum.GetValues(typeof(Face));
        dependatesChunkOffset = new Vector3D<int>[faces.Length];
        for (int i = 0; i < faces.Length; i++) {
            dependatesChunkOffset[i] = FaceOffset.getOffsetOfFace(faces[i]) * Chunk.CHUNK_SIZE;
        }
    }

    public static readonly Vector3D<int>[] dependatesChunkOffset;
}