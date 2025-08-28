using System.Runtime.InteropServices;

namespace LBMergedMods.Creatures;

[StructLayout(LayoutKind.Sequential)]
public sealed class PlayerCustomData
{
    public DarkGrubVision? GrubVision;
    public float OriginalBounce;
    public int BounceEffectDuration, BlueFaceDuration, GrubVisionDuration;
}