using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.src.GameComponent
{
    public class TextureBlock
    {
        private class TextureBlockJson
        {
            public string name { get; set; }
            public Dictionary<Face, int[]> texture { get; set; }
        }

        private TextureBlockJson textureBlockJson;
        public CubeVertex[] cubeVertices; 

        private static readonly string pathToJson = "./Assets/blocks/json/";

        
        public TextureBlock(string nameBlock)
        {
            string path = pathToJson + nameBlock + ".json";
            string jsonString = File.ReadAllText(path);
            textureBlockJson = JsonSerializer.Deserialize<TextureBlockJson>(jsonString)!;

            List<CubeVertex> listCubeVertices = new List<CubeVertex>();
            listCubeVertices.AddRange(calculateCubeBackVertices());
            listCubeVertices.AddRange(calculateCubeFrontVertices());
            listCubeVertices.AddRange(calculateCubeLeftVertices());
            listCubeVertices.AddRange(calculateCubeRightVertices());
            listCubeVertices.AddRange(calculateCubeBottomVertices());
            listCubeVertices.AddRange(calculateCubeTopVertices());
            cubeVertices = listCubeVertices.ToArray();
        }
        
        public Vector2D<float> bottomLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * textureY) / 256.0f  + 0.01f);
        }

        public Vector2D<float> topRight(int textureX, int textureY) {
            return  new Vector2D<float>( (32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }

        public Vector2D<float> bottomRight(int textureX, int textureY) {
            return new Vector2D<float>((32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * textureY) / 256.0f + 0.01f );
        }

        public Vector2D<float> topLeft(int textureX, int textureY) {
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
