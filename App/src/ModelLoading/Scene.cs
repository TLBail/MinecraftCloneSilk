using Assimp;
using Silk.NET.OpenGL;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Shader = MinecraftCloneSilk.Core.Shader;

namespace MinecraftCloneSilk.ModelLoading;

public class Scene : IDisposable
{
    public static Scene? Load(GL gl,String filePath, Shader shader) {
        AssimpContext context = new AssimpContext();
        Assimp.Scene assimpscene = context.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);
        if (assimpscene is null) return null;
        if (assimpscene.RootNode is null) return null;
        Scene scene = new Scene(assimpscene, gl, shader);
        return scene;
    }

    public Assimp.Scene assimpScene;

    public Mesh[] meshes;

    public List<Model> models;
    public Shader shader;

    public Scene(Assimp.Scene assimpScene, GL gl, Shader shader) {
        this.assimpScene = assimpScene;
        this.shader = shader;
        this.models = new();

        foreach (EmbeddedTexture assimpTexture in this.assimpScene.Textures) {
            //Todo load textures
        }

        meshes = new Mesh[this.assimpScene.MeshCount];
        for (int i = 0; i < assimpScene.MeshCount; i++) {
            meshes[i] = new Mesh(gl, this.assimpScene.Meshes[i]);
        }
       
        LoadNode(this.assimpScene.RootNode);
    }

    private void LoadNode(Node node) {
        foreach (int meshIndex in node.MeshIndices) {
            models.Add(new Model(meshes[meshIndex], new(), node.Transform, shader));
        }
        
        foreach(Node children in node.Children)
        {
            LoadNode(children);
        }
    }
    public void Draw(GL gl, Matrix4x4 t) {
        foreach (Model model in models) {
            model.Draw(gl, t);
        }
    }


    public void Dispose() {
        foreach(Mesh mesh in meshes) {
            mesh.Dispose();
        }
    }
}