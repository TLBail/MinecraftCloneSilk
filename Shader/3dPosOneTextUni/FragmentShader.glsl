#version 430 core
out vec4 FragColor;
  
in vec2 TexCoord;
in vec3 lightColor;

uniform sampler2D texture1;

uniform float ambientStrength;

void main()
{
    vec3 ambient = ambientStrength * lightColor;

    vec4 texColor = texture(texture1, TexCoord);
    if(texColor.a < 0.1)
        discard;
        
    vec4 result = vec4(ambient,1.0) * texColor;
    
    FragColor = result;
}