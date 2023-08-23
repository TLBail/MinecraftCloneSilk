namespace MinecraftCloneSilk.Collision;

public struct HitInfo
{
    public bool haveHited;
    public float fNorm;

    public HitInfo(bool haveHited, float fNorm)
    {
        this.haveHited = haveHited;
        this.fNorm = fNorm;
    }
}