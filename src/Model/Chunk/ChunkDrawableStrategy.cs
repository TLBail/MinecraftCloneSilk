using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Model.Chunk;

public class ChunkDrawableStrategy : ChunkStrategy
{
    private bool displayable;
    private int nbVertex = 0;
    
    public ChunkDrawableStrategy(Chunk chunk, World world) : base(chunk, world) {
        if (chunk.chunkState != ChunkState.GENERATEDTERRAINANDSTRUCTURES) {
            chunk.chunkStrategy = new ChunkTerrainAndStructuresStrategy(chunk, world);
        }
        displayChunk();
        chunk.chunkState = ChunkState.DRAWABLE;
    }
    
    public override ChunkState getChunkStateOfStrategy() => ChunkState.DRAWABLE;


    public override void updateChunkVertex() {
        if(!displayable) return;
        List<CubeVertex> vertices = getCubeVertices();
        nbVertex = vertices.Count;
        sendCubeVertices(vertices);
    }

    public override void draw(GL gl, double deltaTime) {
        if(!displayable) return;
        if(nbVertex == 0) return;
        chunk.Vao.Bind();
        Chunk.cubeShader.Use();
        Chunk.cubeTexture.Bind();

        var model = Matrix4x4.Identity;
        model = Matrix4x4.CreateTranslation(new Vector3(chunk.position.X, chunk.position.Y, chunk.position.Z));
        Chunk.cubeShader.SetUniform("model", model);


        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }

    public override void setBlock(int x, int y, int z, string name) {
        chunk.blocks[x, y, z].id = name.GetHashCode();
        updateBlocksAround(x, y, z);
        if(displayable) updateChunkVertex();
    }


    private void displayChunk()
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
    
    
    private void setOpenGl()
    {
        const int nbFacePerBlock = 6;
        const int nbVertexPerFace = 6;
        int nbVertexMax = (int)(nbVertexPerFace * nbFacePerBlock * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE);
        chunk.Vbo = new BufferObject<CubeVertex>(chunk.Gl, nbVertexMax, BufferTargetARB.ArrayBuffer);
        chunk.Vao = new VertexArrayObject<CubeVertex, uint>(chunk.Gl, chunk.Vbo);
        
        chunk.Vao.Bind();
        chunk.Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        chunk.Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
    }
    
    private void sendCubeVertices(List<CubeVertex> vertices) {
        chunk.Vbo.Bind();
        chunk.Vbo.sendData(vertices.ToArray(), 0);
    }
    
    private List<CubeVertex> getCubeVertices() {
        var listVertices = new List<CubeVertex>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    BlockData block = chunk.blocks[x, y, z];
                    if(block.id == 0  || Chunk.blockFactory.getBlockNameById(block.id).Equals(BlockFactory.AIR_BLOCK)) continue;
                    List<Face> faces = getFaces(x ,y, z);
                    if (faces.Count > 0) {
                        listVertices.AddRange(Chunk.blockFactory.blocksReadOnly[block.id].textureBlock
                            .getCubeVertices(faces.ToArray(), new Vector3D<float>(x, y, z)));
                    }
                }
            }
        }
        nbVertex = listVertices.Count;
        return listVertices;
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
    
    private bool isBlockTransparent(int x, int y, int z) {
        BlockData blockData;
        if (x >= Chunk.CHUNK_SIZE || x < 0 ||
            y >= Chunk.CHUNK_SIZE || y < 0 ||
            z >= Chunk.CHUNK_SIZE || z < 0) {
            blockData = world.getBlockData(chunk.position + new Vector3D<int>(x, y, z));
        } else {
            blockData = chunk.blocks[x, y, z];
        }
        return blockData.id == 0 || Chunk.blockFactory.isBlockTransparent(blockData);
    }

    
    internal void updateBlocksAround(int x, int y, int z)
    {   
        if(x == 0) world.updateChunkVertex(chunk.getChunkPosition(chunk.position + new Vector3D<int>(x - 1, y, z)));
        if(x == Chunk.CHUNK_SIZE- 1) world.updateChunkVertex(chunk.getChunkPosition(chunk.position + new Vector3D<int>(x + 1, y ,z)));
        
        if(y == 0) world.updateChunkVertex(chunk.getChunkPosition(chunk.position + new Vector3D<int>(x, y  - 1, z)));
        if(y == Chunk.CHUNK_SIZE - 1) world.updateChunkVertex(chunk.getChunkPosition(chunk.position + new Vector3D<int>(x , y + 1,z)));

        if(z == 0) world.updateChunkVertex(chunk.getChunkPosition(chunk.position + new Vector3D<int>(x , y, z - 1)));
        if(z == Chunk.CHUNK_SIZE - 1) world.updateChunkVertex(chunk.getChunkPosition(chunk.position + new Vector3D<int>(x , y ,z + 1)));

    }
}