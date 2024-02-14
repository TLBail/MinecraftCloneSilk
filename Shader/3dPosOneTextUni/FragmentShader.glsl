#version 430 core
out vec4 FragColor;
  
in vec2 TexCoord;
in float lightLevel;
in float ambiantOcclusion;


uniform sampler2D texture1;

uniform float ambientStrength;

void main()
{

    vec4 texColor = texture(texture1, TexCoord);
    if(texColor.a < 0.1)
        discard;
        
    texColor.rgb *= ambiantOcclusion;
    texColor.rgb *= lightLevel;
    texColor.rgb *= ambientStrength;
    
    FragColor = texColor; 
}