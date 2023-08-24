using Silk.NET.OpenGL;
using System;
using Microsoft.VisualBasic.CompilerServices;
using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Core
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private uint handle;
        private BufferTargetARB bufferType;
        private GL gl;
        private bool disposed = false;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            this.gl = gl;
            this.bufferType = bufferType;

    
            handle = this.gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                this.gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public unsafe BufferObject(GL gl, int nbVertex, BufferTargetARB bufferType)
        {
            this.gl = gl;
            this.bufferType = bufferType;

            handle = gl.GenBuffer();
            Bind();
            gl.BufferData(bufferType, (nuint)(nbVertex * sizeof(TDataType)), null, BufferUsageARB.DynamicDraw);
        }

        public void SendData(ReadOnlySpan<TDataType> data, nint offset)
        {
            gl.BufferSubData(bufferType, offset,  data);
        }
        
        
        public void Bind()
        {
            gl.BindBuffer(bufferType, handle);
        }

        ~BufferObject() {
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
                    gl.DeleteBuffer(handle);
                }
                disposed = true;
            }            
        }
    }

}
