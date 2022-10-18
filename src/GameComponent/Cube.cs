using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.src.Core;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Runtime.InteropServices;
namespace MinecraftCloneSilk.src.GameComponent
{
    public class Cube
    {
        
        private static Shader cubeShader;

        private BufferObject<CubeVertex> Vbo;
        private VertexArrayObject<CubeVertex, uint> VaoCube;
        private uint vbo;
        private uint vao;
        private static Texture cubeTexture;
        private TextureBlock textureBlock;

        public unsafe Cube(GL Gl, string name, Face[] faces)
        {
            Game game = Game.getInstance();
            game.disposables += Dispose;

            textureBlock = new TextureBlock("./Assets/blocks/json/dirt.json");
    
            
            
            CubeVertex[] cubeVertices = new CubeVertex[36];
            int index = 0;
            foreach(CubeVertex vertices in textureBlock.CubeBackVertices())
            {
                cubeVertices[index] = vertices;
                index++;
            }
            
            foreach(CubeVertex vertices in textureBlock.CubeFrontVertices())
            {
                cubeVertices[index] = vertices;
                index++;
            }
            
            foreach(CubeVertex vertices in textureBlock.CubeRightVertices())
            {
                cubeVertices[index] = vertices;
                index++;
            }
            
            foreach(CubeVertex vertices in textureBlock.CubeLeftVertices())
            {
                cubeVertices[index] = vertices;
                index++;
            }
            
            foreach(CubeVertex vertices in textureBlock.CubeBottomVertices())
            {
                cubeVertices[index] = vertices;
                index++;
            }

            foreach(CubeVertex vertices in textureBlock.CubeTopVertices())
            {
                cubeVertices[index] = vertices;
                index++;
            }




            Vbo = new BufferObject<CubeVertex>(Gl, cubeVertices, BufferTargetARB.ArrayBuffer);
            VaoCube = new VertexArrayObject<CubeVertex, uint>(Gl, Vbo);

            VaoCube.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
            VaoCube.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");




            //The lighting shader will give our main cube its colour multiplied by the lights intensity
            if (cubeShader == null)
            {
                cubeShader = new Shader(Gl, "./Shader/3dPosOneTextUni/VertexShader.hlsl", "./Shader/3dPosOneTextUni/FragmentShader.hlsl");
            }

            if (cubeTexture == null)
            {
                cubeTexture = new Texture(Gl, "./Assets/spriteSheet.png");
            }
            
            cubeShader.Use();
            cubeShader.SetUniform("texture1", 0);
        }

        public unsafe void Draw(GL Gl, double deltaTime, Vector3 position)
        {
            //Gl.BindVertexArray(vao);
            VaoCube.Bind();
            cubeShader.Use();
            cubeTexture.Bind();

            Matrix4x4 model = Matrix4x4.Identity;
            model = Matrix4x4.CreateTranslation(position);
            cubeShader.SetUniform("model", model);


            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        public void Dispose()
        {

        }
        
        
        
    }
}
