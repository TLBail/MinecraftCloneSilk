using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.OpenGL;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.RegionDrawing;

public class RegionBuffer : IDisposable
{
    private BufferObject<CubeVertex>? vbo;
    private VertexArrayObject<CubeVertex, uint>? vao;
    private Texture cubeTexture;
    private GL gl;
    private int nbVertex = 0;

    public const int CHUNKS_PER_REGION = 16;

    private Chunk?[] chunks;

    private int chunkCount = 0;


    public RegionBuffer(Texture cubeTexture, GL gl) {
        this.cubeTexture = cubeTexture;
        this.gl = gl;
        chunks = new Chunk?[CHUNKS_PER_REGION];
    }

    public void AddChunk(Chunk chunk) {
        chunks[chunkCount] = chunk;
        chunkCount++;
    }

    public void Draw() {
        if (nbVertex == 0) return;
        vao!.Bind();
        Chunk.cubeShader!.Use();
        cubeTexture.Bind();

        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }

    public void Update() {
        
        
        nbVertex = 0;
        for (int i = 0; i < chunkCount; i++) {
            if(chunks[i]!.chunkState != ChunkState.DRAWABLE) continue;
            nbVertex += chunks[i]!.GetVertices().Length;
        }

        Span<CubeVertex> vertices = stackalloc CubeVertex[nbVertex];
        int offset = 0;
        for (int i = 0; i < chunkCount; i++) {
            Chunk chunk = chunks[i]!;
            if(chunk.chunkState != ChunkState.DRAWABLE) continue;
            ReadOnlySpan<CubeVertex> verticesChunk1 = chunk.GetVertices();
            verticesChunk1.CopyTo(vertices[offset..]);
            offset += verticesChunk1.Length;
        }

        vbo?.Dispose();
        vao?.Dispose();
        vbo = new BufferObject<CubeVertex>(gl, vertices, BufferTargetARB.ArrayBuffer);
        vao = new VertexArrayObject<CubeVertex, uint>(gl, vbo);

        vao.Bind();
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
    }

    public void Dispose() {
        vbo?.Dispose();
        vao?.Dispose();
    }

    public void AddVertices(Chunk chunk, ReadOnlySpan<CubeVertex> vertices, int nbVertex) {
        this.nbVertex += nbVertex;
        vbo!.Bind();
        vbo!.SendData(vertices, 0);
    }


    public bool HaveAvailableSpace() {
        return chunkCount >= CHUNKS_PER_REGION - 1;
    }

    public void RemoveChunk(Chunk chunk) {
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