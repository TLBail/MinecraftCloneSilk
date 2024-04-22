using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using MinecraftCloneSilk.Core;
using Silk.NET.OpenGL;
using Shader = MinecraftCloneSilk.Core.Shader;
using Texture = MinecraftCloneSilk.Core.Texture;

namespace MinecraftCloneSilk.GameComponent.Components;

public class TextureRendererUi : Component
{


    private struct TexVertex
    {
        public Vector3 position;
        private Vector2 texCoord;

        public TexVertex(Vector3 vector3, Vector2 vector2) {
            position = vector3;
            texCoord = vector2;
        }
    }
    
    private Texture texture;
    private Shader shader;
    private VertexArrayObject<TexVertex, uint> vao;
    private BufferObject<TexVertex> vbo;
    private BufferObject<uint> ebo;

    private Transform transform;

    private TexVertex[] vertices = new TexVertex[4];
    private static readonly ImmutableArray<TexVertex> VERTICES_BASE = ImmutableArray.Create<TexVertex>(
        new (new Vector3(1f,  1f, 0.0f), new Vector2(1.0f, 1.0f)),  // top right
        new (new Vector3(1f, -1f, 0.0f), new Vector2(1.0f, 0.0f)),  // bottom right
        new (new Vector3(-1f, -1f, 0.0f), new Vector2(0.0f, 0.0f)),  // bottom left
        new (new Vector3(-1f,  1f, 0.0f), new Vector2(0.0f, 1.0f))   // top left
    );

    private uint[] indices =
    [ 
        3, 1, 0, 
        3, 2, 1 
    ]; 
    
    
    public TextureRendererUi(GameObject gameObject, Texture texture, Transform transform = default) : base(gameObject) {
        this.texture = texture;
        base.gameObject.game.uiDrawables += Draw;
        this.transform = transform;
        if (this.transform is null) this.transform = new();
    }

    private void UpdateData() {
        Matrix4x4 transformMatrix = transform.TransformMatrix;
        VERTICES_BASE.CopyTo(vertices);
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i].position = Vector3.Transform(vertices[i].position, transformMatrix);
        }
        
        vbo.SendData(vertices, 0);
    }


    public override void Start() {
        GL gl = gameObject.game.GetGl();
        shader = new Shader(gl, "./Shader/2dTexture/VertexShader.glsl", "./Shader/2dTexture/FragmentShader.glsl");
        ebo = new BufferObject<uint>(gl, indices, BufferTargetARB.ElementArrayBuffer);
        vbo = new BufferObject<TexVertex>(gl,4, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        vao = new VertexArrayObject<TexVertex, uint>(gl, vbo, ebo);
        
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0);
        vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, "texCoord");
        UpdateData();
        
    }


    private unsafe void Draw(GL gl, double deltatime) {
        vao.Bind();
        shader.Use();
        texture.Bind();
        shader.SetUniform("ourTexture", 0);
        gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, null);
    }


    public override void ToImGui() {
        ImGui.Text("TextureRendererUi");
        if(transform.ToImGui("ui")) UpdateData();
    }

    public override void Destroy() {
        base.Destroy();
        Dispose();
    }
    
    private void Dispose() {
        vao.Dispose();
        vbo.Dispose();
        ebo.Dispose();
        shader.Dispose();
    }

}