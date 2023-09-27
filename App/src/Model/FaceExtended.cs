using System.Collections.ObjectModel;
using Silk.NET.Maths;

namespace MinecraftCloneSilk.Model;

public enum FaceExtended
{
    TOP = 0,
    BOTTOM = 1,
    LEFT = 2,
    RIGHT = 3,
    FRONT = 4,
    BACK = 5,
		
    LEFTTOP = 6,
    RIGHTTOP = 7,
    TOPFRONT = 8,
    TOPBACK = 9,
		
    LEFTBOTTOM = 10,
    RIGHTBOTTOM = 11,
    BOTTOMFRONT = 12,
    BOTTOMBACK = 13,
		
    LEFTTOPFRONT = 14,
    RIGHTTOPFRONT = 15,
    LEFTTOPBACK = 16,
    RIGHTTOPBACK = 17,

    LEFTBOTTOMFRONT = 18,
    RIGHTBOTTOMFRONT = 19,
    LEFTBOTTOMBACK = 20,
    RIGHTBOTTOMBACK = 21,
		
    LEFTFRONT = 22,
    RIGHTFRONT = 23,
    LEFTBACK = 24,
    RIGHTBACK = 25
};

public static class FaceExtendedConst
{
	public static readonly ReadOnlyCollection<FaceExtended> FACES = new ReadOnlyCollection<FaceExtended>(Enum.GetValues<FaceExtended>());
}

public static class FaceExtendedOffset
{
	public static Vector3D<int> GetOffsetOfFace(FaceExtended face)
	{
		switch (face) {
			case FaceExtended.TOP:
				return new Vector3D<int>(0, 1, 0);
			case FaceExtended.BOTTOM:
				return new Vector3D<int>(0, -1, 0);
			case FaceExtended.LEFT:
				return new Vector3D<int>(-1, 0, 0);
			case FaceExtended.RIGHT:
				return new Vector3D<int>(1, 0, 0);
			case FaceExtended.FRONT:
				return new Vector3D<int>(0, 0, 1);
			case FaceExtended.BACK:
				return new Vector3D<int>(0, 0, -1);
				
			case FaceExtended.LEFTTOP:
				return new Vector3D<int>(-1, 1, 0);
			case FaceExtended.RIGHTTOP:
				return new Vector3D<int>(1, 1, 0);
			case FaceExtended.TOPFRONT:
				return new Vector3D<int>(0, 1, 1);
			case FaceExtended.TOPBACK:
				return new Vector3D<int>(0, 1, -1);
				
			case FaceExtended.LEFTBOTTOM:
				return new Vector3D<int>(-1, -1, 0);
			case FaceExtended.RIGHTBOTTOM:
				return new Vector3D<int>(1, -1, 0);
			case FaceExtended.BOTTOMFRONT:
				return new Vector3D<int>(0, -1, 1);
			case FaceExtended.BOTTOMBACK:
				return new Vector3D<int>(0, -1, -1);
				
			case FaceExtended.LEFTTOPFRONT:
				return new Vector3D<int>(-1, 1, 1);
			case FaceExtended.RIGHTTOPFRONT:
				return new Vector3D<int>(1, 1, 1);
			case FaceExtended.LEFTTOPBACK:
				return new Vector3D<int>(-1, 1, -1);
			case FaceExtended.RIGHTTOPBACK:
				return new Vector3D<int>(1, 1, -1);
				
			case FaceExtended.LEFTBOTTOMFRONT:
				return new Vector3D<int>(-1, -1, 1);
			case FaceExtended.RIGHTBOTTOMFRONT:
				return new Vector3D<int>(1, -1, 1);
			case FaceExtended.LEFTBOTTOMBACK:
				return new Vector3D<int>(-1, -1, -1);
			case FaceExtended.RIGHTBOTTOMBACK:
				return new Vector3D<int>(1, -1, -1);
				
				
			case FaceExtended.LEFTFRONT:
				return new Vector3D<int>(-1, 0, 1);
			case FaceExtended.RIGHTFRONT:
				return new Vector3D<int>(1, 0, 1);
			case FaceExtended.LEFTBACK:
				return new Vector3D<int>(-1, 0, -1);
			case FaceExtended.RIGHTBACK:
				return new Vector3D<int>(1, 0, -1);
			
			default:
				return Vector3D<int>.Zero;
		}
	}

}