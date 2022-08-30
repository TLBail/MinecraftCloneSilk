using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.src.GameComponent
{
    public struct CubeVertex
    {
        Vector3D<float> position;
        Vector2D<float> texCoords;

        public CubeVertex(Vector3D<float> position, Vector2D<float> texCoords)
        {
            this.position = position;
            this.texCoords = texCoords;
        }

    }
}
