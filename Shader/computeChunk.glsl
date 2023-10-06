#version 430 core
layout (local_size_x = 4, local_size_y = 4, local_size_z = 4) in;

struct Vertex {
    vec4 position;
    vec2 coords;
    int ambientOcclusion;
    int lightLevel;
};

struct TextureCoordsFace{
    vec2 bottomLeft;
    vec2 bottomRight;
    vec2 topLeft;
    vec2 topRight;
};

layout(std430, binding = 1) buffer outputBuffer {
    Vertex vertices[];
};


struct BlockData{
    int id;
    int data1;
};

layout(std430, binding = 2) buffer chunkBlocks {
    BlockData blocks[];
};


layout(std140, binding = 3)  uniform texBuffer {
    TextureCoordsFace texCoords[384];
};

layout(std140, binding = 4)  uniform transparentBlocks {
    vec4 transparent[64];
};

struct Count{
    uint vertexCount;
    uint blockCount;
    uint firstIndex;
    uint vertexIndex;
};

layout(std430, binding = 5) buffer countBuffer{
    Count count;
};


layout(std140, binding = 6) uniform chunkCoord{
    vec4 chunkCoords[16];
};

const uint EMPTY = 0;
const uint TOP = 1;
const uint BOTTOM = 2;
const uint LEFT = 4;
const uint RIGHT = 8;
const uint FRONT = 16;
const uint BACK = 32;

bool isTransparent(int id){
    return transparent[id].x != 0;
}
int vertexAO(bool side1, bool side2, bool corner) {
  if(side1 && side2) {
    return 0;
  }
  return 3 - ((side1 ? 1 : 0)+ (side2 ? 1 : 0) + (corner ? 1 : 0));
}


const int chunkSize = 18;
const int nbBlockPerChunk = chunkSize * chunkSize * chunkSize;

uint getIndex(uint chunkIndex, uint x, uint y, uint z){
    return (chunkIndex * nbBlockPerChunk) + 
        ((x + 1) * (chunkSize * chunkSize)) +
        ((y + 1) * chunkSize) +
        (z + 1);
}


void main(){
    //simple color gradient
    
    
    uint chunkIndex = uint(floor(gl_WorkGroupID.x / 4));
    
    uint chunkXWorkGroup = gl_WorkGroupID.x % 4;


    uint x = (chunkXWorkGroup * 4) + gl_LocalInvocationID.x;
    uint y = (gl_WorkGroupID.y * 4) + gl_LocalInvocationID.y;
    uint z = (gl_WorkGroupID.z * 4) + gl_LocalInvocationID.z;
    uint index = getIndex(chunkIndex, x, y, z);
    
    if(blocks[index].id == 0){
        return;
    }
    
    //calculate nb faces
    uint facesFlag = 0;
    uint nbFaces = 0;
    
    
    
    //top
    if(isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z)].id)){
        facesFlag = facesFlag | TOP;
        nbFaces++;
    }
    //bottom
    if(isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z)].id)){
        facesFlag = facesFlag | BOTTOM;
        nbFaces++;
    }
    //left
    if(isTransparent(blocks[getIndex(chunkIndex,x + 1, y, z)].id)){
        facesFlag = facesFlag | LEFT;
        nbFaces++;
    }
    //right
    if(isTransparent(blocks[getIndex(chunkIndex,x - 1, y, z)].id)){
        facesFlag = facesFlag | RIGHT;
        nbFaces++;
    }
    //front
    if(isTransparent(blocks[getIndex(chunkIndex,x, y, z + 1)].id)){
        facesFlag = facesFlag | FRONT;
        nbFaces++;
    }
    //back
    if(isTransparent(blocks[getIndex(chunkIndex,x, y, z - 1)].id)){
        facesFlag = facesFlag | BACK;
        nbFaces++;
    }
    
    vec4 chunkCoord = chunkCoords[chunkIndex];
    vec4 position = vec4(chunkCoord.x + x, chunkCoord.y + y,chunkCoord.z + z, 1.0f);
    
    
    uint vertexIndex = atomicAdd(count.vertexCount, nbFaces * 6);
    
   
    //back
    if((facesFlag & BACK) != 0){
        int indexFace = (blocks[index].id * 6)  + 5;
        vertices[vertexIndex].position =  vec4(-0.5f, -0.5f, -0.5f, 0.0f) + position;
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 0.0f) + position;
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 0.0f) + position;
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
    }
    
    // front
    if((facesFlag & FRONT) != 0){
        int indexFace = (blocks[index].id * 6)  + 4;
        vertices[vertexIndex].position =  vec4(-0.5f, -0.5f, 0.5f, 0.0f) + position;
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
    }
    
    //right
    
    if((facesFlag & RIGHT) != 0){
        int indexFace = (blocks[index].id * 6)  + 3;
        vertices[vertexIndex].position =  vec4(-0.5f, 0.5f, 0.5f, 0.0f) + position;
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;   
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;    
    }
   
    //left   
    if((facesFlag & LEFT) != 0){
        int indexFace = (blocks[index].id * 6)  + 2;
        vertices[vertexIndex].position =  vec4(0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;                   
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;    
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y, z + 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
    }          
                
    //bottom
    if((facesFlag & BOTTOM) != 0){
        int indexFace = (blocks[index].id * 6)  + 1;
        vertices[vertexIndex].position =  vec4(-0.5f, -0.5f, -0.5f, 0.0f) + position;
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;        
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z - 1)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)].id)
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z - 1)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)].id)
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z + 1)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)].id)
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z + 1)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)].id)
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z + 1)].id),
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)].id)
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x, y - 1, z - 1)].id), 
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)].id) 
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
    }

    //top
    if((facesFlag & TOP) != 0){
        int indexFace = (blocks[index].id * 6);
        vertices[vertexIndex].position =  vec4(-0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z)].id), // block left top
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z - 1)].id), // block top back
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)].id) // block top left back
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z)].id), // block right top
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z + 1)].id), // block top front
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)].id) // block top right front
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z)].id), // block right top
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z - 1)].id), // block top back
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)].id) // block top right back
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z)].id), // block right top
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z + 1)].id), // block top front
            !isTransparent(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)].id) // block top right front
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, -0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z)].id), // block left top
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z - 1)].id), // block top back
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)].id) // block top left back
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, 0.5f, 0.0f) + position; 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].ambientOcclusion = vertexAO(
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z)].id), // block left top
            !isTransparent(blocks[getIndex(chunkIndex, x, y + 1, z + 1)].id), // block top front
            !isTransparent(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)].id) // block top left front
        );
        vertices[vertexIndex].lightLevel = blocks[index].data1 & 0x0F;
    }
    
}