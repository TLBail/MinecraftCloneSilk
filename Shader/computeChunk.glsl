#version 430 core
layout (local_size_x = 4, local_size_y = 4, local_size_z = 4) in;

struct Vertex {
    vec4 position;
    vec4 coords;
};

layout(std430, binding = 1) buffer outputBuffer {
    Vertex vertices[];
};



layout(std430, binding = 2) buffer chunkBlocks {
    int idBlocks[];
};


layout(std140, binding = 3)  uniform texBuffer {
    vec4 texCoords[384];
};

layout(std140, binding = 4)  uniform transparentBlocks {
    bool transparent[64];
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


layout(std140, binding = 0) uniform chunkCoord{
    vec4 chunkCoords[16];
};

const uint EMPTY = 0;
const uint TOP = 1;
const uint BOTTOM = 2;
const uint LEFT = 4;
const uint RIGHT = 8;
const uint FRONT = 16;
const uint BACK = 32;

vec4 bottomLeft(vec4 textureCoords) {
    return vec4( (32.0f * textureCoords.x) / 256.0f + 0.01f, (32.0f * textureCoords.y) / 256.0f  + 0.01f, 0.0f, 0.0f);
}

vec4 topRight(vec4 textureCoords) {
    return  vec4( (32.0f * (textureCoords.x + 1)) / 256.0f - 0.01f, (32.0f * (textureCoords.y + 1)) / 256.0f - 0.01f , 0.0f, 0.0f);
}

vec4 bottomRight(vec4 textureCoords) {
    return vec4((32.0f * (textureCoords.x + 1)) / 256.0f - 0.01f, (32.0f * textureCoords.y) / 256.0f + 0.01f , 0.0f, 0.0f);
}

vec4 topLeft(vec4 textureCoords) {
    return vec4( (32.0f * textureCoords.x) / 256.0f + 0.01f, (32.0f * (textureCoords.y + 1)) / 256.0f - 0.01f , 0.0f, 0.0f);
}

const int chunkSize = 16;

uint getIndex(uint x, uint y, uint z){
    return (x * (chunkSize * chunkSize)) +
        (y * chunkSize) +
        (z);
}


void main(){
    //simple color gradient
    
    //count.blockCount = texCoords[(idBlocks[index] * 6)  + 5].x;
    //count.vertexIndex = texCoords[(idBlocks[index] * 6)  + 5].y;


    uint x = (gl_WorkGroupID.x * 4) + gl_LocalInvocationID.x;
    uint y = (gl_WorkGroupID.y * 4) + gl_LocalInvocationID.y;
    uint z = (gl_WorkGroupID.z * 4) + gl_LocalInvocationID.z;
    uint index = getIndex(x, y, z);
    
    if(transparent[idBlocks[index]]){
        return;
    }
    
    //calculate nb faces
    uint facesFlag = 0;
    uint nbFaces = 0;
    
    
    
    //top
    if(y < (chunkSize - 1) &&  transparent[idBlocks[getIndex(x, y + 1, z)]]){
        facesFlag = facesFlag | TOP;
        nbFaces++;
    }
    //bottom
    if(y > 0 && transparent[idBlocks[getIndex(x, y - 1, z)]]){
        facesFlag = facesFlag | BOTTOM;
        nbFaces++;
    }
    //left
    if(x < (chunkSize - 1) &&  transparent[idBlocks[getIndex(x + 1, y, z)]]){
        facesFlag = facesFlag | LEFT;
        nbFaces++;
    }
    //right
    if(x > 0 &&  transparent[idBlocks[getIndex(x - 1, y, z)]]){
        facesFlag = facesFlag | RIGHT;
        nbFaces++;
    }
    //front
    if(z < (chunkSize - 1) &&  transparent[idBlocks[getIndex(x, y, z + 1)]]){
        facesFlag = facesFlag | FRONT;
        nbFaces++;
    }
    //back
    if(z > 0 && transparent[idBlocks[getIndex(x, y, z - 1)]]){
        facesFlag = facesFlag | BACK;
        nbFaces++;
    }
    
    int chunkIndex = int(floor(gl_WorkGroupID.x / 4));
    vec4 chunkCoord = chunkCoords[chunkIndex];
    vec4 position = vec4(chunkCoord.x + x, chunkCoord.y + y,+ z + chunkCoord.z, 1.0f);
    
    
    uint vertexIndex = atomicAdd(count.vertexCount, nbFaces * 6);
    
   
    //back
    if((facesFlag & BACK) != 0){
        vertices[vertexIndex].position =  vec4(-0.5f, -0.5f, -0.5f, 1.0f) + position;
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 5]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 1.0f) + position;
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 5]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 1.0f) + position;
        vertices[vertexIndex].coords = bottomRight(texCoords[(idBlocks[index] * 6)  + 5]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 5]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 5]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[(idBlocks[index] * 6)  + 5]);
        vertexIndex++;
    }
    
    // front
    if((facesFlag & FRONT) != 0){
        vertices[vertexIndex].position =  vec4(-0.5f, -0.5f, 0.5f, 1.0f) + position;
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 4]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[(idBlocks[index] * 6)  + 4]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 4]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 4]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[(idBlocks[index] * 6)  + 4]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 4]);
        vertexIndex++;
    }
    
    //right
    
    if((facesFlag & RIGHT) != 0){
        vertices[vertexIndex].position =  vec4(-0.5f, 0.5f, 0.5f, 1.0f) + position;
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 3]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[(idBlocks[index] * 6)  + 3]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 3]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 3]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[(idBlocks[index] * 6)  + 3]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 3]);   
        vertexIndex++;    
    }
   
    //left   
    if((facesFlag & LEFT) != 0){
        vertices[vertexIndex].position =  vec4(0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[(idBlocks[index] * 6)  + 2]);                   
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[(idBlocks[index] * 6)  + 2]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 2]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[(idBlocks[index] * 6)  + 2]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[(idBlocks[index] * 6)  + 2]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 2]);    
        vertexIndex++;
    }          
                
    //bottom
    if((facesFlag & BOTTOM) != 0){
        vertices[vertexIndex].position =  vec4(-0.5f, -0.5f, -0.5f, 1.0f) + position;
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 1]);        
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[(idBlocks[index] * 6)  + 1]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 1]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[(idBlocks[index] * 6)  + 1]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[(idBlocks[index] * 6)  + 1]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, -0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[(idBlocks[index] * 6)  + 1]);
        vertexIndex++;
    }

    //top
    if((facesFlag & TOP) != 0){
        vertices[vertexIndex].position =  vec4(-0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[idBlocks[index] * 6]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[idBlocks[index] * 6]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topRight(texCoords[idBlocks[index] * 6]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomRight(texCoords[idBlocks[index] * 6]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, -0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = topLeft(texCoords[idBlocks[index] * 6]);
        vertexIndex++;
        vertices[vertexIndex].position = vec4(-0.5f, 0.5f, 0.5f, 1.0f) + position; 
        vertices[vertexIndex].coords = bottomLeft(texCoords[idBlocks[index] * 6]);
    }
    
}