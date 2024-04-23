using System.Numerics;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;

namespace MinecraftCloneSilk.ModelLoading;

public class Model
{
    Mesh mesh;
    List<Texture> textures;
    Matrix4x4 transform;
    Shader shader;
    
    public Model(Mesh mesh, List<Texture> textures, Assimp.Matrix4x4 transform,Shader shader) {
        this.mesh = mesh;
        this.textures = textures;
        this.transform = new Matrix4x4(
            transform.A1, transform.A2, transform.A3, transform.A4,
            transform.B1, transform.B2, transform.B3, transform.B4,
            transform.C1, transform.C2, transform.C3, transform.C4,
            transform.D1, transform.D2, transform.D3, transform.D4
        );
        this.shader = shader;
    }
    
    
    public unsafe void Draw(GL gl, Matrix4x4 t) {
        mesh.vao.Bind();
        shader.Use();
        shader.SetUniform("model", transform + t);
        
        gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.indices.Length, DrawElementsType.UnsignedInt, null);
    } 
    
}