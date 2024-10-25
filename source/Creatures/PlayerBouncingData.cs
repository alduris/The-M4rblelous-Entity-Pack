using System.Runtime.InteropServices;

namespace LBMergedMods.Creatures;

[StructLayout(LayoutKind.Sequential)]
public sealed class PlayerBouncingData
{
    public float OriginalBounce;
    public int BounceEffectDuration;
}