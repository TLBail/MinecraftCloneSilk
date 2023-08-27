using Silk.NET.OpenGL;
using System;
using Microsoft.VisualBasic.CompilerServices;
using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Core
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        public uint handle { get; private set; }
        private BufferTargetARB bufferType;
        private GL gl;
        private bool disposed = false;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType, BufferUsageARB usage  = BufferUsageARB.StaticDraw)
        {
            this.gl = gl;
            this.bufferType = bufferType;

    
            handle = this.gl.GenBuffer();
            Bind(bufferType);
            fixed (void* d = data)
            {
                this.gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, usage);
            }
        }

        public unsafe BufferObject(GL gl, int nbVertex, BufferTargetARB bufferType, BufferUsageARB bufferUsageArb = BufferUsageARB.DynamicCopy)
        {
            this.gl = gl;
            this.bufferType = bufferType;

            handle = gl.GenBuffer();
            Bind(bufferType);
            gl.BufferData(bufferType, (nuint)(nbVertex * sizeof(TDataType)), null, bufferUsageArb);
        }

        public void SendData(ReadOnlySpan<TDataType> data, nint offset)
        {
            Bind(bufferType);
            gl.BufferSubData(bufferType, offset,  data);
        }
        
        
        public void Bind(BufferTargetARB bufferType)
        {
            gl.BindBuffer(bufferType, handle);
        }
        
        public unsafe TDataType GetData()  {
            gl.GetNamedBufferSubData<TDataType>(handle, 0, (uint)sizeof(TDataType), out var countCompute);
            return countCompute;
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
