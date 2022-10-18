using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MinecraftCloneSilk.src.GameComponent
{
    public struct Block
    {
        public Vector3 position;
        public string name = "air";
        public bool airBlock = true;
        public bool transparent = true;
        public Cube? cube;
        
        public Block(Vector3 position, string name, bool transparent)
        {
            airBlock = name.Equals("air"); 
            this.position = position;
            this.name = name;
            this.transparent = transparent;
            cube = null;
        }
        

    }
}
