using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.UI;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk
{
    public class Program
    {

        public static void Main(string[] args) {

            List<InitGameData> gameObjectNames = new List<InitGameData>()
            {
                new (typeof(Player).FullName!),
                new (typeof(World).FullName!, new object[]{ WorldMode.DYNAMIC, "Worlds/newWorld"}),
                new (typeof(Console).FullName!),
                new (typeof(DebugRayManager).FullName!),
                new (typeof(GameUi).FullName!),
                new (typeof(GeneralInfo).FullName!),
                new (typeof(ItemBarUi).FullName!),
                new (typeof(DemoWindow).FullName!),
                new (typeof(InventaireUi).FullName!),
                new (typeof(ChunkRendererUi).FullName!)
                
            };
            Scene scene = new Scene(gameObjectNames);
            Game game = Game.GetInstance(scene);
            game.Run();
        }
        
        
        
    }
}
