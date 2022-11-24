using System.Numerics;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent;

public class Inventaire
{
    public const int INVENTORYSIZE = 24;
    public const int ITEMBARSIZE = 10;

    public const int STARTING_ITEM_BAR_INDEX = INVENTORYSIZE;
    public const int ENDING_ITEM_BAR_INDEX = INVENTORYSIZE + ITEMBARSIZE - 1;
    
    public InventoryBlock?[] inventoryBlocks { get; set; }
    private Player player;
    public int activeIndex;
    
    
    public Inventaire(Player player) {
        inventoryBlocks = new InventoryBlock[INVENTORYSIZE + ITEMBARSIZE];
        this.player = player;
        int x = 0;
        foreach (var keyValuePair in BlockFactory.getInstance().blocksReadOnly) {
            inventoryBlocks[x] = new InventoryBlock(keyValuePair.Value, 1, new Vector2D<int>(x, x));
            x = (x + 1) % INVENTORYSIZE;
        }
        
    }

    public bool haveBlockToPlace() {
        return inventoryBlocks[activeIndex] != null;
    }

    public InventoryBlock getActiveBlock() {
        return inventoryBlocks[activeIndex];
    }
    
    public void moveActiveIndexByScroolOffset(float offset) {
        if (offset > 0) {
            activeIndex--;
        } else {
            activeIndex++;
        }

        if (activeIndex >= INVENTORYSIZE + ITEMBARSIZE) activeIndex = INVENTORYSIZE;
        if (activeIndex < INVENTORYSIZE) activeIndex = INVENTORYSIZE + ITEMBARSIZE - 1;
    }

    public Span<InventoryBlock?> getInventoryBlocksFromItemBar() =>
        new Span<InventoryBlock?>(inventoryBlocks, INVENTORYSIZE, INVENTORYSIZE + ITEMBARSIZE);
    
    public InventoryBlock get(int x) {
        if (x >= INVENTORYSIZE  || x < 0 )
            throw new GameException(player, "trying to access inventory with wrong position");
        return inventoryBlocks[x];
    }
    
    
    
}