using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkDrawableStrategy : ChunkStrategy
{
  
    private bool displayable;
    private int nbVertex = 0;
    private Action? delegateForUpdate;
    private Action? actionSendVertices;
    private BufferObject<CubeVertex> Vbo;
    private VertexArrayObject<CubeVertex, uint> Vao;
    
    internal static Texture cubeTexture;
    public static object staticObjectLock = new object();

    public ChunkDrawableStrategy(Chunk chunk) : base(chunk) {
        displayable = false;
        nbVertex = 0;
    }
    
   
    private void initStaticMembers()
    {
        lock (staticObjectLock) {
            if (cubeTexture == null) {
                cubeTexture = TextureManager.getInstance().textures["spriteSheet.png"];
            }
        }
    }

    public override async Task init() {
        if (chunk.chunkState != ChunkState.BLOCKGENERATED) {
            chunk.chunkStrategy = new ChunkBlockGeneratedStrategy(chunk);
            await chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        await updateNeighboorChunkState(ChunkState.BLOCKGENERATED);
        initStaticMembers();
        await displayChunk();
        chunk.chunkState = ChunkState.DRAWABLE;
        
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.DRAWABLE;


    public override async Task updateChunkVertex() {
        if(!displayable) return;
        List<CubeVertex> vertices = await getCubeVertices();
        nbVertex = vertices.Count;
        sendCubeVertices(vertices);
    }

    public override void update(double deltaTime) {
        delegateForUpdate?.Invoke();
        delegateForUpdate = null;
        actionSendVertices?.Invoke();
        actionSendVertices = null;
    }

    public override void draw(GL gl, double deltaTime) {

        if(!displayable) return;
        if(nbVertex == 0) return;
        Vao.Bind();
        Chunk.cubeShader.Use();
        cubeTexture.Bind();

        var model = Matrix4x4.CreateTranslation(new Vector3(chunk.position.X, chunk.position.Y, chunk.position.Z));
        Chunk.cubeShader.SetUniform("model", model);


        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
        updateBlocksAround(x, y, z);
        if(displayable) updateChunkVertex();
    }


    private async Task displayChunk()
    {
        if(displayable) return;
        await setOpenGl();
        List<CubeVertex> cubeVertices = await getCubeVertices();
        if (cubeVertices.Count == 0) {
            return;
        }
        await sendCubeVertices(cubeVertices);
        displayable = true;
    }
    
    
    private async Task setOpenGl()
    {
        TaskCompletionSource taskSetOpenGl = new TaskCompletionSource();
        delegateForUpdate = () =>
        {
            const int nbFacePerBlock = 6;
            const int nbVertexPerFace = 6;
            int nbVertexMax = (int)(nbVertexPerFace * nbFacePerBlock * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE *
                                    Chunk.CHUNK_SIZE);
            Vbo = new BufferObject<CubeVertex>(chunk.Gl, nbVertexMax, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<CubeVertex, uint>(chunk.Gl, Vbo);

            Vao.Bind();
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
            taskSetOpenGl.SetResult();
        };
        await taskSetOpenGl.Task;
    }
    
    private async Task sendCubeVertices(List<CubeVertex> vertices) {
        TaskCompletionSource taskSendVertices = new TaskCompletionSource();
        actionSendVertices = () =>
        {
            Vbo.Bind();
            Vbo.sendData(vertices.ToArray(), 0);
            taskSendVertices.SetResult();
        };
        await taskSendVertices.Task;
    }
    
    private async Task<List<CubeVertex>> getCubeVertices() {
        var listVertices = new List<CubeVertex>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData block = chunk.blocks[x, y, z];
                    if(block.id == 0  || Chunk.blockFactory.getBlockNameById(block.id).Equals(BlockFactory.AIR_BLOCK)) continue;
                    FaceFlag faces = getFaces(x ,y, z);
                    if (faces > 0) {
                        listVertices.AddRange(Chunk.blockFactory.blocksReadOnly[block.id].textureBlock
                            .getCubeVertices(faces, new Vector3D<float>(x, y, z)));
                    }
                }
            }
        }
        nbVertex = listVertices.Count;
        return listVertices;
    }
    
    private FaceFlag getFaces(int x, int y, int z) {
        FaceFlag faceFlag = FaceFlag.EMPTY;
        //X
        if (isBlockTransparent(x - 1, y, z)) {
            faceFlag |= FaceFlag.RIGHT;
        }

        if (isBlockTransparent(x + 1, y, z)) {
            faceFlag |= FaceFlag.LEFT;
        }

        //y
        if (isBlockTransparent(x, y - 1, z)) {
            faceFlag |= FaceFlag.BOTTOM;
        }

        if (isBlockTransparent(x, y  + 1, z)) {
            faceFlag |= FaceFlag.TOP;
        }

        //z
        if (isBlockTransparent(x, y, z - 1)) {
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
            blockData = chunk.chunksNeighbors[(int)Face.BOTTOM]!
                .getBlockData(new Vector3D<int>(x, y + (int)Chunk.CHUNK_SIZE, z));
        }else if (y >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors[(int)Face.TOP]!
                .getBlockData(new Vector3D<int>(x, y - (int)Chunk.CHUNK_SIZE, z));
        }else if (x < 0) {
            blockData = chunk.chunksNeighbors[(int)Face.LEFT]!
                .getBlockData(new Vector3D<int>(x + (int)Chunk.CHUNK_SIZE, y, z));
        }else if (x >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors[(int)Face.RIGHT]!
                .getBlockData(new Vector3D<int>(x - (int)Chunk.CHUNK_SIZE, y, z));
        } else if (z < 0) {
            blockData = chunk.chunksNeighbors[(int)Face.BACK]!
                .getBlockData(new Vector3D<int>(x, y, z + (int)Chunk.CHUNK_SIZE));
        }else if (z >= Chunk.CHUNK_SIZE) {
            blockData = chunk.chunksNeighbors[(int)Face.FRONT]!
                .getBlockData(new Vector3D<int>(x, y, z - (int)Chunk.CHUNK_SIZE));
        } else {
            blockData = chunk.blocks[x, y, z];
        }
        return blockData.id == 0 || Chunk.blockFactory.isBlockTransparent(blockData);
    }

    
    internal void updateBlocksAround(int x, int y, int z)
    {
        if(x == 0) chunk.chunksNeighbors[(int)Face.LEFT]!.updateChunkVertex();
        if(x == Chunk.CHUNK_SIZE- 1) chunk.chunksNeighbors[(int)Face.RIGHT]!.updateChunkVertex();
        
        if(y == 0) chunk.chunksNeighbors[(int)Face.BOTTOM]!.updateChunkVertex();;
        if(y == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.TOP]!.updateChunkVertex();

        if(z == 0) chunk.chunksNeighbors[(int)Face.BACK]!.updateChunkVertex();
        if(z == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.FRONT]!.updateChunkVertex();

    }


    public override void Dispose() {
        Vao?.Dispose();
        Vbo?.Dispose();

        chunk.chunkStrategy = new ChunkBlockGeneratedStrategy(chunk);
        chunk.chunkState = ChunkState.BLOCKGENERATED;
        chunk.chunksNeighbors = null;
    }
}