#version 330 core
out vec4 FragColor;
  
in vec2 TexCoord;
flat in int face;

uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform sampler2D texture4;
uniform sampler2D texture5;
uniform sampler2D texture6;


void main()
{
    
    switch(face){
        case 0: //TOP
            FragColor = texture(texture1, TexCoord);
            break;
        case 1065353216: //Ground
            FragColor = texture(texture2, TexCoord);
            break;
        case 1077936128: //LEFT
            FragColor = texture(texture3, TexCoord);
            break;
        case 1073741824: //RIGHT
            FragColor = texture(texture4, TexCoord);
            break;
        case 1084227584: //BACK
            FragColor = texture(texture5, TexCoord);
            break;
        case 1082130432: //FRONT
            FragColor = texture(texture5, TexCoord);
            break;
        default:
            FragColor = vec4(0.2, 0.2, 0.2, 1.0);
        break;
    }
}