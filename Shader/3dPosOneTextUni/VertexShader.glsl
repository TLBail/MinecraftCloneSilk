#version 430 core
layout (location = 0) in vec4 aPos;
layout (location = 1) in vec4 aTexCoord;

out vec2 TexCoord;

layout (std140, binding = 0) uniform Matrices{
    mat4 projection;
    mat4 view;
};

void main()
{
    gl_Position = projection * view * aPos;
    TexCoord = vec2(aTexCoord.x, aTexCoord.y);
} 