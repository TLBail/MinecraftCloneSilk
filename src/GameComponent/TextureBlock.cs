using System.Numerics;
using System.Text.Json;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent
{
    public class TextureBlock
    {
        private class TextureBlockJson
        {
            public string? name { get; set; }
            public Dictionary<Face, int[]> texture { get; set; }
        }

        private readonly TextureBlockJson textureBlockJson;
        private readonly CubeVertex[][] cubeVertices;

        private const string PATH_TO_JSON = "./Assets/blocks/json/";

        private static readonly Dictionary<string, TextureBlock> textureBlocks = new Dictionary<string, TextureBlock>();

        public static List<string> keys()
        {
            return textureBlocks.Keys.ToList();
        }
        
        public static TextureBlock get(string nameBlock)
        {
            if (textureBlocks.ContainsKey(nameBlock)) {
                return textureBlocks[nameBlock];
            }

            TextureBlock textureBlock = new TextureBlock(nameBlock);
            textureBlocks.Add(nameBlock, textureBlock);
            return textureBlock;
        }

        private TextureBlock(string nameBlock)
        {
            string path = PATH_TO_JSON + nameBlock + ".json";
            string jsonString = File.ReadAllText(path);
            textureBlockJson = JsonSerializer.Deserialize<TextureBlockJson>(jsonString)!;
            cubeVertices = new CubeVertex[6][];
            cubeVertices[(int)Face.BACK]  = calculateCubeBackVertices();
            cubeVertices[(int)Face.FRONT]  = calculateCubeFrontVertices();
            cubeVertices[(int)Face.LEFT]  = calculateCubeLeftVertices();
            cubeVertices[(int)Face.RIGHT]  = calculateCubeRightVertices();
            cubeVertices[(int)Face.BOTTOM]  = calculateCubeBottomVertices();
            cubeVertices[(int)Face.TOP]  = calculateCubeTopVertices();
        }

        public CubeVertex[] getCubeVertices(Face[] faces, Vector3D<float> blockPosition)
        {
            CubeVertex[] vertices = new CubeVertex[6 * faces.Length];
            int index = 0;
            foreach (var face in faces) {
                foreach (var vertex in cubeVertices[(int)face]) {
                    vertices[index] = vertex;
                    vertices[index].position += blockPosition;
                    index++;
                }
            }
            return vertices;
        }
        private Vector2D<float> bottomLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * textureY) / 256.0f  + 0.01f);
        }

        private Vector2D<float> topRight(int textureX, int textureY) {
            return  new Vector2D<float>( (32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }

        private Vector2D<float> bottomRight(int textureX, int textureY) {
            return new Vector2D<float>((32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * textureY) / 256.0f + 0.01f );
        }

        private Vector2D<float> topLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }
        
        
        private CubeVertex[] calculateCubeBackVertices()
        {
            return new CubeVertex[]
            {

                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f),
                    bottomLeft(textureBlockJson.texture[Face.BACK][0], textureBlockJson.texture[Face.BACK][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f),
                    topRight(textureBlockJson.texture[Face.BACK][0], textureBlockJson.texture[Face.BACK][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),
                    bottomRight(textureBlockJson.texture[Face.BACK][0], textureBlockJson.texture[Face.BACK][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    topRight(textureBlockJson.texture[Face.BACK][0], textureBlockJson.texture[Face.BACK][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.BACK][0], textureBlockJson.texture[Face.BACK][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(textureBlockJson.texture[Face.BACK][0], textureBlockJson.texture[Face.BACK][1]))
            };
        }
        
        private  CubeVertex[] calculateCubeFrontVertices()
        {
            return new[]
            {
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f),
                    bottomLeft(textureBlockJson.texture[Face.FRONT][0], textureBlockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomRight(textureBlockJson.texture[Face.FRONT][0], textureBlockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topRight(textureBlockJson.texture[Face.FRONT][0], textureBlockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topRight(textureBlockJson.texture[Face.FRONT][0], textureBlockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    topLeft(textureBlockJson.texture[Face.FRONT][0], textureBlockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.FRONT][0], textureBlockJson.texture[Face.FRONT][1]))
            };
        }

        private CubeVertex[] calculateCubeRightVertices ()
        {
            return new CubeVertex[]
            {

                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f),
                    topRight(textureBlockJson.texture[Face.RIGHT][0], textureBlockJson.texture[Face.RIGHT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(textureBlockJson.texture[Face.RIGHT][0], textureBlockJson.texture[Face.RIGHT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.RIGHT][0], textureBlockJson.texture[Face.RIGHT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.RIGHT][0], textureBlockJson.texture[Face.RIGHT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    bottomRight(textureBlockJson.texture[Face.RIGHT][0], textureBlockJson.texture[Face.RIGHT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    topRight(textureBlockJson.texture[Face.RIGHT][0], textureBlockJson.texture[Face.RIGHT][1]))
            };
        }
        
        
        private CubeVertex[] calculateCubeLeftVertices()
        {
            return new[]
            {

                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topLeft(textureBlockJson.texture[Face.LEFT][0], textureBlockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    bottomRight(textureBlockJson.texture[Face.LEFT][0], textureBlockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    topRight(textureBlockJson.texture[Face.LEFT][0], textureBlockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    bottomRight(textureBlockJson.texture[Face.LEFT][0], textureBlockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topLeft(textureBlockJson.texture[Face.LEFT][0], textureBlockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.LEFT][0], textureBlockJson.texture[Face.LEFT][1]))
            };
        }
        private  CubeVertex[] calculateCubeBottomVertices()
        {
            return new[]
            {

                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f),
                    topRight(textureBlockJson.texture[Face.BOTTOM][0], textureBlockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    topLeft(textureBlockJson.texture[Face.BOTTOM][0], textureBlockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.BOTTOM][0], textureBlockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.BOTTOM][0], textureBlockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    bottomRight(textureBlockJson.texture[Face.BOTTOM][0], textureBlockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    topRight(textureBlockJson.texture[Face.BOTTOM][0], textureBlockJson.texture[Face.BOTTOM][1])),

            };
        }
        private  CubeVertex[] calculateCubeTopVertices ()
        {
            return new[]
            {
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(textureBlockJson.texture[Face.TOP][0], textureBlockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    bottomRight(textureBlockJson.texture[Face.TOP][0], textureBlockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    topRight(textureBlockJson.texture[Face.TOP][0], textureBlockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    bottomRight(textureBlockJson.texture[Face.TOP][0], textureBlockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(textureBlockJson.texture[Face.TOP][0], textureBlockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    bottomLeft(textureBlockJson.texture[Face.TOP][0], textureBlockJson.texture[Face.TOP][1]))
            };
        }

    }
}
