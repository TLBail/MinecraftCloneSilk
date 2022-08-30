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
        private static readonly float[] Vertices =
        {
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
             0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
        };

        private static readonly CubeVertex[] CubeVertices =
        {
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(1.0f, 0.0f)  ),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 0.0f)),
         
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(0.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(0.0f, 0.0f)),
            
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(0.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(0.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f,  0.5f),new Vector2D<float>(0.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f, -0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(1.0f, 1.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(1.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f,  0.5f),new Vector2D<float>(0.0f, 0.0f)),
            new CubeVertex(new Vector3D<float>(0.5f,  0.5f, -0.5f),new Vector2D<float>(0.0f, 1.0f)),
        };



        private static Shader cubeShader;

        //private BufferObject<CubeVertex> Vbo;
        //private VertexArrayObject<CubeVertex, uint> VaoCube;
        private uint vbo;
        private uint vao;
        private Texture cubeTexture;


        public unsafe Cube(GL Gl, string name, Face[] faces)
        {
            Game game = Game.getInstance();
            game.drawables += Draw;
            game.disposables += Dispose;


            //Vbo = new BufferObject<CubeVertex>(Gl, CubeVertices, BufferTargetARB.ArrayBuffer);
            //VaoCube = new VertexArrayObject<CubeVertex, uint>(Gl, Vbo);

            vbo = Gl.GenBuffer();
            vao = Gl.GenBuffer();

            Gl.BindVertexArray(vao);
            Gl.BindBuffer(GLEnum.ArrayBuffer, vbo);

            Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(6 * sizeof(CubeVertex)), null, GLEnum.DynamicDraw);

            //VaoCube.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 1, 0);
            //VaoCube.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 1, Marshal.OffsetOf(typeof(CubeVertex), "texCoords").ToInt32());

            Gl.EnableVertexAttribArray(0);
            Gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(CubeVertex), (void*)0);
            Gl.EnableVertexAttribArray(1);
            Gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)sizeof(CubeVertex), (void*)Marshal.OffsetOf<CubeVertex>("texCoords"));

            

            Gl.BindVertexArray(0);





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

        public unsafe void Draw(GL Gl, double deltaTime)
        {
            Gl.BindVertexArray(vao);
            cubeShader.Use();
            cubeTexture.Bind();

            Gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            Span<CubeVertex> data = new Span<CubeVertex>(CubeVertices);
            fixed (void* d = data)
            {
                Gl.BufferSubData(GLEnum.ArrayBuffer, 0, (uint)(36 * sizeof(CubeVertex)), d);
            }


            cubeShader.SetUniform("model", Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(25f)));


            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        public void Dispose()
        {

        }

    }
}
