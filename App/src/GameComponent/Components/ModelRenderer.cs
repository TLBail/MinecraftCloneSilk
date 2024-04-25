using Assimp;
using MinecraftCloneSilk.Core;
using Silk.NET.OpenGL;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Scene = MinecraftCloneSilk.ModelLoading.Scene;
using Shader = MinecraftCloneSilk.Core.Shader;

namespace MinecraftCloneSilk.GameComponent.Components;

public class ModelRenderer : Component
{

    private Scene scene;
    private string filePath;
    private Shader shader;
    public Transform transform;
    
    public ModelRenderer(GameObject gameObject, string filePath) : base(gameObject) {
        this.filePath = filePath;
        gameObject.game.drawables += Draw;
        transform = new Transform();
    }


    public override void Start() {
        base.Start();
        GL gl = gameObject.game.GetGl();
        
        
        shader = new Shader(gl, Generated.FilePathConstants.__Shader_3dPosNormColor.VertexShader_glsl,
            Generated.FilePathConstants.__Shader_3dPosNormColor.FragmentShader_glsl);

        scene = Scene.Load(gl,filePath, shader)!;
    }

    public override void ToImGui() {
        base.ToImGui();
        transform.ToImGui("m");
    }


    private void Draw(GL gl, double deltatime) {
        Matrix4x4 t = transform.TransformMatrix;
        scene.Draw(gl, t);
    }

    public override void Destroy() {
        base.Destroy();
        Dispose();
    }

    public void Dispose() {
        shader.Dispose();
    }
    
}