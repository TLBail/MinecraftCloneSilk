using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MinecraftCloneSilk.GameComponent;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace MinecraftCloneSilk
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Game game = Game.getInstance();
            game.Run();
        }
        
    }
}
