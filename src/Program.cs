using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.UI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace MinecraftCloneSilk
{
    public class Program
    {

        public static void Main(string[] args) {

            List<InitGameData> gameObjectNames = new List<InitGameData>()
            {
                new (typeof(Player).FullName),
                new (typeof(World).FullName, new object[]{ WorldMode.DYNAMIC}),
                new (typeof(DebugRayManagerUI).FullName),
                new (typeof(PlayerInteractionUI).FullName),
                new (typeof(GameUi).FullName),
                new (typeof(WorldGenerationUI).FullName),
                new (typeof(GeneralInfo).FullName)
            };
            Scene scene = new Scene(gameObjectNames);
            Game game = Game.getInstance(scene);
            
            
            game.Run();
        }
        
    }
}
