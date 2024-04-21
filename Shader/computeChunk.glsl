#version 430 core
layout (local_size_x = 4, local_size_y = 4, local_size_z = 4) in;

struct Vertex {
    int position;
    int data;
    vec2 coords;
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

layout(std430, binding = 7) buffer waterOutputBuffer {
    Vertex waterVertices[];
};

struct BlockData{
    int data;
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
    uint waterVertexCount;
    uint firstIndex;
    uint vertexIndex;
};

layout(std430, binding = 5) buffer countBuffer{
    Count count;
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

int getIdOfBlockData(BlockData block){
    return block.data & 0xFFFF;
}

int getSkyLightOfBlockData(BlockData block){
    return ((block.data >> 16) & 0xF0) >> 4;
}

int getBlockLightOfBlockData(BlockData block){
    return (block.data >> 16) & 0xF;
}


const int chunkSize = 18;
const int nbBlockPerChunk = chunkSize * chunkSize * chunkSize;

uint getIndex(uint chunkIndex, uint x, uint y, uint z){
    return (chunkIndex * nbBlockPerChunk) + 
        ((x + 1) * (chunkSize * chunkSize)) +
        ((y + 1) * chunkSize) +
        (z + 1);
}
// same function but different output buffer
void processWater(uint chunkIndex, int x, int y, int z, uint index);
void processBlock(uint chunkIndex, int x, int y, int z, uint index);

void main(){
    uint chunkIndex = uint(floor(gl_WorkGroupID.x / 4));
    
    uint chunkXWorkGroup = gl_WorkGroupID.x % 4;


    uint x = (chunkXWorkGroup * 4) + gl_LocalInvocationID.x;
    uint y = (gl_WorkGroupID.y * 4) + gl_LocalInvocationID.y;
    uint z = (gl_WorkGroupID.z * 4) + gl_LocalInvocationID.z;
    uint index = getIndex(chunkIndex, x, y, z);
    
    if(getIdOfBlockData(blocks[index]) == 0){
        return;
    }
    
    if(getIdOfBlockData(blocks[index]) == 10){
        processWater(chunkIndex, int(x), int(y), int(z), index);
    }else{
        processBlock(chunkIndex, int(x), int(y), int(z), index);
    }
    
}

void processWater(uint chunkIndex, int x, int y, int z, uint index){
    //calculate nb faces
    uint facesFlag = 0;
    uint nbFaces = 0;
     
    
    //top
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z)])) && getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z)]) != 10){
        facesFlag = facesFlag | TOP;
        nbFaces++;
    }
    //bottom
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z)])) && getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z)]) != 10){
        facesFlag = facesFlag | BOTTOM;
        nbFaces++;
    }
    //left
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x + 1, y, z)])) && getIdOfBlockData(blocks[getIndex(chunkIndex,x + 1, y, z)]) != 10){
        facesFlag = facesFlag | LEFT;
        nbFaces++;
    }
    //right
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x - 1, y, z)])) && getIdOfBlockData(blocks[getIndex(chunkIndex,x - 1, y, z)]) != 10){
        facesFlag = facesFlag | RIGHT;
        nbFaces++;
    }
    //front
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x, y, z + 1)])) && getIdOfBlockData(blocks[getIndex(chunkIndex,x, y, z + 1)]) != 10){
        facesFlag = facesFlag | FRONT;
        nbFaces++;
    }
    //back
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x, y, z - 1)])) && getIdOfBlockData(blocks[getIndex(chunkIndex,x, y, z - 1)]) != 10){
        facesFlag = facesFlag | BACK;
        nbFaces++;
    }
    
    int position = x + (y << 4) + (z << 8) + (int(chunkIndex) << 12);
    
    
    uint vertexIndex = atomicAdd(count.waterVertexCount, nbFaces * 6);
    
    int skyLight = (blocks[index].data & 0xF00000) >> 4;
    int blockLight = blocks[index].data & 0xF0000;
    int lightLevel = max(skyLight, blockLight) << 2;

    
   
    //back
    if((facesFlag & BACK) != 0){
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 5;
        waterVertices[vertexIndex].position = position + (30 << 16);
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (31 << 16);
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (32 << 16);
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (33 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (34 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (35 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
    }
    
    // front
    if((facesFlag & FRONT) != 0){
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 4;
        waterVertices[vertexIndex].position = position + (24 << 16);
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (25 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (26 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (27 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (28 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (29 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
    }
    
    //right
    
    if((facesFlag & RIGHT) != 0){
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 3;
        waterVertices[vertexIndex].position = position + (18 << 16);
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (19 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (20 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (21 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (22 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (23 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;   
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;    
    }
   
    //left   
    if((facesFlag & LEFT) != 0){
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 2;
        waterVertices[vertexIndex].position = position + (12 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;                   
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (13 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (14 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (15 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (16 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (17 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;    
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
    }          
                
    //bottom
    if((facesFlag & BOTTOM) != 0){
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 1;
        waterVertices[vertexIndex].position = position + (6 << 16);
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;        
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)]))
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (7 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)]))
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (8 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)]))
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (9 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)]))
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (10 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)]))
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (11 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
    }

    //top
    if((facesFlag & TOP) != 0){
        int indexFace = (getIdOfBlockData(blocks[index]) * 6);
        waterVertices[vertexIndex].position = position + (0 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), // block left top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), // block top back
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) // block top left back
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (1 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), // block right top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), // block top front
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) // block top right front
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (2 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), // block right top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), // block top back
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) // block top right back
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (3 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), // block right top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), // block top front
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) // block top right front
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (4 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), // block left top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), // block top back
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) // block top left back
        );
        waterVertices[vertexIndex].data += lightLevel;
        vertexIndex++;
        waterVertices[vertexIndex].position = position + (5 << 16); 
        waterVertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        waterVertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), // block left top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), // block top front
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) // block top left front
        );
        waterVertices[vertexIndex].data += lightLevel;
 
    }
    
}

void processBlock(uint chunkIndex, int x, int y, int z, uint index){
    
    //calculate nb faces
    uint facesFlag = 0;
    uint nbFaces = 0;
     
    
    //top
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z)]))){
        facesFlag = facesFlag | TOP;
        nbFaces++;
    }
    //bottom
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z)]))){
        facesFlag = facesFlag | BOTTOM;
        nbFaces++;
    }
    //left
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x + 1, y, z)]))){
        facesFlag = facesFlag | LEFT;
        nbFaces++;
    }
    //right
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x - 1, y, z)]))){
        facesFlag = facesFlag | RIGHT;
        nbFaces++;
    }
    //front
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x, y, z + 1)]))){
        facesFlag = facesFlag | FRONT;
        nbFaces++;
    }
    //back
    if(isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex,x, y, z - 1)]))){
        facesFlag = facesFlag | BACK;
        nbFaces++;
    }
    
    int position = x + (y << 4) + (z << 8) + (int(chunkIndex) << 12);
    
    uint vertexIndex = atomicAdd(count.vertexCount, nbFaces * 6);
    
   
    //back
    if((facesFlag & BACK) != 0){
        int skyLight = getSkyLightOfBlockData(blocks[getIndex(chunkIndex, x, y, z - 1)]);
        int blockLight = getBlockLightOfBlockData(blocks[getIndex(chunkIndex, x, y, z - 1)]);
    
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 5;
        vertices[vertexIndex].position = position + (30 << 16);
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;
        vertexIndex++;
        vertices[vertexIndex].position = position + (31 << 16);
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;
        vertexIndex++;
        vertices[vertexIndex].position = position + (32 << 16);
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (33 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (34 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (35 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
    }
    
    // front
    if((facesFlag & FRONT) != 0){
        int skyLight = getSkyLightOfBlockData(blocks[getIndex(chunkIndex, x, y, z + 1)]);
        int blockLight = getBlockLightOfBlockData(blocks[getIndex(chunkIndex, x, y, z + 1)]);
    
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 4;
        vertices[vertexIndex].position = position + (24 << 16);
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (25 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (26 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (27 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (28 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (29 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
    }
    
    //right
    
    if((facesFlag & RIGHT) != 0){
        int skyLight = getSkyLightOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z)]);
        int blockLight = getBlockLightOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z)]);
    
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 3;
        vertices[vertexIndex].position = position + (18 << 16);
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (19 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (20 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (21 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (22 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (23 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;   
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;    
    }
   
    //left   
    if((facesFlag & LEFT) != 0){
        int skyLight = getSkyLightOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z)]);
        int blockLight = getBlockLightOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z)]);
    
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 2;
        vertices[vertexIndex].position = position + (12 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;                   
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (13 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (14 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (15 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (16 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (17 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;    
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y, z + 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
    }          
                
    //bottom
    if((facesFlag & BOTTOM) != 0){
        int skyLight = getSkyLightOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z)]);
        int blockLight = getBlockLightOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z)]);
    
        int indexFace = (getIdOfBlockData(blocks[index]) * 6)  + 1;
        vertices[vertexIndex].position = position + (6 << 16);
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;        
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)]))
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (7 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z - 1)]))
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (8 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)]))
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (9 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y - 1, z + 1)]))
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (10 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z + 1)])),
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z + 1)]))
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (11 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y - 1, z - 1)])), 
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y - 1, z - 1)])) 
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
    }

    //top
    if((facesFlag & TOP) != 0){
        int skyLight = getSkyLightOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z)]);
        int blockLight = getBlockLightOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z)]);
    
        int indexFace = (getIdOfBlockData(blocks[index]) * 6);
        vertices[vertexIndex].position = position + (0 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), // block left top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), // block top back
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) // block top left back
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (1 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), // block right top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), // block top front
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) // block top right front
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (2 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), // block right top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), // block top back
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z - 1)])) // block top right back
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (3 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomRight;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z)])), // block right top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), // block top front
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x + 1, y + 1, z + 1)])) // block top right front
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (4 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].topLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), // block left top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z - 1)])), // block top back
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z - 1)])) // block top left back
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

        vertexIndex++;
        vertices[vertexIndex].position = position + (5 << 16); 
        vertices[vertexIndex].coords = texCoords[indexFace].bottomLeft;
        vertices[vertexIndex].data = vertexAO(
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z)])), // block left top
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x, y + 1, z + 1)])), // block top front
            !isTransparent(getIdOfBlockData(blocks[getIndex(chunkIndex, x - 1, y + 1, z + 1)])) // block top left front
        );
        vertices[vertexIndex].data += blockLight << 2;
        vertices[vertexIndex].data += skyLight << 6;

    }
    
}