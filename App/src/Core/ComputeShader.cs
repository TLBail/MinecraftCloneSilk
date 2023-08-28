using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace MinecraftCloneSilk.Core;

public class ComputeShader :  IDisposable
{
    public uint handle { get; private set; }
    private GL gl;
    private bool disposed = false;

    public ComputeShader(GL gl, string computePath) {
        this.gl = gl;

        uint compute = LoadShader(ShaderType.ComputeShader, computePath);

        handle = gl.CreateProgram();
        gl.AttachShader(handle, compute);
        gl.LinkProgram(handle);
        gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
        if (status == 0) {
            throw new Exception($"Program failed to link with error: {gl.GetProgramInfoLog(handle)}");
        }
        
        gl.DetachShader(handle, compute);
        gl.DeleteShader(compute);
        
    }
    
    public void Use() {
        gl.UseProgram(handle);
    }
    
    private uint LoadShader(ShaderType type, string path) {
        string src = File.ReadAllText(path);
        uint handle = gl.CreateShader(type);
        gl.ShaderSource(handle, src);
        gl.CompileShader(handle);
        string infoLog = gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog)) {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }

    public uint getUniformBlockIndex(string name)
    {
        return gl.GetUniformBlockIndex(handle, name);
    }
   
    public void SetUniform(string name, int value) {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1) {
            throw new Exception($"{name} uniform not found on shader.");
        }

        gl.Uniform1(location, value);
    }
    
    public void SetUniform(string name, float value) {
        int location =gl.GetUniformLocation(handle, name);
        if (location == -1) {
            throw new Exception($"{name} uniform not found on shader.");
        }
        gl.Uniform1(location, value);
    }
    
    
    public void SetUniform(string name, Vector3 value) {
        int location = gl.GetUniformLocation(handle, name);
        if (location == -1) {
            throw new Exception($"{name} uniform not found on shader.");
        }

        gl.Uniform3(location, value.X, value.Y, value.Z);
    }

    ~ComputeShader() {
        Dispose(false);
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    public void Dispose(bool disposing) {
        if (!disposed) {
            if (disposing) {
                gl.DeleteProgram(handle);
            }

            disposed = true;
        }
    }
}