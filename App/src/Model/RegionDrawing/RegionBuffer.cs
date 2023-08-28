using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.RegionDrawing;

public class RegionBuffer : IDisposable
{
    
    internal static BufferObject<bool> transparentBlocksBuffer; 
    internal static BufferObject<Vector4D<float>> textureCoordsBuffer;
    internal static ComputeShader computeShader;
    internal static BufferObject<Vector4D<float>> chunksPositionBuffer;
    internal static BufferObject<BlockData> blockDataBuffer;
    internal static BufferObject<CountCompute> countComputeBuffer;



    private BufferObject<CubeVertex>? vbo;
    private VertexArrayObject<CubeVertex, uint>? vao;
    private Texture cubeTexture;
    private GL gl;
    internal static Shader? cubeShader;
    private int nbVertex = 0;

    public const int CHUNKS_PER_REGION = 16;

    private Chunk?[] chunks;

    private int chunkCount = 0;
    public static void InitComputeShader(GL gl, BlockFactory blockFactory) {
        // compute shadere 
        computeShader = new ComputeShader(gl, "Shader/computeChunk.glsl");
        computeShader.Use();

        //init bindings
        uint inputBufferIndex =  gl.GetProgramResourceIndex(computeShader.handle, ProgramInterface.ShaderStorageBlock, "chunkBlocks");
        gl.ShaderStorageBlockBinding(computeShader.handle, inputBufferIndex, 2);
        uint ouputBufferIndex =  gl.GetProgramResourceIndex(computeShader.handle, ProgramInterface.ShaderStorageBlock, "outputBuffer");
        gl.ShaderStorageBlockBinding(computeShader.handle, ouputBufferIndex, 1);
        
        //init block data buffer
        blockDataBuffer = new BufferObject<BlockData>(gl, CHUNKS_PER_REGION * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE, BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StreamDraw);
        
        //init countComputeBuffer
        countComputeBuffer = new BufferObject<CountCompute>(gl, 1, BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StaticRead);


        // init transparent blocks buffer
        int maxIndex = 0;
        foreach (KeyValuePair<int,Block> keyValuePair in blockFactory.blocksReadOnly) {
            if (keyValuePair.Key > maxIndex) maxIndex = keyValuePair.Key;
        }
        bool[] transparentBlocks = new bool[maxIndex + 1];
        foreach (KeyValuePair<int,Block> keyValuePair in blockFactory.blocksReadOnly) {
            transparentBlocks[keyValuePair.Key] = keyValuePair.Value.transparent;
        }

        transparentBlocksBuffer = new BufferObject<bool>(gl, transparentBlocks, BufferTargetARB.UniformBuffer,
            BufferUsageARB.StaticRead);

        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 4, transparentBlocksBuffer.handle);

        // init texture coords buffer
        Vector4D<float>[] textureCoords = new Vector4D<float>[(maxIndex + 1) * 6];
        foreach (KeyValuePair<int,Block> keyValuePair in blockFactory.blocksReadOnly) {
            if(keyValuePair.Value.textureBlock is  null) continue;
            foreach (Face face in Enum.GetValues(typeof(Face))) {
                int[] coords = keyValuePair.Value.textureBlock.blockJson.texture[face];
                textureCoords[(keyValuePair.Key * 6) +  (int)face] = new Vector4D<float>(coords[0], coords[1], 0.0f, 0.0f);
            }
        }
        textureCoordsBuffer = new BufferObject<Vector4D<float>>(gl, textureCoords, BufferTargetARB.UniformBuffer, BufferUsageARB.StaticRead);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 3, textureCoordsBuffer.handle); 
        
        
        // init chunks position buffer
        chunksPositionBuffer = new BufferObject<Vector4D<float>>(gl, CHUNKS_PER_REGION, BufferTargetARB.UniformBuffer, BufferUsageARB.StreamDraw);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 0, chunksPositionBuffer.handle);
    }


    public RegionBuffer(Texture cubeTexture, GL gl) {
        this.cubeTexture = cubeTexture;
        this.gl = gl;
        chunks = new Chunk?[CHUNKS_PER_REGION];
        
        const int nbFacePerBlock = 6;
        const int nbVertexPerFace = 6;
        int nbVertexMax = (int)(nbVertexPerFace * nbFacePerBlock * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE *
                                Chunk.CHUNK_SIZE);
        vbo = new BufferObject<CubeVertex>(gl, nbVertexMax, BufferTargetARB.ArrayBuffer);
        vao = new VertexArrayObject<CubeVertex, uint>(gl, vbo);
        vao.Bind();
        vao.VertexAttributePointer(0, 4, VertexAttribPointerType.Float, "position");
        vao.VertexAttributePointer(1, 4, VertexAttribPointerType.Float, "texCoords");
    }

    public void AddChunk(Chunk chunk) {
        chunks[chunkCount] = chunk;
        chunkCount++;
    }

    public void Draw() {
        if (nbVertex == 0) return;
        vao!.Bind();
        cubeShader!.Use();
        cubeTexture.Bind();

        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
    }

    public unsafe void Update() {
        computeShader.Use();
        //reset countComputeBuffer
        CountCompute[] resetCountCompute = new CountCompute[1];
        resetCountCompute[0].vertexCount = 0;
        countComputeBuffer.SendData(resetCountCompute, 0);
        
        //Todo stackalloc ?
        BlockData[] blockDatas = new BlockData[chunkCount * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE];
        Span<BlockData> blockDatasSpan = blockDatas;
        
        // copy Blockdata
        int sizeOfChunksBlock = Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * sizeof(BlockData);
        int offset = 0;
        for (int i = 0; i < chunkCount; i++) {
            Chunk chunk = chunks[i]!;
            if(chunk.chunkState != ChunkState.DRAWABLE) continue;
            fixed(BlockData* blockDataPtr = chunk.blocks) {
                Span<BlockData> blockDataSpan = new Span<BlockData>(blockDataPtr, Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE);
                blockDataSpan.CopyTo(blockDatasSpan[offset..]);
            }
            offset += sizeOfChunksBlock;
        }
        gl.BindBufferRange(GLEnum.ShaderStorageBuffer, 2, blockDataBuffer.handle, 0, (nuint)(sizeof(BlockData) * blockDatas.Length));
        blockDataBuffer.SendData(blockDatas, 0);
        
        // update output buffer to vbo
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 1, vbo.handle);
        
        // update countComputeBuffer
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 5, countComputeBuffer.handle);
        
        
        transparentBlocksBuffer.Bind(BufferTargetARB.UniformBuffer);
        textureCoordsBuffer.Bind(BufferTargetARB.UniformBuffer);
        
        
        // update chunks position buffer
        Vector4D<float>[] chunksPosition = new Vector4D<float>[CHUNKS_PER_REGION];
        for (int i = 0; i < chunkCount; i++) {
            chunksPosition[i] = new Vector4D<float>(chunks[i]!.position.X, chunks[i]!.position.Y, chunks[i]!.position.Z, 0.0f);
        }
        chunksPositionBuffer.SendData(chunksPosition, 0);
        
        // compute
        gl.DispatchCompute((uint)(4 * chunkCount),4,4);
        gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);
        CountCompute countCompute = countComputeBuffer.GetData();
        nbVertex = countCompute.vertexCount;
    }

    public void Dispose() {
        vbo?.Dispose();
        vao?.Dispose();
    }

    public void AddVertices(Chunk chunk, ReadOnlySpan<CubeVertex> vertices, int nbVertex) {
        this.nbVertex += nbVertex;
        vbo!.Bind(BufferTargetARB.ArrayBuffer);
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
    
    internal struct CountCompute
    {
        public int vertexCount{get;set;}
        public int blockCount{get;set;}
        public int firstIndex{get;set;}
        public int vertexIndex{get;set;}

        public override string ToString() {
            return $"vertexCount : {vertexCount} blockCount : {blockCount} firstIndex : {firstIndex} vertexIndex : {vertexIndex}";
        }
    }
}