using System.Numerics;
using ImGuiNET;

namespace MinecraftCloneSilk.Core
{
    public class Transform
    {

        public Vector3 Position { get; set; } 

        public Vector3 Scale { get; set; }

        public Quaternion Rotation { get; set; }
        
        public Matrix4x4 TransformMatrix =>  Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position);
        
        /**
         * @brief Constructor of the Transform class
         * @param position the position of the transform (default is 0)
         * @param scale the scale of the transform (default is 1)
         * @param rotation the rotation of the transform (default is 0)
         */
        public Transform(Vector3 position = default, Vector3? scale = null, Quaternion rotation = default) {
            Position = position;
            if (scale is not null) {
                Scale = (Vector3)scale;
            } else {
                Scale = Vector3.One;
            }
            Rotation = rotation;
        }
        
        
        /**
         * @brief display the transform in the ImGui window
         * @return true if the transform has been modified
         * @return false if the transform has not been modified
         */
        public bool ToImGui(string labelprefix) {
            Vector3 newPosition = Position;
            Vector3 newScale = Scale;
            Vector3 newRotation = Euler.FromQuaternion(Rotation);
            bool modified = false;
            ImGui.Text("Position");
            ImGui.DragFloat(labelprefix + "p.x", ref newPosition.X, 0.01f);
            ImGui.SameLine();
            ImGui.DragFloat(labelprefix + "p.y", ref newPosition.Y, 0.01f);
            ImGui.SameLine();
            ImGui.DragFloat(labelprefix + "p.z", ref newPosition.Z, 0.01f);
        
            if(newPosition != Position) {
                Position = newPosition;
                modified = true;    
            }
            
            ImGui.Text("Scale");
            ImGui.DragFloat(labelprefix + "s.x", ref newScale.X, 0.01f);
            ImGui.SameLine();
            ImGui.DragFloat(labelprefix + "s.y", ref newScale.Y, 0.01f);
            ImGui.SameLine();
            ImGui.DragFloat(labelprefix + "s.z", ref newScale.Z, 0.01f);
            
            if(newScale != Scale) {
                Scale = newScale;
                modified = true;    
            }
            
            ImGui.Text("Rotation");
            ImGui.DragFloat(labelprefix + "r.x", ref newRotation.X, 0.01f);
            ImGui.SameLine();
            ImGui.DragFloat(labelprefix + "r.y", ref newRotation.Y, 0.01f);
            ImGui.SameLine();
            ImGui.DragFloat(labelprefix + "r.z", ref newRotation.Z, 0.01f);

            Quaternion newQuaternion = Euler.ToQuaternion(newRotation);
            if(newQuaternion != Rotation) {
                Rotation = newQuaternion;
                modified = true;    
            }
            return modified;
        }
    }
}
