﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using Silk.NET.Maths;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.Core;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.GameComponent
{
    public class Cube
    {
        
        private static Shader cubeShader;

        private BufferObject<CubeVertex> Vbo;
        private VertexArrayObject<CubeVertex, uint> VaoCube;
        private uint vbo;
        private uint vao;
        private static Texture cubeTexture;
        private static Dictionary<string, TextureBlock> textureBlocks = new Dictionary<string, TextureBlock>();
        private uint nbVertex;
    
        
        
        public unsafe Cube(GL Gl, string name, Face[] faces, Vector3D<float> blockPosition)
        {
            this.nbVertex = (uint)(faces.Length * 6);
            initStaticMembers(Gl, name);
            TextureBlock textureBlock = textureBlocks[name];
            
            
            Vbo = new BufferObject<CubeVertex>(Gl, textureBlock.getCubeVertices(faces, blockPosition), BufferTargetARB.ArrayBuffer);
            VaoCube = new VertexArrayObject<CubeVertex, uint>(Gl, Vbo);

            VaoCube.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
            VaoCube.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoords");
            
            cubeShader.Use();
            cubeShader.SetUniform("texture1", 0);
        }


        private void initStaticMembers(GL Gl, string name)
        {
            if (!textureBlocks.ContainsKey(name))
            {
                textureBlocks[name] = new TextureBlock(name);
            }
            if (cubeShader == null)
            {
                cubeShader = new Shader(Gl, "./Shader/3dPosOneTextUni/VertexShader.hlsl", "./Shader/3dPosOneTextUni/FragmentShader.hlsl");
            }

            if (cubeTexture == null)
            {
                cubeTexture = new Texture(Gl, "./Assets/spriteSheet.png");
            }
        }

        public unsafe void Draw(GL Gl, double deltaTime, Vector3 chunkPosition)
        {
            VaoCube.Bind();
            cubeShader.Use();
            cubeTexture.Bind();

            Matrix4x4 model = Matrix4x4.Identity;
            model = Matrix4x4.CreateTranslation(chunkPosition);
            cubeShader.SetUniform("model", model);


            Gl.DrawArrays(PrimitiveType.Triangles, 0, nbVertex);
        }

        
        
        
    }
}
