using System.Runtime.InteropServices;

namespace LBMergedMods.Items;

[StructLayout(LayoutKind.Sequential)]
public sealed class RubberBlossomProperties
{
    public int NumberOfFruits, RemainingOpenCycles;
    public bool FirstTimeRealized = true, StartsOpen, AlwaysOpen, AlwaysClosed, Open;

    internal RubberBlossomProperties(bool startsOpen, int numberOfFruits, int remainingOpenCycles, bool alwaysOpen, bool alwaysClosed)
    {
        NumberOfFruits = numberOfFruits;
        RemainingOpenCycles = remainingOpenCycles;
        StartsOpen = startsOpen;
        AlwaysOpen = alwaysOpen;
        AlwaysClosed = alwaysClosed;
    }
}