using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.GameComponent
{
	public enum Face
	{
		TOP = 0,
		BOTTOM = 1,
		LEFT = 2,
		RIGHT = 3,
		FRONT = 4,
		BACK = 5
		
	};

	public static class FaceOffset
	{
		public static Vector3D<int> getOffsetOfFace(Face face)
		{
			switch (face) {
				case Face.TOP:
					return new Vector3D<int>(0, 1, 0);
				case Face.BOTTOM:
					return new Vector3D<int>(0, -1, 0);
				case Face.LEFT:
					return new Vector3D<int>(-1, 0, 0);
				case Face.RIGHT:
					return new Vector3D<int>(1, 0, 0);
				case Face.FRONT:
					return new Vector3D<int>(0, 0, 1);
				case Face.BACK:
					return new Vector3D<int>(0, 0, -1);
				default:
					return Vector3D<int>.Zero;
			}
		} 
	}
	
	
	
}
