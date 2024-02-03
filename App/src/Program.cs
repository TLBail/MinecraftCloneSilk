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
            List<InitGameData> gameObjectNames = new List<InitGameData>()
            {
                new (typeof(Player).FullName!, new object[]{new Vector3(0.0f, 10f, 0.0f)}),
                new (typeof(World).FullName!, new object[]{new WorldNaturalGeneration(1234), WorldMode.SIMPLE }),
                new (typeof(StartingWindow).FullName!),
                new (typeof(GeneralInfo).FullName!),
                new (typeof(DemoWindow).FullName!),
            };
            Scene scene = new Scene(gameObjectNames, new OpenGlConfig(true, false));
            Game game = Game.GetInstance(scene);
            game.Run();
        }
        
        
        
    }
}
