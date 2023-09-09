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
		
    TOPLEFT = 6,
    TOPRIGHT = 7,
    TOPFRONT = 8,
    TOPBACK = 9,
		
    BOTTOMLEFT = 10,
    BOTTOMRIGHT = 11,
    BOTTOMFRONT = 12,
    BOTTOMBACK = 13,
		
    TOPLEFTFRONT = 14,
    TOPRIGHTFRONT = 15,
    TOPLEFTBACK = 16,
    TOPRIGHTBACK = 17,

    BOTTOMLEFTFRONT = 18,
    BOTTOMRIGHTFRONT = 19,
    BOTTOMLEFTBACK = 20,
    BOTTOMRIGHTBACK = 21,
		
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
				
			case FaceExtended.TOPLEFT:
				return new Vector3D<int>(-1, 1, 0);
			case FaceExtended.TOPRIGHT:
				return new Vector3D<int>(1, 1, 0);
			case FaceExtended.TOPFRONT:
				return new Vector3D<int>(0, 1, 1);
			case FaceExtended.TOPBACK:
				return new Vector3D<int>(0, 1, -1);
				
			case FaceExtended.BOTTOMLEFT:
				return new Vector3D<int>(-1, -1, 0);
			case FaceExtended.BOTTOMRIGHT:
				return new Vector3D<int>(1, -1, 0);
			case FaceExtended.BOTTOMFRONT:
				return new Vector3D<int>(0, -1, 1);
			case FaceExtended.BOTTOMBACK:
				return new Vector3D<int>(0, -1, -1);
				
			case FaceExtended.TOPLEFTFRONT:
				return new Vector3D<int>(-1, 1, 1);
			case FaceExtended.TOPRIGHTFRONT:
				return new Vector3D<int>(1, 1, 1);
			case FaceExtended.TOPLEFTBACK:
				return new Vector3D<int>(-1, 1, -1);
			case FaceExtended.TOPRIGHTBACK:
				return new Vector3D<int>(1, 1, -1);
				
			case FaceExtended.BOTTOMLEFTFRONT:
				return new Vector3D<int>(-1, -1, 1);
			case FaceExtended.BOTTOMRIGHTFRONT:
				return new Vector3D<int>(1, -1, 1);
			case FaceExtended.BOTTOMLEFTBACK:
				return new Vector3D<int>(-1, -1, -1);
			case FaceExtended.BOTTOMRIGHTBACK:
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