#version 430 core
layout (location = 0) in vec4 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in int ambiantOcclusion;
layout (location = 3) in int lightLevel;

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
    float factor = 1.0f;
    
    if(ambiantOcclusion == 2){
        factor = 0.8f;
    }
    if(ambiantOcclusion == 1){
        factor = 0.6f;
    }
    if(ambiantOcclusion == 0){
        factor = 0.4f;
    }
    
    factor *= float(lightLevel) / 15.0f;
    lightColor = vec4(lightColor.r * factor, lightColor.g * factor, lightColor.b * factor, 1.0f);
    
} 