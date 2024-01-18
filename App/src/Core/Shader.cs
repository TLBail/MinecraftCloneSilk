using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using MinecraftCloneSilk.GameComponent;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneSilk.Core
{
    public class Shader : IDisposable
    {
        private uint handle;
        private GL gl;
        private bool disposed = false;
        private Dictionary<string, int> uniformLocations = new Dictionary<string, int>();
        
        public Shader(GL gl, string vertexPath, string fragmentPath)
        {
            this.gl = gl;

            uint vertex = LoadShader(ShaderType.VertexShader, vertexPath);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentPath);
            handle = this.gl.CreateProgram();
            this.gl.AttachShader(handle, vertex);
            this.gl.AttachShader(handle, fragment);
            this.gl.LinkProgram(handle);
            this.gl.GetProgram(handle, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                throw new Exception($"Program failed to link with error: {this.gl.GetProgramInfoLog(handle)}");
            }
            this.gl.DetachShader(handle, vertex);
            this.gl.DetachShader(handle, fragment);
            this.gl.DeleteShader(vertex);
            this.gl.DeleteShader(fragment);
        }

        public void Use()
        {
            gl.UseProgram(handle);
        }

        public uint GetUniformBlockIndex(string name)
        {
            return gl.GetUniformBlockIndex(handle, name);
        }

        public void SetUniform(string name, int value) => gl.Uniform1(GetUniformLocation(name), value);

        public int GetUniformLocation(string name) {
            if (!uniformLocations.ContainsKey(name)) {
                int location = gl.GetUniformLocation(handle, name);
                if (location == -1)
                {
                    throw new Exception($"{name} uniform not found on shader.");
                }
                uniformLocations.Add(name, gl.GetUniformLocation(handle, name));
            }
            return uniformLocations[name];
        }

        public unsafe void SetUniform(string name, Matrix4x4 value) =>
            gl.UniformMatrix4(GetUniformLocation(name), 1, false, (float*)&value);

        public void SetUniform(string name, float value) => gl.Uniform1(GetUniformLocation(name), value);

        public void SetUniform(string name, Vector3 value) => gl.Uniform3(GetUniformLocation(name), value.X, value.Y, value.Z);

        ~Shader() {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    gl.DeleteProgram(handle);
                }
                disposed = true;
            }
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = File.ReadAllText(path);
            uint handle = gl.CreateShader(type);
            gl.ShaderSource(handle, src);
            gl.CompileShader(handle);
            string infoLog = gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
            {
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
            }

            return handle;
        }
    }
}
