using Silk.NET.OpenGL;
using System;
using System.Runtime.InteropServices;
using MinecraftCloneSilk.GameComponent;


namespace MinecraftCloneSilk.Core
{
    public class VertexArrayObject<TVertexType, TIndexType> : IDisposable
        where TVertexType : unmanaged
        where TIndexType : unmanaged
    {
        private uint handle;
        private GL gl;
        private bool disposed = false;

        public VertexArrayObject(GL gl, BufferObject<TVertexType> vbo, BufferObject<TIndexType>? ebo = null)
        {
            this.gl = gl;

            handle = this.gl.GenVertexArray();
            Bind();
            vbo.Bind(BufferTargetARB.ArrayBuffer);
            ebo?.Bind(BufferTargetARB.ElementArrayBuffer);
        }

        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, string fieldName)
        {
            gl.VertexAttribPointer(index, count, type, false,(uint) sizeof(TVertexType), (void*) (Marshal.OffsetOf(typeof(TVertexType), fieldName)));
            gl.EnableVertexAttribArray(index);
        }

        public void Bind()
        {
            gl.BindVertexArray(handle);
        }

        ~VertexArrayObject() {
            Dispose(false);
        }

        protected void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    gl.DeleteVertexArray(handle);
                }
                disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
