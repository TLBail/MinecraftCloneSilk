#version 430 core
layout (location = 0) in int pos;
layout (location = 1) in int data;
layout (location = 2) in vec2 aTexCoord;

out vec2 TexCoord;
flat out int lightLevel;
flat out int skyLightLevel;
out float ambiantOcclusion;

layout (std140, binding = 0) uniform Matrices{
    mat4 projection;
    mat4 view;
};


layout(std430, binding = 5) readonly buffer subBlockPositionsBuffer {
    vec4 subBlockPositions[65535]; //2^16 - 1
};

layout(std140, binding = 6) uniform chunkCoord{
    vec4 chunkCoords[16];
};

void main()
{
    int x = pos & 0x0000000F;
    int y = (pos & 0x000000F0) >> 4;
    int z = (pos & 0x00000F00) >> 8;
    int chunkIndex = (pos & 0x0000F000) >> 12;
    int subBlockPositionsIndex = (pos & 0xFFFF0000) >> 16;
    
    vec4 aPos = vec4(int(x), int(y), int(z), 1.0f) + subBlockPositions[subBlockPositionsIndex] + chunkCoords[chunkIndex];
    
    lightLevel = int((data >> 2) & 0xF);
    skyLightLevel = int((data >> 6) & 0xF);
    int aoInt = data & 0x3;
    if(aoInt == 3) ambiantOcclusion = 1.0f;
    if(aoInt == 2) ambiantOcclusion = 0.80f;
    if(aoInt == 1) ambiantOcclusion = 0.60f;
    if(aoInt == 0) ambiantOcclusion = 0.40f;

    gl_Position = projection * view * aPos;
    TexCoord = vec2(aTexCoord.x, aTexCoord.y);
} 