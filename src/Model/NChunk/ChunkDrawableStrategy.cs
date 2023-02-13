using System.Numerics;
using MinecraftCloneSilk.Core;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.NChunk;

public class ChunkDrawableStrategy : ChunkStrategy
{
  
    private BufferObject<CubeVertex> Vbo;
    private VertexArrayObject<CubeVertex, uint> Vao;
    
    internal static Texture cubeTexture;
    public static object staticObjectLock = new object();

    private bool openGlSetup = false;
    private bool needToSendVertices = false;
    private bool needToUpdateChunkVertices = false;
    private Action disposeAction;
    private List<CubeVertex> vertices;

    
    
    private int nbVertex = 0;
    
    
    public ChunkDrawableStrategy(Chunk chunk) : base(chunk) {
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

    public override void init() {
        if (chunk.chunkState != minimumChunkStateOfNeighbors()) {
            chunk.chunkStrategy = new ChunkBlockGeneratedStrategy(chunk);
            chunk.chunkStrategy.init();
            chunk.chunkStrategy = this;
        }
        updateNeighboorChunkState(minimumChunkStateOfNeighbors());
        initStaticMembers();
        initVertices();
        chunk.chunkState = ChunkState.DRAWABLE;
    }

    public override ChunkState getChunkStateOfStrategy() => ChunkState.DRAWABLE;


    public override void updateChunkVertex() {
        if (!openGlSetup) return;
        updateCubeVertices();
        nbVertex = vertices.Count;
        sendCubeVertices();
        needToUpdateChunkVertices = false;
    }

    public override void update(double deltaTime) {
        if (!openGlSetup) setOpenGl();
        if (openGlSetup && needToSendVertices) sendCubeVertices();
        if(needToUpdateChunkVertices) updateChunkVertex();
        disposeAction?.Invoke();
    }

    public override void draw(GL gl, double deltaTime) {

        if(!openGlSetup) return;
        if(nbVertex == 0) return;
        Vao.Bind();
        Chunk.cubeShader.Use();
        cubeTexture.Bind();

        var model = Matrix4x4.CreateTranslation(new Vector3(chunk.position.X, chunk.position.Y, chunk.position.Z));
        Chunk.cubeShader.SetUniform("model", model);


        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }

    public override void setBlock(int x, int y, int z, string name) {
        lock (chunk.blocksLock) {
            chunk.blocks[x, y, z].id = name.GetHashCode();
        }
        updateBlocksAround(x, y, z);
        needToUpdateChunkVertices = true;
    }

    public override ChunkState minimumChunkStateOfNeighbors() => ChunkState.BLOCKGENERATED;
    
    private void initVertices()
    {
        updateCubeVertices();
        if (vertices.Count == 0) {
            return;
        }
        needToSendVertices = true;
    }
    
    
    private void setOpenGl()
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
        
        chunk.chunkManager.addChunkToDraw(chunk);
        openGlSetup = true;
    }
    
    private void sendCubeVertices() {
        Vbo.Bind();
        Vbo.sendData(vertices.ToArray(), 0);
        needToSendVertices = false;
    }
    
    private void updateCubeVertices() {
        lock (chunk.blocksLock) {
            lock (chunk.chunksNeighborsLock) {
                vertices = new List<CubeVertex>();
                for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                    for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                        for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                            BlockData block = chunk.blocks[x, y, z];
                            if(block.id == 0  || Chunk.blockFactory.getBlockNameById(block.id).Equals(BlockFactory.AIR_BLOCK)) continue;
                            FaceFlag faces = getFaces(x ,y, z);
                            if (faces > 0) {
                                vertices.AddRange(Chunk.blockFactory.blocksReadOnly[block.id].textureBlock
                                    .getCubeVertices(faces, new Vector3D<float>(x, y, z)));
                            }
                        }
                    }
                }
                nbVertex = vertices.Count;   
            }
        }
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
            lock (chunk.blocksLock) {
                blockData = chunk.blocks[x, y, z];
            }
        }
        return blockData.id == 0 || Chunk.blockFactory.isBlockTransparent(blockData);
    }

    
    internal void updateBlocksAround(int x, int y, int z)
    {
        lock (chunk.chunksNeighborsLock) {
            if(x == 0) chunk.chunksNeighbors[(int)Face.LEFT]!.updateChunkVertex();
            if(x == Chunk.CHUNK_SIZE- 1) chunk.chunksNeighbors[(int)Face.RIGHT]!.updateChunkVertex();
        
            if(y == 0) chunk.chunksNeighbors[(int)Face.BOTTOM]!.updateChunkVertex();;
            if(y == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.TOP]!.updateChunkVertex();

            if(z == 0) chunk.chunksNeighbors[(int)Face.BACK]!.updateChunkVertex();
            if(z == Chunk.CHUNK_SIZE - 1) chunk.chunksNeighbors[(int)Face.FRONT]!.updateChunkVertex();
        }
    }

    public override void debug(bool? setDebug = null) {
        chunk.debugMode = setDebug ?? !chunk.debugMode;

        
        if (!chunk.debugMode) {
            foreach (var debugRay in chunk.debugRays) {
                debugRay.remove();
            }
            chunk.debugRays.Clear();
        }
        else {
            //base
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y -0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));

            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y  - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE- 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            
            //top base

            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f , chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));

            
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            
            //between
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X  - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z- 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f, chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z+ Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            chunk.debugRays.Add( new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y - 0.5f, chunk.position.Z + Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(chunk.position.X - 0.5f , chunk.position.Y +  Chunk.CHUNK_SIZE - 0.5f, chunk.position.Z+ Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
            
            // diagonal
            chunk.debugRays.Add(new DebugRay(new Vector3D<float>(chunk.position.X - 0.5f, chunk.position.Y -0.5f, chunk.position.Z - 0.5f),
                new Vector3D<float>(chunk.position.X + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Y + Chunk.CHUNK_SIZE - 0.5f , chunk.position.Z +  Chunk.CHUNK_SIZE - 0.5f), new Vector3D<float>(1.0f, 1.0f, 0.0f)));
        }
    }


    public override void Dispose() {
        lock (chunk.chunkStrategyLock) {
            Vao?.Dispose();
            Vbo?.Dispose();
            chunk.chunkStrategy = new ChunkBlockGeneratedStrategy(chunk);
            chunk.chunkState = ChunkState.BLOCKGENERATED;
            
            lock (chunk.chunksNeighborsLock) {
                for (int i = 0; i < chunk.chunksNeighbors.Length; i++) { 
                    chunk.chunksNeighbors[i] = null;
                }   
            }
        }

    }
}