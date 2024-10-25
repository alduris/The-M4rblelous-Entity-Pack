using System.Runtime.InteropServices;

namespace LBMergedMods.Creatures;

[StructLayout(LayoutKind.Sequential)]
public sealed class FlyProperties
{
    public int Ext1, Ext2;
    public bool IsSeed, Born;
}