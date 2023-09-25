using System.Diagnostics;
using System.Numerics;
using MinecraftCloneSilk.Collision;
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
    const int SUPER_CHUNK_SIZE = Chunk.CHUNK_SIZE + 2;
    const int SUPER_CHUNK_NB_BLOCK = SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE;
    
    internal static BufferObject<Vector4> transparentBlocksBuffer; 
    internal static BufferObject<Vector4D<float>> textureCoordsBuffer;
    internal static ComputeShader computeShader;
    internal static BufferObject<Vector4D<float>> chunksPositionBuffer;
    internal static BufferObject<BlockData> blockDataBuffer;
    internal static BufferObject<CountCompute> countComputeBuffer;
    internal static BufferObject<CubeVertex> outputBuffer;



    private BufferObject<CubeVertex>? vbo;
    private VertexArrayObject<CubeVertex, uint>? vao;
    private Texture cubeTexture;
    private GL gl;
    internal static Shader? cubeShader;
    public int nbVertex { get; private set; }

    public const int CHUNKS_PER_REGION = 16;

    private Chunk?[] chunks;

    public int chunkCount { get; private set; }
    
    public bool haveDrawLastFrame { get; private set; }
    
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
        blockDataBuffer = new BufferObject<BlockData>(gl, CHUNKS_PER_REGION * SUPER_CHUNK_NB_BLOCK, BufferTargetARB.ShaderStorageBuffer,
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
        Vector4[] transparentVector = new Vector4[transparentBlocks.Length];
        for (int i = 0; i < transparentBlocks.Length; i++) {
            transparentVector[i] = new Vector4(transparentBlocks[i] ? 1.0f : 0.0f, 0.0f, 0.0f, 0.0f);
        }

        transparentBlocksBuffer = new BufferObject<Vector4>(gl, transparentVector, BufferTargetARB.UniformBuffer,
            BufferUsageARB.StaticDraw);

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
        textureCoordsBuffer = new BufferObject<Vector4D<float>>(gl, textureCoords, BufferTargetARB.UniformBuffer, BufferUsageARB.StaticDraw);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 3, textureCoordsBuffer.handle); 
        
        
        // init chunks position buffer
        chunksPositionBuffer = new BufferObject<Vector4D<float>>(gl, CHUNKS_PER_REGION, BufferTargetARB.UniformBuffer, BufferUsageARB.StreamDraw);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 6, chunksPositionBuffer.handle);
        
        
        // init output buffer
        
        const int nbFacePerBlock = 6;
        const int nbVertexPerFace = 6;
        int nbVertexMax = (int)(nbVertexPerFace * nbFacePerBlock * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE *
                                Chunk.CHUNK_SIZE);
        outputBuffer = new BufferObject<CubeVertex>(gl, CHUNKS_PER_REGION * nbVertexMax, BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StaticCopy);
    }


    public RegionBuffer(Texture cubeTexture, GL gl) {
        this.cubeTexture = cubeTexture;
        this.gl = gl;
        chunks = new Chunk?[CHUNKS_PER_REGION];
        
    }

    public void AddChunk(Chunk chunk) {
        Debug.Assert(!chunk.chunksNeighbors!.Any(chunk1 => chunk1.chunkState < ChunkState.BLOCKGENERATED));
        chunks[chunkCount] = chunk;
        chunkCount++;
    }

    public void Draw(Camera cam, Lighting lighting) {
        haveDrawLastFrame = false;
        if (nbVertex == 0) return;
        if (!RegionInCameraView(cam)) return;
        
        vao!.Bind();
        cubeShader!.Use();
        cubeShader.SetUniform("ambientStrength", lighting.lightLevel);
        cubeTexture.Bind();

        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbVertex);
        haveDrawLastFrame = true;
    }

    private bool RegionInCameraView(Camera cam) {
        Frustrum frustrum = cam.GetFrustrum();
        for (int i = 0; i < chunkCount; i++) {
            Chunk chunk = chunks[i]!;
            AABBCube aabbCube = chunk.GetAABBCube();
            if (aabbCube.IsInFrustrum(frustrum)) return true;
        }
        return false;
    }

    public unsafe void Update() {
        if (chunkCount == 0) {
            vbo?.Dispose();
            vao?.Dispose();
            nbVertex = 0;
            return;
        }
        
        
        computeShader.Use();
        //reset countComputeBuffer
        CountCompute[] resetCountCompute = new CountCompute[1];
        resetCountCompute[0].vertexCount = 0;
        countComputeBuffer.SendData(resetCountCompute, 0);
        
        //create super block data
        
        Span<BlockData> blockDatasSpan = stackalloc BlockData[chunkCount * SUPER_CHUNK_NB_BLOCK];
        
        CreateSuperChunk(blockDatasSpan);
        
        gl.BindBufferRange(GLEnum.ShaderStorageBuffer, 2, blockDataBuffer.handle, 0, (nuint)(sizeof(BlockData) * blockDatasSpan.Length));
        blockDataBuffer.SendData(blockDatasSpan, 0);
        
        // update output buffer to vbo
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 1, outputBuffer.handle);
        
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
        
        vbo?.Dispose();
        vao?.Dispose();
        if(nbVertex == 0) return;
        
       
    
        
        vbo = new BufferObject<CubeVertex>(gl, nbVertex, BufferTargetARB.ArrayBuffer);
        // copy output buffer to vbo
        outputBuffer.Bind(BufferTargetARB.CopyReadBuffer);
        vbo.Bind(BufferTargetARB.CopyWriteBuffer);
        gl.CopyBufferSubData(CopyBufferSubDataTarget.CopyReadBuffer, CopyBufferSubDataTarget.CopyWriteBuffer, 0, 0,(uint)( nbVertex * sizeof(CubeVertex)));
        
        vao = new VertexArrayObject<CubeVertex, uint>(gl, vbo);
        vao.Bind();
        vao.VertexAttributePointer(0, 4, VertexAttribPointerType.Float, "position");
        vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
        vao.VertexAttributeIPointer(2, 1, VertexAttribIType.Int, "ambientOcclusion");
        vao.VertexAttributeIPointer(3, 1, VertexAttribIType.Int, "lightLevel");
    }
    
    
    private unsafe void CreateSuperChunk(Span<BlockData> superChunk) {
        //Todo rajouter les chunks dans toutes les diagonales donc 9 * 3 = 27 chunks au total comme sa les ambiant occlusion seront correct
        int offset = 0;
        for (int i = 0; i < chunkCount; i++) {
            Chunk chunk = chunks[i]!;
            if(chunk.chunkState != ChunkState.DRAWABLE) continue;
           
            // inner chunk
            fixed(BlockData* blockDataPtr = chunk.blocks) {
                Span<BlockData> blockDataSpan = new Span<BlockData>(blockDataPtr, Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE);
                for (int x = 0; x < 16; x++) {
                    for (int y = 0; y < 16; y++) {
                        int offsetSuperChunk = ((x + 1) * 18 * 18 + (y + 1) * 18 + 1) + offset;
                        int offsetBlocks = x * 16 * 16 + y * 16;
                        blockDataSpan.Slice(offsetBlocks, 16).CopyTo(superChunk[offsetSuperChunk..]);
                    }
                }
            }
            // left chunk
            Chunk leftNeighbor = chunk.chunksNeighbors![(int)Face.LEFT];
            Debug.Assert(leftNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            for(int y = 0; y < 16; y++) {
                for(int z = 0; z < 16; z++) {
                    int superChunkIndex = 0 * 18 * 18 + (y + 1) * 18 + (z + 1);
                    superChunk[offset + superChunkIndex] = leftNeighbor.blocks[15,y,z];
                }
            }
            
            // right chunk
            Chunk rightNeighbor = chunk.chunksNeighbors![(int)Face.RIGHT];
            Debug.Assert(rightNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            for(int y = 0; y < 16; y++) {
                for(int z = 0; z < 16; z++) {
                    int superChunkIndex = 17 * 18 * 18 + (y + 1) * 18 + (z + 1);
                    superChunk[offset + superChunkIndex] = rightNeighbor.blocks[0,y,z];
                }
            }
            
            // top chunk
            Chunk topNeighbor = chunk.chunksNeighbors![(int)Face.TOP];
            Debug.Assert(topNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            for(int x = 0; x < 16; x++) {
                for(int z = 0; z < 16; z++) {
                    int superChunkIndex = (x + 1) * 18 * 18 + 17 * 18 + (z + 1);
                    superChunk[offset + superChunkIndex] = topNeighbor.blocks[x,0,z];
                }
            }
            
            // bottom chunk
            Chunk bottomNeighbor = chunk.chunksNeighbors![(int)Face.BOTTOM];
            Debug.Assert(bottomNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            for(int x = 0; x < 16; x++) {
                for(int z = 0; z < 16; z++) {
                    int superChunkIndex = (x + 1) * 18 * 18 + 0 * 18 + (z + 1);
                    superChunk[offset + superChunkIndex] = bottomNeighbor.blocks[x,15,z];
                }
            }
            
            // front chunk
            Chunk frontNeighbor = chunk.chunksNeighbors![(int)Face.FRONT];
            Debug.Assert(frontNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            for(int x = 0; x < 16; x++) {
                for(int y = 0; y < 16; y++) {
                    int superChunkIndex = (x + 1) * 18 * 18 + (y + 1) * 18 + 17;
                    superChunk[offset + superChunkIndex] = frontNeighbor.blocks[x,y,0];
                }
            }
            
            // back chunk
            Chunk backNeighbor = chunk.chunksNeighbors![(int)Face.BACK];
            Debug.Assert(backNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            for(int x = 0; x < 16; x++) {
                for(int y = 0; y < 16; y++) {
                    int superChunkIndex = (x + 1) * 18 * 18 + (y + 1) * 18 + 0;
                    superChunk[offset + superChunkIndex] = backNeighbor.blocks[x,y,15];
                }
            }
            offset += SUPER_CHUNK_NB_BLOCK;
        }
    }

    public void Dispose() {
        vbo?.Dispose();
        vao?.Dispose();
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