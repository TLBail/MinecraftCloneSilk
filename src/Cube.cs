using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
namespace MinecraftCloneSilk.src
{
    public class Cube
    {
        private static readonly float[] Vertices =
        {
            //X    Y      Z
            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,

            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f, -0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,

             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,

            -0.5f, -0.5f, -0.5f,
             0.5f, -0.5f, -0.5f,
             0.5f, -0.5f,  0.5f,
             0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f,  0.5f,
            -0.5f, -0.5f, -0.5f,

            -0.5f,  0.5f, -0.5f,
             0.5f,  0.5f, -0.5f,
             0.5f,  0.5f,  0.5f,
             0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f,  0.5f,
            -0.5f,  0.5f, -0.5f
        };

        private static readonly uint[] Indices =
        {
            0, 1, 3,
            1, 2, 3
        };

        private static Shader lightingShader;
        private static Shader lampShader;

        private BufferObject<float> Vbo;
        private BufferObject<uint> Ebo;
        private VertexArrayObject<float, uint> VaoCube;



        public Cube(GL Gl)
        {
            Game game = Game.getInstance();
            game.drawables += Draw;


            Ebo = new BufferObject<uint>(Gl, Indices, BufferTargetARB.ElementArrayBuffer);
            Vbo = new BufferObject<float>(Gl, Vertices, BufferTargetARB.ArrayBuffer);
            VaoCube = new VertexArrayObject<float, uint>(Gl, Vbo, Ebo);

            VaoCube.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 3, 0);

            //The lighting shader will give our main cube its colour multiplied by the lights intensity
            if(lightingShader == null)
            {
                lightingShader = new Shader(Gl, "./Shader/shader.vert", "./Shader/lighting.frag");

            }
            if(lampShader == null)
            {
                //The Lamp shader uses a fragment shader that just colours it solid white so that we know it is the light source
                lampShader = new Shader(Gl, "./Shader/shader.vert", "./Shader/shader.frag");
            }

        }

        public void Draw(GL Gl,Camera mainCamera, double deltaTime)
        {
            VaoCube.Bind();
            lightingShader.Use();

            //Slightly rotate the cube to give it an angled face to look at
            lightingShader.SetUniform("uModel", Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(25f)));
            lightingShader.SetUniform("uView", mainCamera.GetViewMatrix());
            lightingShader.SetUniform("uProjection", mainCamera.GetProjectionMatrix());
            lightingShader.SetUniform("objectColor", new Vector3(1.0f, 0.5f, 0.31f));
            lightingShader.SetUniform("lightColor", Vector3.One);

            //We're drawing with just vertices and no indicies, and it takes 36 verticies to have a six-sided textured cube
            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);

            lampShader.Use();

            //The Lamp cube is going to be a scaled down version of the normal cubes verticies moved to a different screen location
            var lampMatrix = Matrix4x4.Identity;
            lampMatrix *= Matrix4x4.CreateScale(0.2f);
            lampMatrix *= Matrix4x4.CreateTranslation(new Vector3(1.2f, 1.0f, 2.0f));

            lampShader.SetUniform("uModel", lampMatrix);
            lampShader.SetUniform("uView", mainCamera.GetViewMatrix());
            lampShader.SetUniform("uProjection", mainCamera.GetProjectionMatrix());

            Gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

    }
}
