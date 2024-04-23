using Assimp;
using MinecraftCloneSilk.Core;
using Silk.NET.OpenGL;
using PrimitiveType = Silk.NET.OpenGL.PrimitiveType;

namespace MinecraftCloneSilk.ModelLoading;

public class Mesh
{
    public Assimp.Mesh mesh;
    public VertexArrayObject<Vertex, uint> vao;
    public BufferObject<Vertex> vbo;
    public BufferObject<uint> ebo;

    public Vertex[] vertices;
    public uint[] indices;
    
    public Mesh(GL gl,Assimp.Mesh mesh) {
        this.mesh = mesh;
        SetupMesh(gl);
    }
  
    
    private void SetupMesh(GL gl) {
        vertices = new Vertex[mesh.VertexCount];
        indices = new uint[mesh.FaceCount * 3];
        
        CreateVertexBuffer();
        
        vbo = new BufferObject<Vertex>(gl,vertices, BufferTargetARB.ArrayBuffer);
        ebo = new BufferObject<uint>(gl,indices, BufferTargetARB.ElementArrayBuffer);
        vao = new VertexArrayObject<Vertex, uint>(gl, vbo, ebo);
        
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, "normal");
        vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, "texCoords");
    }

    private void CreateVertexBuffer() {
        List<Vector3D> verts = mesh.Vertices;
        List<Vector3D>? norms = (mesh.HasNormals) ? mesh.Normals : null;
        List<Vector3D>? uvs = mesh.HasTextureCoords(0) ? mesh.TextureCoordinateChannels[0] : null;
        for(int i = 0; i < verts.Count; i++)
        {
            Vector3D pos = verts[i];
            Vector3D norm = (norms != null) ? norms[i] : new Vector3D(0, 0, 0);
            Vector3D uv = (uvs != null) ? uvs[i] : new Vector3D(0, 0, 0);

            vertices[i] = new Vertex(pos, norm, new Vector2D(uv.X, uv.Y)); 
        }

        List<Face> faces = mesh.Faces;
        int iIndex = 0;
        for(int i = 0; i < faces.Count; i++)
        {
            Face f = faces[i];

            //Ignore non-triangle faces
            if(f.IndexCount != 3)
            {
                indices[iIndex++] = 0;
                indices[iIndex++] = 0;
                indices[iIndex++] = 0;
                continue;
            }

            indices[iIndex++] = (uint) (f.Indices[0]);
            indices[iIndex++] = (uint) (f.Indices[1]);
            indices[iIndex++] = (uint) (f.Indices[2]);
        }
    }
    
}