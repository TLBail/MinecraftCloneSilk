using Silk.NET.Input;
using System;
using System.Numerics;
using MinecraftCloneSilk.Collision;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using MinecraftCloneSilk.GameComponent;

namespace MinecraftCloneSilk.Core
{
    public class Camera
    {
        
        public Vector3 position { get; set; }
        public Vector3 Front { get; set; }

        public Vector3 up { get; private set; }
        public Vector3 Right => Vector3.Normalize(Vector3.Cross(Front, up));
        public float aspectRatio { get; set; }

        public float yaw { get; set; } = -90f;
        public float pitch { get; set; }
        
        public float nearPlane { get; set; } = 0.1f;
        public float farPlane { get; set; } = 1000f;

        public float zoom { get; private set; } = 45f;
        private Vector2 lastMousePosition;
        private bool isZoomActive = false;
        private IMouse? mouse;
        
        private Frustrum frustrum;
        
        public Camera(IWindow? window = null, IMouse? mouse = null) 
        {
            Setup(Vector3.Zero, Vector3.UnitZ * 1, Vector3.UnitY, 800f / 600f);
            this.frustrum = new Frustrum(this);
            
            if(window is null) return;
            Vector2D<int> size = window.GetFullSize();
            aspectRatio = (float)size.X / (float)size.Y;
            window.FramebufferResize += FrameBufferResize;
            mouse.Cursor.CursorMode = CursorMode.Normal;
            mouse.MouseMove += OnMouseMove;
            
        }
        public void SetZoomActive(bool active) {
            if(isZoomActive == active) return;
            if (active) {
                mouse!.Scroll += OnMouseWheel;
            } else {
                mouse!.Scroll -= OnMouseWheel;
            }
        }

        private void Setup(Vector3 position, Vector3 front, Vector3 up, float aspectRatio)
        {
            this.position = position;
            this.aspectRatio = aspectRatio;
            Front = front;
            this.up = up;
        }

        public Frustrum GetFrustrum() {
            UpdateFrustrum();
            return frustrum;
        }
        
        public void UpdateFrustrum() {
            frustrum.Update(this);
        }
        
        
        public void ModifyZoom(float zoomAmount)
        {
            //We don't want to be able to zoom in too close or too far away so clamp to these values
            zoom = Math.Clamp(zoom - zoomAmount, 1.0f, 45f);
        }

        public void ModifyDirection(float xOffset, float yOffset)
        {
            yaw += xOffset;
            pitch -= yOffset;

            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            pitch = Math.Clamp(pitch, -89f, 89f);

            var cameraDirection = Vector3.Zero;
            cameraDirection.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            cameraDirection.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            cameraDirection.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));

            Front = Vector3.Normalize(cameraDirection);
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(position, position + Front, up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(zoom), aspectRatio, nearPlane, farPlane);
        }

        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            var lookSensitivity = 0.1f;
            if (lastMousePosition == default) { lastMousePosition = position; }
            else
            {
                var xOffset = (position.X - lastMousePosition.X) * lookSensitivity;
                var yOffset = (position.Y - lastMousePosition.Y) * lookSensitivity;
                lastMousePosition = position;

                ModifyDirection(xOffset, yOffset);
            }
        }

        private void OnMouseWheel(IMouse mouse, ScrollWheel scrollWheel)
        {
            ModifyZoom(scrollWheel.Y);
        }

        private void FrameBufferResize(Vector2D<int> size)
        {
            aspectRatio = (float)size.X / (float)size.Y;
        }

    }
}
