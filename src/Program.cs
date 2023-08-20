using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.Core;
using MinecraftCloneSilk.GameComponent;
using MinecraftCloneSilk.UI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Console = MinecraftCloneSilk.UI.Console;

namespace MinecraftCloneSilk
{
    public class Program
    {

        public static void Main(string[] args) {

            List<InitGameData> gameObjectNames = new List<InitGameData>()
            {
                new (typeof(Player).FullName),
                new (typeof(World).FullName, new object[]{ WorldMode.EMPTY}),
                new (typeof(Console).FullName),
                new (typeof(DebugRayManager).FullName),
                new (typeof(GameUi).FullName),
                new (typeof(GeneralInfo).FullName),
                new (typeof(ItemBarUi).FullName),
                new (typeof(DemoWindow).FullName),
                new (typeof(InventaireUi).FullName)
                
            };
            Scene scene = new Scene(gameObjectNames);
            Game game = Game.getInstance(scene);
            
            
            game.Run();
        }
        
        
        
    }
}
