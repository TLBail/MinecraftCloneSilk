#version 430 core
out vec4 FragColor;
  
in vec2 TexCoord;
flat in int lightLevel;
flat in int skyLightLevel;
in float ambiantOcclusion;


uniform sampler2D texture1;

uniform float ambientStrength;

void main()
{

    vec4 texColor = texture(texture1, TexCoord);
    if(texColor.a < 0.1)
        discard;
        
    float skyLight = (float(skyLightLevel) / 15.0) * ambientStrength;
    float lightMultiplier = max(max(float(lightLevel) / 15.0, skyLight), 0.05);    
    texColor.rgb *= lightMultiplier;
    texColor.rgb *= ambiantOcclusion;
    
    FragColor = texColor; 
}