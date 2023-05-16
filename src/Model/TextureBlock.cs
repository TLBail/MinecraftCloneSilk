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
            cubeVertices[(int)Face.TOP]  = calculateCubeTopVertices();
            cubeVertices[(int)Face.BOTTOM]  = calculateCubeBottomVertices();
            cubeVertices[(int)Face.LEFT]  = calculateCubeLeftVertices();
            cubeVertices[(int)Face.RIGHT]  = calculateCubeRightVertices();
            cubeVertices[(int)Face.FRONT]  = calculateCubeFrontVertices();
            cubeVertices[(int)Face.BACK]  = calculateCubeBackVertices();

        }

        public IEnumerable<CubeVertex> getCubeVertices(FaceFlag faceFlag, Vector3D<float> blockPosition, Vector3D<float> chunkPosition)
        {
            CubeVertex[] vertices = new CubeVertex[6 * FaceFlagUtils.nbFaces(faceFlag)];
            int index = 0;
            IEnumerator<Face> enumerator = FaceFlagUtils.getFaces(faceFlag);
            while(enumerator.MoveNext()) {
                Face face = enumerator.Current;
                foreach (var vertex in cubeVertices[(int)face]) {
                    vertices[index] = vertex;
                    vertices[index].position += blockPosition + chunkPosition;
                    index++;
                }
            }
            return vertices;
        }

        private static Vector2D<float> bottomLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * textureY) / 256.0f  + 0.01f);
        }

        private static Vector2D<float> topRight(int textureX, int textureY) {
            return  new Vector2D<float>( (32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }

        private static Vector2D<float> bottomRight(int textureX, int textureY) {
            return new Vector2D<float>((32.0f * (textureX + 1)) / 256.0f - 0.01f, (32.0f * textureY) / 256.0f + 0.01f );
        }

        private static Vector2D<float> topLeft(int textureX, int textureY) {
            return new Vector2D<float>( (32.0f * textureX) / 256.0f + 0.01f, (32.0f * (textureY + 1)) / 256.0f - 0.01f );
        }
        
        
        private CubeVertex[] calculateCubeBackVertices()
        {
            return new CubeVertex[]
            {

                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f),
                    bottomLeft(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(0.5f, 0.5f, -0.5f),
                    topRight(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(0.5f, -0.5f, -0.5f),
                    bottomRight(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    topRight(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    bottomLeft(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1])),
                new (new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(blockJson.texture[Face.BACK][0], blockJson.texture[Face.BACK][1]))
            };
        }
        
        private  CubeVertex[] calculateCubeFrontVertices()
        {
            return new[]
            {
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f),
                    bottomLeft(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomRight(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topRight(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topRight(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    topLeft(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    bottomLeft(blockJson.texture[Face.FRONT][0], blockJson.texture[Face.FRONT][1]))
            };
        }

        private CubeVertex[] calculateCubeRightVertices ()
        {
            return new CubeVertex[]
            {

                new (new Vector3D<float>(-0.5f, 0.5f, 0.5f),
                    topRight(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    bottomLeft(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    bottomLeft(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    bottomRight(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1])),
                new (new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    topRight(blockJson.texture[Face.RIGHT][0], blockJson.texture[Face.RIGHT][1]))
            };
        }
        
        
        private CubeVertex[] calculateCubeLeftVertices()
        {
            return new[]
            {

                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topLeft(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    bottomRight(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    topRight(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    bottomRight(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    topLeft(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomLeft(blockJson.texture[Face.LEFT][0], blockJson.texture[Face.LEFT][1]))
            };
        }
        private  CubeVertex[] calculateCubeBottomVertices()
        {
            return new[]
            {

                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f),
                    topRight(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f), 
                    topLeft(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomLeft(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(0.5f, -0.5f, 0.5f), 
                    bottomLeft(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, 0.5f), 
                    bottomRight(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, -0.5f, -0.5f), 
                    topRight(blockJson.texture[Face.BOTTOM][0], blockJson.texture[Face.BOTTOM][1])),

            };
        }
        private  CubeVertex[] calculateCubeTopVertices ()
        {
            return new[]
            {
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    bottomRight(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, -0.5f), 
                    topRight(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(0.5f, 0.5f, 0.5f), 
                    bottomRight(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, -0.5f), 
                    topLeft(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1])),
                new CubeVertex(new Vector3D<float>(-0.5f, 0.5f, 0.5f), 
                    bottomLeft(blockJson.texture[Face.TOP][0], blockJson.texture[Face.TOP][1]))
            };
        }

    }
}
