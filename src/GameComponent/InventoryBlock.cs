using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class InventoryBlock
{
    public Block block { get; set; }
    public int quantity { get; set; }
    public Vector2D<int> position;

    public InventoryBlock(Block block, int quantity, Vector2D<int> position) {
        this.position = position;
        this.block = block;
        this.quantity = quantity;
    }
}