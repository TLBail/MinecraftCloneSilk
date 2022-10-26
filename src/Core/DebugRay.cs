using System.Numerics;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Core;

public class DebugRay
{

    private static readonly Vector3D<float> DEFAULT_COLOR = new Vector3D<float>(1.0f, 0, 0);

    private BufferObject<DebugRayVertex> Vbo;
    private VertexArrayObject<DebugRayVertex, uint> Vao;

    
    private Vector3D<float> start;
    private Vector3D<float> end;

    private static Shader rayShader;
    private GL Gl;
    private Game game;
    
    public DebugRay(Vector3D<float> start, Vector3D<float> end, Vector3D<float> color)
    : this(start, end, color, color) {    }

    public DebugRay(Vector3D<float> start, Vector3D<float> end, Vector3D<float> startColor, Vector3D<float> endColor)
    {
        this.start = start;
        this.end = end;
        game = Game.getInstance();
        Gl = game.getGL();
        game.drawables += Drawables;

        DebugRayVertex[] vertices = new[]
        {
            new DebugRayVertex(start, startColor),
            new DebugRayVertex(end, endColor),
        };

        Vbo = new BufferObject<DebugRayVertex>(Gl, vertices, BufferTargetARB.ArrayBuffer);
        Vao = new VertexArrayObject<DebugRayVertex, uint>(Gl, Vbo);
        
        Vao.Bind();
        Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, "position");
        Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, "color");

        if (rayShader == null) {
            rayShader = new Shader(Gl, "./Shader/3dPosOneColorUni/VertexShader.hlsl",
                "./Shader/3dPosOneColorUni/FragmentShader.hlsl");
        }

    }
    
    
    public DebugRay(Vector3D<float> start, Vector3D<float> end) : this(start,  end, DEFAULT_COLOR){}

    public void remove()
    {
        game.drawables -= Drawables;
        Vao.Dispose();
        Vbo.Dispose();
    }
    
    private void Drawables(GL gl, double deltatime)
    {
        Vao.Bind();
        rayShader.Use();
        
        
        var model = Matrix4x4.Identity;
        model = Matrix4x4.CreateTranslation(new Vector3(0,0,0));
        rayShader.SetUniform("model", model);
        
        
        Gl.DrawArrays(PrimitiveType.Lines, 0, 2);
    }
}