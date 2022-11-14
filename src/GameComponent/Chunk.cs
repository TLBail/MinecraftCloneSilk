using System.Collections.Concurrent;
using System.Numerics;
using DotnetNoise;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.UI;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.GameComponent;

public class Chunk : IDisposable
{
    private readonly Vector3D<int> position;
    private readonly BlockData[,,] blocks;

    public static readonly uint CHUNK_SIZE = 16;
    private static Shader cubeShader;
    private static Texture cubeTexture;
    

    private BufferObject<CubeVertex> Vbo;
    private VertexArrayObject<CubeVertex, uint> Vao;

    private World world;
    private readonly GL Gl;
    public bool displayable { get; private set; }
    private int nbVertex = 0;

    private List<DebugRay> debugRays = new List<DebugRay>();
    private bool debugMode = false;


    private WorldGeneration worldGeneration;

    private static BlockFactory blockFactory;
    
    public Chunk(Vector3D<int> position, World world)
    {
        this.world = world;
        this.position = position;
        this.displayable = false;
        if(blockFactory == null) blockFactory = BlockFactory.getInstance();
        this.worldGeneration = world.worldGeneration;
        blocks = new BlockData[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        Gl = Game.getInstance().getGL();
        initStaticMembers();
        generateTerrain();
    }


    public void displayChunk()
    {
        if(displayable) return;
        displayable = true;
        setOpenGl();
        List<CubeVertex> cubeVertices = getCubeVertices();
        if (cubeVertices.Count == 0) {
            return;
        } 
        sendCubeVertices(cubeVertices);
    }

    


    public Block getBlock(Vector3D<int> blockPosition)
    {
        if (blockPosition.X >= CHUNK_SIZE || blockPosition.X < 0 ||
            blockPosition.Y >= CHUNK_SIZE || blockPosition.Y < 0 ||
            blockPosition.Z >= CHUNK_SIZE || blockPosition.Z < 0) return world.getBlock(position + blockPosition);
        var blockData = blocks[blockPosition.X, blockPosition.Y, blockPosition.Z];
        return blockFactory.buildFromBlockData(blockPosition, blockData);
    }

    public Block getBlock(int x, int y, int z)
    {
        if (x >= CHUNK_SIZE || x < 0 ||
            y >= CHUNK_SIZE || y < 0 ||
            z >= CHUNK_SIZE || z < 0) return world.getBlock(position + new Vector3D<int>(x, y, z));
        var blockData = blocks[x, y, z];
        return blockFactory.buildFromBlockData(new Vector3D<int>(x, y, z), blockData);
    }

    public Vector3D<int> getPosition() => position;



    public void setBlock(Vector3D<int> blockPosition, string name)
    {
        setBlock(blockPosition.X, blockPosition.Y, blockPosition.Z, name);
    }

    public void setBlock(int x, int y, int z, string name)
    {
        blocks[x, y, z].id = name.GetHashCode();
        updateBlocksAround(x, y, z);
        if(displayable) updateChunkVertex();
    }



    public void removeBlock(Vector3D<int> localPosition)
    {
        blocks[localPosition.X, localPosition.Y, localPosition.Z].id = 0;
        //only if the cube is visible => faces not empty
        updateBlocksAround(localPosition.X, localPosition.Y, localPosition.Z);
        
        if(displayable) updateChunkVertex();
    }
    
     private void setOpenGl()
    {
        const int nbFacePerBlock = 6;
        const int nbVertexPerFace = 6;
        int nbVertexMax = (int)(nbVertexPerFace * nbFacePerBlock * CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE);
        Vbo = new BufferObject<CubeVertex>(Gl, nbVertexMax, BufferTargetARB.ArrayBuffer);
        Vao = new VertexArrayObject<CubeVertex, uint>(Gl, Vbo);
        
        Vao.Bind();
        Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
    }


    
    public void updateChunkVertex()
    {
        if(!displayable) return;
        List<CubeVertex> vertices = getCubeVertices();
        nbVertex = vertices.Count;
        sendCubeVertices(vertices);
    }

    
    private List<CubeVertex> getCubeVertices() {
        var listVertices = new List<CubeVertex>();
        for (int x = 0; x < CHUNK_SIZE; x++) {
            for (int y = 0; y < CHUNK_SIZE; y++) {
                for (int z = 0; z < CHUNK_SIZE; z++) {
                    BlockData block = blocks[x, y, z];
                    if(block.id == 0  || blockFactory.getBlockNameById(block.id).Equals(BlockFactory.AIR_BLOCK)) continue;
                    List<Face> faces = getFaces(x ,y, z);
                    if (faces.Count > 0) {
                        listVertices.AddRange(
                            TextureBlock.get(blockFactory.getBlockNameById(block.id)).getCubeVertices(faces.ToArray(),
                                new Vector3D<float>(x, y, z)));
                    }
                }
            }
        }
        nbVertex = listVertices.Count;
        return listVertices;
    }

    
    private void sendCubeVertices(List<CubeVertex> vertices) {
        Vbo.Bind();
        Vbo.sendData(vertices.ToArray(), 0);
    }
    

    public void debug(bool? setDebug = null)
    {
        debugMode = setDebug ?? !debugMode;

        
        if (!debugMode) {
            foreach (var debugRay in debugRays) {
                debugRay.remove();
            }
            debugRays.Clear();
        }
        else {
            //base
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y -0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + Chunk.CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f , position.Z +  Chunk.CHUNK_SIZE - 0.5f)));

            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y  - 0.5f, position.Z +  Chunk.CHUNK_SIZE- 0.5f)));
            
            //top base

            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + Chunk.CHUNK_SIZE - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X - 0.5f , position.Y + CHUNK_SIZE - 0.5f , position.Z +  Chunk.CHUNK_SIZE - 0.5f)));

            
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y +CHUNK_SIZE - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y + CHUNK_SIZE - 0.5f, position.Z +  Chunk.CHUNK_SIZE - 0.5f)));
            
            //between
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X  - 0.5f, position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z- 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y - 0.5f, position.Z + CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X + CHUNK_SIZE - 0.5f, position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z+ CHUNK_SIZE - 0.5f)));
            debugRays.Add( new DebugRay(new Vector3D<float>(position.X - 0.5f, position.Y - 0.5f, position.Z + CHUNK_SIZE - 0.5f),
                new Vector3D<float>(position.X - 0.5f , position.Y +  Chunk.CHUNK_SIZE - 0.5f, position.Z+ CHUNK_SIZE - 0.5f)));

            
            
        }

    }

    private void initStaticMembers()
    {
        if (cubeShader == null) {
            cubeShader = new Shader(Gl, "./Shader/3dPosOneTextUni/VertexShader.hlsl",
                "./Shader/3dPosOneTextUni/FragmentShader.hlsl");
            cubeShader.Use();
            cubeShader.SetUniform("texture1", 0);
        }

        if (cubeTexture == null) {
            cubeTexture = new Texture(Gl, "./Assets/spriteSheet.png");
        }
    }

    private void generateTerrain()
    {
        worldGeneration.generateTerrain(position, blocks);
    }

   
    

    private List<Face> getFaces(int x, int y, int z)
    {

        var faces = new List<Face>();
        //X
        if (isBlockTransparent(x - 1, y, z)) {
            faces.Add(Face.RIGHT);
        }

        if (isBlockTransparent(x + 1, y, z)) {
            faces.Add(Face.LEFT);
        }

        //y
        if (isBlockTransparent(x, y - 1, z)) {
            faces.Add(Face.BOTTOM);
        }

        if (isBlockTransparent(x, y  + 1, z)) {
            faces.Add(Face.TOP);
        }

        //z
        if (isBlockTransparent(x, y, z - 1)) {
            faces.Add(Face.BACK);
        }

        if (isBlockTransparent(x, y, z + 1)) {
            faces.Add(Face.FRONT);
        }


        return faces;
    }
    

    public void Update(double deltaTime)
    {
    }


    public void Draw(GL Gl, double deltaTime)
    {
        if(!displayable) return;
        if(nbVertex == 0) return;
        Vao.Bind();
        cubeShader.Use();
        cubeTexture.Bind();

        var model = Matrix4x4.Identity;
        model = Matrix4x4.CreateTranslation(new Vector3(position.X, position.Y, position.Z));
        cubeShader.SetUniform("model", model);


        Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }
    
    
    private void updateBlocksAround(int x, int y, int z)
    {   
        if(x == 0) world.updateChunkVertex(getChunkPosition(position + new Vector3D<int>(x - 1, y, z)));
        if(x == CHUNK_SIZE- 1) world.updateChunkVertex(getChunkPosition(position + new Vector3D<int>(x + 1, y ,z)));
        
        if(y == 0) world.updateChunkVertex(getChunkPosition(position + new Vector3D<int>(x, y  - 1, z)));
        if(y == CHUNK_SIZE - 1) world.updateChunkVertex(getChunkPosition(position + new Vector3D<int>(x , y + 1,z)));

        if(z == 0) world.updateChunkVertex(getChunkPosition(position + new Vector3D<int>(x , y, z - 1)));
        if(z == CHUNK_SIZE - 1) world.updateChunkVertex(getChunkPosition(position + new Vector3D<int>(x , y ,z + 1)));

    }

    private bool isBlockTransparent(int x, int y, int z) {
        BlockData blockData;
        if (x >= CHUNK_SIZE || x < 0 ||
            y >= CHUNK_SIZE || y < 0 ||
            z >= CHUNK_SIZE || z < 0) {
            blockData = world.getBlockData(position + new Vector3D<int>(x, y, z));
        } else {
            blockData = blocks[x, y, z];
        }
        return blockData.id == 0 || blockFactory.isBlockTransparent(blockData);
    }

    private Vector3D<int> getChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)(MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Y/ Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE)
        );

    }

    protected bool disposed = false;

    public void Dispose() {
        Dispose(true);  
        GC.SuppressFinalize(this);

    } 
    ~Chunk() => Dispose(false);

    protected virtual void Dispose(bool disposing) {
        if (!disposed) {

            if (disposing) {
                Vao?.Dispose();
                Vbo?.Dispose();
            }
            disposed = true;
        }


    }

    public BlockData getBlockData(Vector3D<int> localPosition) {
        if (localPosition.X < 0 || localPosition.X >= CHUNK_SIZE ||
            localPosition.Y < 0 || localPosition.Y >= CHUNK_SIZE ||
            localPosition.Z < 0 || localPosition.Z >= CHUNK_SIZE) return world.getBlockData(position + localPosition);
        return blocks[localPosition.X, localPosition.Y, localPosition.Z];
    }
}