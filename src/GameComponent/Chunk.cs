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

public class Chunk
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

    public const int seed = 1234543;

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
        updateChunkVertex();
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
        var listVertices = new List<CubeVertex>();
        for (int x = 0; x < CHUNK_SIZE; x++) {
            for (int y = 0; y < CHUNK_SIZE; y++) {
                for (int z = 0; z < CHUNK_SIZE; z++) {
                    if(blocks[x, y, z].id == 0  || blockFactory.getBlockNameById(blocks[x ,y, z].id).Equals(BlockFactory.AIR_BLOCK)) continue;
                    Block block = getBlock(x, y, z);
                    Face[] faces = getFaces(block);
                    if (!block.airBlock && faces.Length > 0) {
                        listVertices.AddRange(
                            TextureBlock.get(block.name).getCubeVertices(faces,
                                new Vector3D<float>(block.position.X, block.position.Y, block.position.Z)));
                    }
                }
            }
        }
        nbVertex = listVertices.Count;
        Vbo.Bind();
        Vbo.sendData(listVertices.ToArray(), 0);
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

   
    

    private Face[] getFaces(Block block)
    {
        if (block.transparent) {
            return new[] { Face.TOP, Face.BOTTOM, Face.LEFT, Face.RIGHT, Face.FRONT, Face.BACK };
        }

        var faces = new List<Face>();
        //X
        if (isBlockTransparent(block.position.X - 1, block.position.Y, block.position.Z)) {
            faces.Add(Face.RIGHT);
        }

        if (isBlockTransparent(block.position.X + 1, block.position.Y, block.position.Z)) {
            faces.Add(Face.LEFT);
        }

        //Y
        if (isBlockTransparent(block.position.X, block.position.Y - 1, block.position.Z)) {
            faces.Add(Face.BOTTOM);
        }

        if (isBlockTransparent(block.position.X, block.position.Y  + 1, block.position.Z)) {
            faces.Add(Face.TOP);
        }

        //Z
        if (isBlockTransparent(block.position.X, block.position.Y, block.position.Z - 1)) {
            faces.Add(Face.BACK);
        }

        if (isBlockTransparent(block.position.X, block.position.Y, block.position.Z + 1)) {
            faces.Add(Face.FRONT);
        }


        return faces.ToArray();
    }
    

    public void Update(double deltaTime)
    {
    }


    public void Draw(GL Gl, double deltaTime)
    {
        if(!displayable) return;
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

    private bool isBlockTransparent(int x, int y, int z)
    {
        if (x >= CHUNK_SIZE || x < 0 ||
            y >= CHUNK_SIZE || y < 0 ||
            z >= CHUNK_SIZE || z < 0) return world.getBlock(position + new Vector3D<int>(x, y, z)).transparent;

        BlockData blockData = blocks[x, y, z];
        if (blockData.id == 0 || blockFactory.isBlockTransparent(blockData)) {
            return true;
        }
        return getBlock(x, y, z).transparent;
    }

    private Vector3D<int> getChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)(MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Y/ Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE)
        );

    }
}