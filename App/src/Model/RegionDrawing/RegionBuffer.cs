using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.OpenGL;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model;

public class RegionBuffer : IDisposable
{
    private BufferObject<CubeVertex> Vbo;
    private VertexArrayObject<CubeVertex, uint> Vao;
    private Texture cubeTexture;
    private GL gl;
    private int nbVertex = 0;

    public const int CHUNKS_PER_REGION = 16;

    private Chunk[] chunks;

    private int chunkCount = 0;


    public RegionBuffer(Texture cubeTexture, GL gl) {
        this.cubeTexture = cubeTexture;
        this.gl = gl;

        chunks = new Chunk[CHUNKS_PER_REGION];
    }

    public void addChunk(Chunk chunk) {
        chunks[chunkCount] = chunk;
        chunkCount++;
    }

    public void draw() {
        if (nbVertex == 0) return;
        Vao.Bind();
        Chunk.cubeShader.Use();
        cubeTexture.Bind();

        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }

    public void update() {
        
        
        nbVertex = 0;
        for (int i = 0; i < chunkCount; i++) {
            if(chunks[i].chunkState != ChunkState.DRAWABLE) continue;
            nbVertex += chunks[i].getVertices().Length;
        }

        Span<CubeVertex> vertices = stackalloc CubeVertex[nbVertex];
        int offset = 0;
        for (int i = 0; i < chunkCount; i++) {
            Chunk chunk = chunks[i];
            if(chunk.chunkState != ChunkState.DRAWABLE) continue;
            ReadOnlySpan<CubeVertex> verticesChunk1 = chunk.getVertices();
            verticesChunk1.CopyTo(vertices[offset..]);
            offset += verticesChunk1.Length;
        }

        Vbo?.Dispose();
        Vao?.Dispose();
        Vbo = new BufferObject<CubeVertex>(gl, vertices, BufferTargetARB.ArrayBuffer);
        Vao = new VertexArrayObject<CubeVertex, uint>(gl, Vbo);

        Vao.Bind();
        Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
    }

    public void Dispose() {
        Vbo.Dispose();
        Vao.Dispose();
    }

    public void addVertices(Chunk chunk, ReadOnlySpan<CubeVertex> vertices, int nbVertex) {
        this.nbVertex += nbVertex;
        Vbo.Bind();
        Vbo.sendData(vertices, 0);
    }


    public bool haveAvailableSpace() {
        return chunkCount >= CHUNKS_PER_REGION - 1;
    }

    public void removeChunk(Chunk chunk) {
        int indexOfChunk = Array.IndexOf(chunks, chunk);
        int offset = 0;
        for (int i = 0; i < CHUNKS_PER_REGION; i++) {
            if (i >= chunkCount - 1) {
                chunks[i] = null;
                continue;
            }
            if (i == indexOfChunk) {
                chunks[i] = chunks[i + 1];
                offset = 1;
            } else {
                chunks[i] = chunks[i + offset];
            }
        }
        chunkCount--;
    }
}