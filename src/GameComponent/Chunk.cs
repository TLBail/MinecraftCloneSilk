using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using MinecraftCloneSilk.Core;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;


namespace MinecraftCloneSilk.GameComponent
{
    public class Chunk
    {
        private Vector3 position;
        private Block[,,] blocks;

        private static readonly uint CHUNK_SIZE = 16;
        private static Shader cubeShader;
        private static Texture cubeTexture;

        private static Dictionary<string, TextureBlock> textureBlocks = new Dictionary<string, TextureBlock>();

        
        private BufferObject<CubeVertex> Vbo;
        private VertexArrayObject<CubeVertex, uint> Vao;
        private CubeVertex[] vertices;

        private GL Gl;
        
        public Chunk(Vector3 position)
        {
            this.position = position;
            blocks = new Block[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
            this.Gl = Game.getInstance().getGL();
            initStaticMembers();
            generateTerrain();
            fillChunkWithAirBlock();
            initBlocks();
            setOpenGl();
        }

        private void setOpenGl()
        {
            List<CubeVertex> listVertices = new List<CubeVertex>();
            foreach (Block block in blocks)
            {
                if(!block.airBlock) listVertices.AddRange(
                    textureBlocks[block.name].getCubeVertices(getFaces(block),
                        new Vector3D<float>(block.position.X, block.position.Y, block.position.Z)));
            }
            vertices = listVertices.ToArray();
            Vbo = new BufferObject<CubeVertex>(Gl, vertices, BufferTargetARB.ArrayBuffer);
            Vao = new VertexArrayObject<CubeVertex, uint>(Gl, Vbo);

            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
            Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");

        }

        private void initStaticMembers() {
            if (cubeShader == null)
            {
                cubeShader = new Shader(Gl, "./Shader/3dPosOneTextUni/VertexShader.hlsl", "./Shader/3dPosOneTextUni/FragmentShader.hlsl");
                cubeShader.Use();
                cubeShader.SetUniform("texture1", 0);
            }
            if (cubeTexture == null)
            {
                cubeTexture = new Texture(Gl, "./Assets/spriteSheet.png");
            }
        }

        private void fillChunkWithAirBlock()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if(blocks[x,y,z].Equals(default(Block))) blocks[x, y, z] = new Block(new Vector3D<int>(x, y, z));
                    }
                }
            }

        }

        private void generateTerrain()
        {

            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    blocks[x, 0, z] = new Block(new Vector3D<int>(x, 0, z), "grass", false);
                }
            }
            
            Vector3D<int>[] positions = new[]
            {
                new Vector3D<int>(0, 1, 0),
                new Vector3D<int>(2, 1, 0),
                new Vector3D<int>(4, 1, 0),
                new Vector3D<int>(0, 2, 0),
                new Vector3D<int>(0, 4, 0),
                new Vector3D<int>(0, 1, 2),
                new Vector3D<int>(0, 1, 4),
                new Vector3D<int>(2, 1, 2),
                new Vector3D<int>(4, 1, 4)
            };
            foreach (var position in positions)
            {
                blocks[(int)position.X, (int)position.Y, (int)position.Z] = new Block(position, "grass", false);
            }

            blocks[6, 2, 0] = new Block(new Vector3D<int>(6, 2, 0), "dirt", false);
            blocks[6, 2, 2] = new Block(new Vector3D<int>(6, 2, 2), "cobblestone", false);
            blocks[6, 2, 4] = new Block(new Vector3D<int>(6, 2, 4), "foliage", true);
            blocks[6, 2, 6] = new Block(new Vector3D<int>(6, 2, 6), "grass", false);
            blocks[6, 2, 8] = new Block(new Vector3D<int>(6, 2, 8), "metal", false);
            blocks[6, 2, 10] = new Block(new Vector3D<int>(6, 2, 10), "oak", false);
            blocks[6, 2, 12] = new Block(new Vector3D<int>(6, 2, 12), "stone", false);

        }

        private void initBlocks()
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        if(!blocks[x, y, z].airBlock) initCubes(x, y, z);
                    }
                }
            }
        }

        private void initCubes(int x, int y, int z)
        {
            Block block = blocks[x, y, z];
            if(getFaces(block).Length == 0) return;
            Face[] faces = getFaces(block);
            if (!textureBlocks.ContainsKey(block.name))
            {
                textureBlocks[block.name] = new TextureBlock(block.name);
            }
        }

        public void addBlock(int x, int y , int z, string name)
        {
            blocks[x, y, z] = new Block(new Vector3D<int>(x, y, z), name, false);
            //only if the cube is visible => faces not empty
            initCubes(x, y, z);
        }

        private Face[] getFaces(Block block)
        {
            if(block.transparent) return new [] { Face.TOP,Face.BOTTOM, Face.LEFT,Face.RIGHT, Face.FRONT,Face.BACK};
            List<Face> faces = new List<Face>();
            //X
            if (block.position.X > 0 &&
                blocks[block.position.X - 1, block.position.Y, block.position.Z].transparent)
                faces.Add(Face.RIGHT);
            if(block.position.X < CHUNK_SIZE - 1 && 
               blocks[block.position.X + 1, block.position.Y, block.position.Z].transparent)
                faces.Add(Face.LEFT);
            //Y
            if (block.position.Y > 0 &&
                blocks[block.position.X , block.position.Y - 1, block.position.Z].transparent)
                faces.Add(Face.BOTTOM);
            if(block.position.Y < CHUNK_SIZE - 1 && 
               blocks[block.position.X, block.position.Y  + 1, block.position.Z].transparent)
                faces.Add(Face.TOP);
            //Z
            if (block.position.Z > 0 &&
                blocks[block.position.X , block.position.Y, block.position.Z - 1].transparent)
                faces.Add(Face.BACK);
            if(block.position.Z < CHUNK_SIZE - 1 && 
               blocks[block.position.X, block.position.Y, block.position.Z + 1].transparent)
                faces.Add(Face.FRONT);

            
            
            return faces.ToArray();
        }



        public void Update(double deltaTime)
        {

        }

        
        public unsafe void Draw(GL Gl, double deltaTime)
        {
            Vao.Bind();
            cubeShader.Use();
            cubeTexture.Bind();

            Matrix4x4 model = Matrix4x4.Identity;
            model = Matrix4x4.CreateTranslation(position);
            cubeShader.SetUniform("model", model);


            Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertices.Length);
        }
    }
}
