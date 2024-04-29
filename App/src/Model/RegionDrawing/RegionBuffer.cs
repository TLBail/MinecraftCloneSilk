using System.Diagnostics;
using System.Numerics;
using MinecraftCloneSilk.Collision;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.Model.Lighting;
using MinecraftCloneSilk.Model.NChunk;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Buffer = System.Buffer;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.Model.RegionDrawing;

public class RegionBuffer : IDisposable
{
    const int NB_FACE_PER_BLOCK = 6;
    const int NB_VERTEX_PER_FACE = 4;
    public const int SUPER_CHUNK_SIZE = Chunk.CHUNK_SIZE + 2;
    const int SUPER_CHUNK_NB_BLOCK = SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE;
    const int NB_SUB_BLOCK_POSITION = 65535;

    private static BufferObject<Vector4> transparentBlocksBuffer = null!;
    private static BufferObject<FacesTextureCoords> textureCoordsBuffer = null!;
    private static ComputeShader computeShader = null!;
    private static BufferObject<Vector4D<float>> chunksPositionBuffer = null!;
    private static BufferObject<BlockData> blockDataBuffer = null!;
    private static BufferObject<CountCompute> countComputeBuffer = null!;
    private static BufferObject<CubeVertex> outputBuffer = null!;
    private static BufferObject<CubeVertex> waterOutputBuffer = null!;
    private static BufferObject<Vector4D<float>> subBlockPositionsBuffer = null!;



    private BufferObject<CubeVertex>? vboBlock;
    private BufferObject<CubeVertex>? vboWater;
    private VertexArrayObject<CubeVertex, uint>? vaoBlock;
    private VertexArrayObject<CubeVertex, uint>? vaoWater;
    private readonly Texture cubeTexture;
    private GL gl;
    internal static Shader? cubeShader;
    private Vector4D<float>[] chunksPositionComputed;
    public int nbBlockVertex { get; private set; }
    public int nbWaterVertex { get; private set; }

    public const int CHUNKS_PER_REGION = 16;

    private Chunk?[] chunks;

    public int chunkCount { get; private set; }
    
    public bool haveDrawLastFrame { get; private set; }
    
    public static void InitComputeShader(GL gl, BlockFactory blockFactory) {
        // get max work group size
        int[] maxWorkGroupSize = new int[3];
        gl.GetInteger(GLEnum.MaxComputeWorkGroupSize, 0, out maxWorkGroupSize[0]);
        gl.GetInteger(GLEnum.MaxComputeWorkGroupSize, 1, out maxWorkGroupSize[1]);
        gl.GetInteger(GLEnum.MaxComputeWorkGroupSize, 2, out maxWorkGroupSize[2]);
        Console.WriteLine($"max groupe  size : {maxWorkGroupSize[0]} {maxWorkGroupSize[1]} {maxWorkGroupSize[2]}");
        
        
        // get max work group count
        int[] maxWorkGroupCount = new int[3];
        gl.GetInteger(GLEnum.MaxComputeWorkGroupCount, 0, out maxWorkGroupCount[0]);
        gl.GetInteger(GLEnum.MaxComputeWorkGroupCount, 1, out maxWorkGroupCount[1]);
        gl.GetInteger(GLEnum.MaxComputeWorkGroupCount, 2, out maxWorkGroupCount[2]);
        Console.WriteLine($"max groupe count : {maxWorkGroupCount[0]} {maxWorkGroupCount[1]} {maxWorkGroupCount[2]}");
        
        // get max work group invocations
        int[] maxWorkGroupInvocations = new int[1];
        gl.GetInteger(GetPName.MaxComputeWorkGroupInvocations, maxWorkGroupInvocations);
        Console.WriteLine($"max groupe invocations : {maxWorkGroupInvocations[0]}");
        
        
        
        // compute shadere 
        computeShader = new ComputeShader(gl, Generated.FilePathConstants.Shader.computeChunk_glsl);
        computeShader.Use();

        //init bindings
        uint inputBufferIndex =  gl.GetProgramResourceIndex(computeShader.handle, ProgramInterface.ShaderStorageBlock, "chunkBlocks");
        gl.ShaderStorageBlockBinding(computeShader.handle, inputBufferIndex, 2);
        uint ouputBufferIndex =  gl.GetProgramResourceIndex(computeShader.handle, ProgramInterface.ShaderStorageBlock, "outputBuffer");
        gl.ShaderStorageBlockBinding(computeShader.handle, ouputBufferIndex, 1);
        uint waterOuputBufferIndex =  gl.GetProgramResourceIndex(computeShader.handle, ProgramInterface.ShaderStorageBlock, "waterOutputBuffer");
        gl.ShaderStorageBlockBinding(computeShader.handle, waterOuputBufferIndex, 7);
        
        //init block data buffer
        blockDataBuffer = new BufferObject<BlockData>(gl, CHUNKS_PER_REGION * SUPER_CHUNK_NB_BLOCK, BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StreamDraw);
        
        //init countComputeBuffer
        countComputeBuffer = new BufferObject<CountCompute>(gl, 1, BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StaticRead);


        // init transparent blocks buffer
        int maxIndex = 0;
        foreach (KeyValuePair<int,Block> keyValuePair in blockFactory.blocks) {
            if (keyValuePair.Key > maxIndex) maxIndex = keyValuePair.Key;
        }
        bool[] transparentBlocks = new bool[maxIndex + 1];
        foreach (KeyValuePair<int,Block> keyValuePair in blockFactory.blocks) {
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
        FacesTextureCoords[] textureCoords = new FacesTextureCoords[(maxIndex + 1) * NB_FACE_PER_BLOCK];
        foreach (KeyValuePair<int,Block> keyValuePair in blockFactory.blocks) {
            if(keyValuePair.Value.blockJson is  null) continue;
            foreach (Face face in Enum.GetValues(typeof(Face))) {
                int[] coords = keyValuePair.Value.blockJson.texture[face];
                textureCoords[(keyValuePair.Key * NB_FACE_PER_BLOCK) +  (int)face]= new FacesTextureCoords(new Vector2D<int>(coords[0], coords[1]), 32.0f, 256.0f);
            }
        }
        textureCoordsBuffer = new BufferObject<FacesTextureCoords>(gl, textureCoords, BufferTargetARB.UniformBuffer, BufferUsageARB.StaticDraw);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 3, textureCoordsBuffer.handle); 
        
        
      
        
        // init output buffer
        int nbVertexMax = (int)(NB_VERTEX_PER_FACE * NB_FACE_PER_BLOCK * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE *
                                Chunk.CHUNK_SIZE);
        outputBuffer = new BufferObject<CubeVertex>(gl, CHUNKS_PER_REGION * nbVertexMax, BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StaticCopy);
        
        int nbWaterVertexMax = (int)(NB_VERTEX_PER_FACE * NB_FACE_PER_BLOCK * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE *
                                     Chunk.CHUNK_SIZE);
        waterOutputBuffer = new BufferObject<CubeVertex>(gl, CHUNKS_PER_REGION * nbWaterVertexMax,
            BufferTargetARB.ShaderStorageBuffer,
            BufferUsageARB.StaticCopy);
        
        
        
        //Vertex shader
        cubeShader!.Use();
        // init subBlockPositionsBuffer
        Vector4D<float>[] subBlockPositions = new Vector4D<float>[NB_SUB_BLOCK_POSITION];
        int index = 0;
        foreach (var vec in Geometry.verticesOffsets) {
            subBlockPositions[index] = new Vector4D<float>(vec.X, vec.Y, vec.Z, 0.0f);
            index++;
        }
 
        subBlockPositionsBuffer = new BufferObject<Vector4D<float>>(gl, subBlockPositions,
            BufferTargetARB.ShaderStorageBuffer, BufferUsageARB.StaticDraw);
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 5, subBlockPositionsBuffer.handle);
        
        
        // init chunks position buffer
        chunksPositionBuffer = new BufferObject<Vector4D<float>>(gl, CHUNKS_PER_REGION, BufferTargetARB.UniformBuffer, BufferUsageARB.StreamDraw);
        gl.BindBufferBase(BufferTargetARB.UniformBuffer, 6, chunksPositionBuffer.handle);
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

    public void Draw(Camera cam, LightCalculator lightCalculator) {
        haveDrawLastFrame = false;
        if (nbBlockVertex == 0) return;
        if (!RegionInCameraView(cam)) return;

        chunksPositionBuffer.SendData(chunksPositionComputed, 0);
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 5, subBlockPositionsBuffer.handle);

        vaoBlock!.Bind();
        cubeShader!.Use();
        cubeShader.SetUniform("ambientStrength", lightCalculator.lightLevel);
        cubeTexture.Bind();
        
        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbBlockVertex);
        

        haveDrawLastFrame = true;
    }

    public void DrawWater(Camera cam, LightCalculator lightCalculator) {
        if (nbWaterVertex == 0) return;
        if (!RegionInCameraView(cam)) return;


        chunksPositionBuffer.SendData(chunksPositionComputed, 0);
        
        vaoWater!.Bind();
        cubeShader!.Use();
        cubeShader.SetUniform("ambientStrength", lightCalculator.lightLevel);
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 5, subBlockPositionsBuffer.handle);

        cubeTexture.Bind();
   
        gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)nbWaterVertex);
        
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
            vboBlock?.Dispose();
            vboWater?.Dispose();
            vaoBlock?.Dispose();
            vaoWater?.Dispose();
            nbBlockVertex = 0;
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
        
        // update water output buffer
        gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 7, waterOutputBuffer.handle);
        
        
        transparentBlocksBuffer.Bind(BufferTargetARB.UniformBuffer);
        textureCoordsBuffer.Bind(BufferTargetARB.UniformBuffer);
        
        
        
        
        // compute
        gl.DispatchCompute((uint)(4 * chunkCount),4,4);
        gl.MemoryBarrier(MemoryBarrierMask.AllBarrierBits);
        CountCompute countCompute = countComputeBuffer.GetData();
        nbBlockVertex = countCompute.vertexCount;
        nbWaterVertex = countCompute.waterVertexCount;
        
        
        vboBlock?.Dispose();
        vboWater?.Dispose();
        vaoBlock?.Dispose();
        vaoWater?.Dispose();

        if (nbBlockVertex != 0) {
            vboBlock = new BufferObject<CubeVertex>(gl, nbBlockVertex, BufferTargetARB.ArrayBuffer);
            // copy output buffer to vbo
            outputBuffer.Bind(BufferTargetARB.CopyReadBuffer);
            vboBlock.Bind(BufferTargetARB.CopyWriteBuffer);
            gl.CopyBufferSubData(CopyBufferSubDataTarget.CopyReadBuffer, CopyBufferSubDataTarget.CopyWriteBuffer, 0, 0,(uint)( nbBlockVertex * sizeof(CubeVertex)));
        
        
            CubeVertex vertex = new CubeVertex();
            vaoBlock = new VertexArrayObject<CubeVertex, uint>(gl, vboBlock);
            vaoBlock.Bind();
        
            vaoBlock.VertexAttributeIPointer(0, 1, VertexAttribIType.Int, vaoBlock.GetOffset(ref vertex, ref vertex.position));
            vaoBlock.VertexAttributeIPointer(1, 1, VertexAttribIType.Int, vaoBlock.GetOffset(ref vertex, ref vertex.data));
            vaoBlock.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, vaoBlock.GetOffset(ref vertex, ref vertex.texCoords));
        }


        if (nbWaterVertex != 0) {
            vboWater = new BufferObject<CubeVertex>(gl, nbWaterVertex, BufferTargetARB.ArrayBuffer);
            // copy output buffer to vbo
            waterOutputBuffer.Bind(BufferTargetARB.CopyReadBuffer);
            vboWater.Bind(BufferTargetARB.CopyWriteBuffer);
            gl.CopyBufferSubData(CopyBufferSubDataTarget.CopyReadBuffer, CopyBufferSubDataTarget.CopyWriteBuffer, 0, 0,(uint)( nbWaterVertex * sizeof(CubeVertex)));
        
            CubeVertex vertex = new CubeVertex();
            vaoWater = new VertexArrayObject<CubeVertex, uint>(gl, vboWater);
            vaoWater.Bind();
        
            vaoWater.VertexAttributeIPointer(0, 1, VertexAttribIType.Int, vaoWater.GetOffset(ref vertex, ref vertex.position));
            vaoWater.VertexAttributeIPointer(1, 1, VertexAttribIType.Int, vaoWater.GetOffset(ref vertex, ref vertex.data));
            vaoWater.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, vaoWater.GetOffset(ref vertex, ref vertex.texCoords));
            
        }
        
        // update chunks position buffer
        chunksPositionComputed = new Vector4D<float>[chunkCount];
        for (int i = 0; i < chunkCount; i++) {
            chunksPositionComputed[i] = new Vector4D<float>(chunks[i]!.position.X, chunks[i]!.position.Y, chunks[i]!.position.Z, 0.0f);
        } 
        
    }
    
    
    private unsafe void CreateSuperChunk(Span<BlockData> superChunk) {
        int offset = 0;
        for (int i = 0; i < chunkCount; i++) {
            Chunk chunk = chunks[i]!;
            if(chunk.chunkState != ChunkState.DRAWABLE) throw new Exception("chunk state is not drawable");
            int superChunkIndex = 0;
           
            // inner chunk
            Span<BlockData> blockDataSpan = Chunk.GetBlockSpan(chunk.blocks);
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    int offsetSuperChunk = ((x + 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + (y + 1) * SUPER_CHUNK_SIZE + 1) + offset;
                    int offsetBlocks = x * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE + y * Chunk.CHUNK_SIZE;
                    blockDataSpan.Slice(offsetBlocks, Chunk.CHUNK_SIZE).CopyTo(superChunk[offsetSuperChunk..]);
                }
            }
            // left chunk
            Chunk leftNeighbor = chunk.chunksNeighbors![(int)FaceExtended.LEFT];
            Debug.Assert(leftNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            BlockData[,,] blocks = leftNeighbor.blocks;
            for(int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for(int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    superChunkIndex = 0 * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + (y + 1) * SUPER_CHUNK_SIZE + (z + 1);
                    superChunk[offset + superChunkIndex] = blocks[15,y,z];
                }
            }
            
            // right chunk
            Chunk rightNeighbor = chunk.chunksNeighbors![(int)FaceExtended.RIGHT];
            Debug.Assert(rightNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = rightNeighbor.blocks;
            for(int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                for(int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    superChunkIndex = (SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + (y + 1) * SUPER_CHUNK_SIZE + (z + 1);
                    superChunk[offset + superChunkIndex] = blocks[0,y,z];
                }
            }
            
            // top chunk
            Chunk topNeighbor = chunk.chunksNeighbors![(int)FaceExtended.TOP];
            Debug.Assert(topNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = topNeighbor.blocks;
            for(int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for(int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    superChunkIndex = (x + 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + (SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE + (z + 1);
                    superChunk[offset + superChunkIndex] = blocks[x,0,z];
                }
            }
            
            // bottom chunk
            Chunk bottomNeighbor = chunk.chunksNeighbors![(int)FaceExtended.BOTTOM];
            Debug.Assert(bottomNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = bottomNeighbor.blocks;
            for(int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for(int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    superChunkIndex = (x + 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + 0 * SUPER_CHUNK_SIZE + (z + 1);
                    superChunk[offset + superChunkIndex] = blocks[x,15,z];
                }
            }
            
            // front chunk
            Chunk frontNeighbor = chunk.chunksNeighbors![(int)FaceExtended.FRONT];
            Debug.Assert(frontNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = frontNeighbor.blocks;
            for(int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for(int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    superChunkIndex = (x + 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + (y + 1) * SUPER_CHUNK_SIZE + (SUPER_CHUNK_SIZE - 1);
                    superChunk[offset + superChunkIndex] = blocks[x,y,0];
                }
            }
            
            // back chunk
            Chunk backNeighbor = chunk.chunksNeighbors![(int)FaceExtended.BACK];
            Debug.Assert(backNeighbor.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = backNeighbor.blocks;
            for(int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for(int y = 0; y < Chunk.CHUNK_SIZE; y++) {
                    superChunkIndex = (x + 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE + (y + 1) * SUPER_CHUNK_SIZE + 0;
                    superChunk[offset + superChunkIndex] = blocks[x,y,15];
                }
            }
            
            
            //LEFTTOP
            Chunk chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTTOP];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = (0 * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (b + 1);
                superChunk[offset + superChunkIndex] = blocks[15,0,b];
            }
            
            
            
            //RIGHTTOP] 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTTOP];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (b + 1);
                superChunk[offset + superChunkIndex] = blocks[0,0,b];
            }
            
            
            //TOPFRONT 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.TOPFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((b+1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1));
                superChunk[offset + superChunkIndex] = blocks[b,0,0];
            }
            //TOPBACK 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.TOPBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((b+1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (0);
                superChunk[offset + superChunkIndex] = blocks[b,0,15];
            }
		
            //LEFTBOTTOM 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTBOTTOM];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = (0 * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + (0 * SUPER_CHUNK_SIZE) + (b+ 1);
                superChunk[offset + superChunkIndex] = blocks[15,15,b];
            }
            
            //RIGHTBOTTOM 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTBOTTOM];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + (0 * SUPER_CHUNK_SIZE) + (b+ 1);
                superChunk[offset + superChunkIndex] = blocks[0,15,b];
            }
            //BOTTOMFRONT 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.BOTTOMFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((b+1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + (0 * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
                superChunk[offset + superChunkIndex] = blocks[b,15,0];
            }
            //BOTTOMBACK 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.BOTTOMBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((b+1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + (0 * SUPER_CHUNK_SIZE) + (0);
                superChunk[offset + superChunkIndex] = blocks[b,15,15];
            }

            //LEFTTOPFRONT 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTTOPFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            superChunkIndex = ((0) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
            superChunk[offset + superChunkIndex] = chkn.blocks[15,0,0];
            //RIGHTTOPFRONT 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTTOPFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
            superChunk[offset + superChunkIndex] = chkn.blocks[0,0,0];
            
            //LEFTTOPBACK 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTTOPBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            superChunkIndex = ((0) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (0);
            superChunk[offset + superChunkIndex] = chkn.blocks[15,0,15];
            
            //RIGHTTOPBACK 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTTOPBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE) + (0);
            superChunk[offset + superChunkIndex] = chkn.blocks[0,0,15];
            
            //LEFTBOTTOMFRONT 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTBOTTOMFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            superChunkIndex = ((0) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((0) * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
            superChunk[offset + superChunkIndex] = chkn.blocks[15,15,0];
            
            //RIGHTBOTTOMFRONT 
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTBOTTOMFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
                superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((0) * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
                superChunk[offset + superChunkIndex] = chkn.blocks[0,15,0];
                
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTBOTTOMBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
                superChunkIndex = ((0) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((0) * SUPER_CHUNK_SIZE) + (0);
                superChunk[offset + superChunkIndex] = chkn.blocks[15,15,15];
                
            //RIGHTBOTTOMBACK
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTBOTTOMBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((0) * SUPER_CHUNK_SIZE) + (0);
            superChunk[offset + superChunkIndex] = chkn.blocks[0,15,15];
            
            //LEFTFRONT  
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = (0 * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((b+1) * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
                superChunk[offset + superChunkIndex] = blocks[15,b,0];
            }
            
            //RIGHTFRONT  
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTFRONT];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((b+1) * SUPER_CHUNK_SIZE) + (SUPER_CHUNK_SIZE - 1);
                superChunk[offset + superChunkIndex] = blocks[0,b,0];
            }
            
            //LEFTBACK  
            chkn = chunk.chunksNeighbors![(int)FaceExtended.LEFTBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((0) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((b+1) * SUPER_CHUNK_SIZE) + (0);
                superChunk[offset + superChunkIndex] = blocks[15,b,15];
            }
            
            //RIGHTBACK  
            chkn = chunk.chunksNeighbors![(int)FaceExtended.RIGHTBACK];
            Debug.Assert(chkn.chunkState >= ChunkState.BLOCKGENERATED);
            blocks = chkn.blocks;
            for(int b = 0; b < Chunk.CHUNK_SIZE; b++) {
                superChunkIndex = ((SUPER_CHUNK_SIZE - 1) * SUPER_CHUNK_SIZE * SUPER_CHUNK_SIZE) + ((b+1) * SUPER_CHUNK_SIZE) + (0);
                superChunk[offset + superChunkIndex] = blocks[0,b,15];
            }
            
            
            
            offset += SUPER_CHUNK_NB_BLOCK;
        }
    }

    public void Dispose() {
        vboBlock?.Dispose();
        vaoBlock?.Dispose();
        vboWater?.Dispose();
        vaoWater?.Dispose();
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
        public int waterVertexCount{get;set;}
        public int firstIndex{get;set;}
        public int vertexIndex{get;set;}

        public override string ToString() {
            return $"vertexCount : {vertexCount} blockCount : {waterVertexCount} firstIndex : {firstIndex} vertexIndex : {vertexIndex}";
        }
    }
}