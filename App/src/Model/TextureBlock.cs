using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model
{
    public class TextureBlock
    {

        private readonly BlockJson blockJson;
        private readonly CubeVertex[][] cubeVertices;

        

        public TextureBlock(BlockJson blockJson) {
            this.blockJson = blockJson;
            cubeVertices = new CubeVertex[6][];
            cubeVertices[(int)Face.TOP]  = CalculateCubeTopVertices();
            cubeVertices[(int)Face.BOTTOM]  = CalculateCubeBottomVertices();
            cubeVertices[(int)Face.LEFT]  = CalculateCubeLeftVertices();
            cubeVertices[(int)Face.RIGHT]  = CalculateCubeRightVertices();
            cubeVertices[(int)Face.FRONT]  = CalculateCubeFrontVertices();
            cubeVertices[(int)Face.BACK]  = CalculateCubeBackVertices();

        }

        public void AddCubeVerticesToList(List<CubeVertex> destinationList,FaceFlag faceFlag, Vector3D<float> blockPosition,
            Vector3D<float> chunkPosition) {
            foreach(Face face in FaceFlagUtils.GetFaces(faceFlag)) {
                foreach (CubeVertex vertex in cubeVertices[(int)face]) {
                    CubeVertex cubeVertex = vertex;
                    cubeVertex.position += blockPosition + chunkPosition;
                    destinationList.Add(cubeVertex);
                }
            }
        }

 

        private static Vector2D<float> BottomLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * textureY) / 256.0f  + 0.01f);
        }

        private static Vector2D<float> TopRight(int textureX, int textureY) {
            return  new Vector2D<float>( (32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }

        private static Vector2D<float> BottomRight(int textureX, int textureY) {
            return new Vector2D<float>((32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * textureY) / 256.0f + 0.01f );
        }

        private static Vector2D<float> TopLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }
        
        
        private CubeVertex[] CalculateCubeBackVertices()
        {
            return new CubeVertex[]
            {

                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f),
                    BottomLeft(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(0.5f, 0.5f, -0.5f),
                    TopRight(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(0.5f, -0.5f, -0.5f),
                    BottomRight(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    TopRight(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    BottomLeft(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    TopLeft(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1]))
            };
        }
        
        private  CubeVertex[] CalculateCubeFrontVertices()
        {
            return new[]
            {
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f),
                    BottomLeft(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    BottomRight(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    TopRight(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    TopRight(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    TopLeft(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    BottomLeft(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1]))
            };
        }

        private CubeVertex[] CalculateCubeRightVertices ()
        {
            return new CubeVertex[]
            {

                new (new Vector3D<float>(-0.5f, 0.5f, 0.5f),
                    TopRight(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    TopLeft(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    BottomLeft(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    BottomLeft(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    BottomRight(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    TopRight(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1]))
            };
        }
        
        
        private CubeVertex[] CalculateCubeLeftVertices()
        {
            return new[]
            {

                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    TopLeft(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    BottomRight(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    TopRight(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    BottomRight(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    TopLeft(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    BottomLeft(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1]))
            };
        }
        private  CubeVertex[] CalculateCubeBottomVertices()
        {
            return new[]
            {

                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f),
                    TopRight(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    TopLeft(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    BottomLeft(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    BottomLeft(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    BottomRight(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    TopRight(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),

            };
        }
        private  CubeVertex[] CalculateCubeTopVertices ()
        {
            return new[]
            {
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    TopLeft(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    BottomRight(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    TopRight(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    BottomRight(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    TopLeft(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    BottomLeft(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1]))
            };
        }

    }
}
