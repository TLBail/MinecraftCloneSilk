using System.Collections.Concurrent;
using System.Numerics;
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
    private readonly Block?[,,] blocks;

    public static readonly uint CHUNK_SIZE = 16;
    private static Shader cubeShader;
    private static Texture cubeTexture;
    

    private BufferObject<CubeVertex> Vbo;
    private VertexArrayObject<CubeVertex, uint> Vao;

    private World world;
    private readonly GL Gl;
    public bool displayable { get; private set; }
    private int nbVertex = 0;
    
    
    public Chunk(Vector3D<int> position, World world)
    {
        this.world = world;
        this.position = position;
        this.displayable = false;
        blocks = new Block?[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
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
        var block = blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] ?? new Block(blockPosition);
        return block;
    }

    public Block getBlock(int x, int y, int z)
    {
        if (x >= CHUNK_SIZE || x < 0 ||
            y >= CHUNK_SIZE || y < 0 ||
            z >= CHUNK_SIZE || z < 0) return world.getBlock(position + new Vector3D<int>(x, y, z));
        var block = blocks[x, y, z] ?? new Block(new Vector3D<int>(x, y, z)); 
        return block;
    }

    public Vector3D<int> getPosition() => position;
    
   

    public void addBlock(int x, int y, int z, string name)
    {
        blocks[x, y, z] = new Block(new Vector3D<int>(x, y, z), name, false);
        updateBlocksAround(x, y, z);
        if(displayable) updateChunkVertex();
    }



    public void removeBlock(Vector3D<int> localPosition)
    {
        blocks[localPosition.X, localPosition.Y, localPosition.Z] = null;
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
                    if(blocks[x, y, z] == null) continue;
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
        for (var x = 0; x < CHUNK_SIZE; x++)
        for (var z = 0; z < CHUNK_SIZE; z++)
            blocks[x, 0, z] = new Block(new Vector3D<int>(x, 0, z), "grass", false);

        Vector3D<int>[] positions =
        {
            new(0, 1, 0),
            new(2, 1, 0),
            new(4, 1, 0),
            new(0, 2, 0),
            new(0, 4, 0),
            new(0, 1, 2),
            new(0, 1, 4),
            new(2, 1, 2),
            new(4, 1, 4)
        };
        foreach (var position in positions)
            blocks[position.X, position.Y, position.Z] = new Block(position, "grass", false);

        blocks[6, 2, 0] = new Block(new Vector3D<int>(6, 2, 0), "dirt", false);
        blocks[6, 2, 2] = new Block(new Vector3D<int>(6, 2, 2), "cobblestone", false);
        blocks[6, 2, 4] = new Block(new Vector3D<int>(6, 2, 4), "foliage", true);
        blocks[6, 2, 6] = new Block(new Vector3D<int>(6, 2, 6), "grass", false);
        blocks[6, 2, 8] = new Block(new Vector3D<int>(6, 2, 8), "metal", false);
        blocks[6, 2, 10] = new Block(new Vector3D<int>(6, 2, 10), "oak", false);
        blocks[6, 2, 12] = new Block(new Vector3D<int>(6, 2, 12), "stone", false);
    }

   
    

    private Face[] getFaces(Block block)
    {
        if (block.transparent) {
            return new[] { Face.TOP, Face.BOTTOM, Face.LEFT, Face.RIGHT, Face.FRONT, Face.BACK };
        }

        var faces = new List<Face>();
        //X
        if (getBlock(block.position.X - 1, block.position.Y, block.position.Z).transparent) {
            faces.Add(Face.RIGHT);
        }

        if (getBlock(block.position.X + 1, block.position.Y, block.position.Z).transparent) {
            faces.Add(Face.LEFT);
        }

        //Y
        if (getBlock(block.position.X, block.position.Y - 1, block.position.Z).transparent) {
            faces.Add(Face.BOTTOM);
        }

        if (getBlock(block.position.X, block.position.Y  + 1, block.position.Z).transparent) {
            faces.Add(Face.TOP);
        }

        //Z
        if (getBlock(block.position.X, block.position.Y, block.position.Z - 1).transparent) {
            faces.Add(Face.BACK);
        }

        if (getBlock(block.position.X, block.position.Y, block.position.Z + 1).transparent) {
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

    private Vector3D<int> getChunkPosition(Vector3D<int> blockPosition) {
        return new Vector3D<int>(
            (int)((int)(MathF.Floor((float)blockPosition.X / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Y/ Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE),
            (int)((int)(MathF.Floor((float)blockPosition.Z / Chunk.CHUNK_SIZE)) * Chunk.CHUNK_SIZE)
        );

    }
}