#version 430 core
out vec4 FragColor;
  
in vec2 TexCoord;
in vec4 lightColor;

uniform sampler2D texture1;

uniform float ambientStrength;

void main()
{
    vec4 ambient = ambientStrength * lightColor;

    vec4 texColor = texture(texture1, TexCoord);
    if(texColor.a < 0.1)
        discard;
        
    vec4 result = ambient * texColor;
    
    FragColor = result;
}