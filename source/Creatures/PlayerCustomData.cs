using System.Runtime.InteropServices;

namespace LBMergedMods.Creatures;

[StructLayout(LayoutKind.Sequential)]
public sealed class PlayerCustomData
{
    public float OriginalBounce;
    public int BounceEffectDuration, BlueFaceDuration;
}