using System.Numerics;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.Model.WorldGen;
using MinecraftCloneSilk.UI;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk
{
    public class Program
    {

        public static void Main(string[] args) {
            Game game = Game.GetInstance(GetClassicScene());
            game.Run();
        }


        private static Scene GetClassicScene() {
            List<InitGameData> gameObjectNames = new List<InitGameData>()
            {
                new (typeof(Player).FullName!, new object[]{new Vector3(0.0f, 10f, 0.0f)}),
                new (typeof(World).FullName!, new object[]{new WorldNaturalGeneration(1234), WorldMode.SIMPLE }),
                new (typeof(StartingWindow).FullName!),
                new (typeof(GeneralInfo).FullName!),
                new (typeof(DemoWindow).FullName!),
            };
            Scene scene = new Scene(gameObjectNames, new OpenGlConfig(false, false));
            return scene;
        }

        private static Scene GetTestScene() {
            List<InitGameData> gameObjectNames = new List<InitGameData>()
            {
                new (typeof(Player).FullName!, new object[]{new Vector3(6f, 5f, 6f)}),
                new (typeof(World).FullName!, new object[]{new WorldFlatGeneration(), WorldMode.SIMPLE, "Worlds/testWorld" }),
                new (typeof(PauseMenu).FullName!),
                new (typeof(ItemBarUi).FullName!),
                new (typeof(GameUi).FullName!),
                new (typeof(InventaireUi).FullName!),
                new (typeof(ChunkRendererUi).FullName!),
                new (typeof(ConsoleUi).FullName!),
                new (typeof(GeneralInfo).FullName!),
            };
            Scene scene = new Scene(gameObjectNames, new OpenGlConfig(true, false));
            return scene;
        }
        
        
        
    }
}
