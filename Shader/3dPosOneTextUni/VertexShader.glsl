#version 430 core
layout (location = 0) in vec4 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float ambiantOcclusion;
layout (location = 3) in float lightLevel;

out vec2 TexCoord;
out vec4 lightColor;

layout (std140, binding = 0) uniform Matrices{
    mat4 projection;
    mat4 view;
};

void main()
{
    gl_Position = projection * view * aPos;
    TexCoord = vec2(aTexCoord.x, aTexCoord.y);
    lightColor = vec4(1.0f, 1.0f, 1.0f, 1.0f);
    if(ambiantOcclusion == 2f){
        lightColor = vec4(0.8f, 0.8f, 0.8f, 1.0f);
    }
    if(ambiantOcclusion == 1f){
        lightColor = vec4(0.6f, 0.6f, 0.6f, 1.0f);
    }
    if(ambiantOcclusion == 0f){
        lightColor = vec4(0.4f, 0.4f, 0.4f, 1.0f);
    }
    
} 