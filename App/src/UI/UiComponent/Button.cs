using System.Numerics;
using ImGuiNET;

namespace MinecraftCloneSilk.UI.UiComponent;

public class Button
{
    public record ButtonStyle(
        Vector4? ButtonColor,
        Vector4? ButtonHoveredColor,
        Vector4? ButtonActiveColor,
        Vector4? TextColor);
    
    public Vector4? ButtonColor;
    public Vector4? ButtonHoveredColor;
    public Vector4? ButtonActiveColor;
    public Vector4? TextColor;

    public string label;

    public Button(string label, ButtonStyle? style = null) {    
        this.label = label;
        if (style is not null) {
            this.ButtonColor = style.ButtonColor;
            this.ButtonHoveredColor = style.ButtonHoveredColor;
            this.ButtonActiveColor = style.ButtonActiveColor;
            this.TextColor = style.TextColor;
        }
    }

    public bool Draw(Vector2 position, Vector2 size) {
        int nbStyle = 0;
        nbStyle += PushStyleColor(ImGuiCol.Button, ButtonColor);
        nbStyle += PushStyleColor(ImGuiCol.ButtonHovered, ButtonHoveredColor);
        nbStyle += PushStyleColor(ImGuiCol.ButtonActive, ButtonActiveColor);
        nbStyle += PushStyleColor(ImGuiCol.Text, TextColor);
        ImGui.SetCursorPosX(position.X);
        ImGui.SetCursorPosY(position.Y);
        bool isClick = ImGui.Button(label, size);
        ImGui.PopStyleColor(nbStyle);
        return isClick;
    }
    private int PushStyleColor(ImGuiCol colorType, Vector4? color) {
        if(color is not null) {
            ImGui.PushStyleColor(colorType, color.Value);
            return 1;
        }
        return 0;
    }
    
    
}