using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model.RegionDrawing;

public struct FacesTextureCoords {
    Vector2D<float> bottomLeft{get; set;}
    Vector2D<float> bottomRight{get; set;}
    Vector2D<float> topLeft{get; set;}
    Vector2D<float> topRight{get; set;}
    public FacesTextureCoords(Vector2D<int> textureCoords, float faceSize, float textureSize) {
        bottomLeft = new Vector2D<float>((faceSize * textureCoords.X) / textureSize + 0.01f,
            (faceSize * textureCoords.Y) / textureSize + 0.01f);
        topRight = new Vector2D<float>( (faceSize * (textureCoords.X + 1)) / textureSize - 0.01f, (faceSize * (textureCoords.Y + 1)) / textureSize - 0.01f );
        bottomRight = new Vector2D<float>((faceSize * (textureCoords.X + 1)) / textureSize - 0.01f, (faceSize * textureCoords.Y) / textureSize + 0.01f );
        topLeft = new Vector2D<float>( (faceSize * textureCoords.X) / textureSize + 0.01f, (faceSize * (textureCoords.Y + 1)) / textureSize - 0.01f );
    }
 
}