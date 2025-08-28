using UnityEngine;
using System.Runtime.InteropServices;

namespace LBMergedMods.Creatures;

[StructLayout(LayoutKind.Sequential)]
public sealed class BigrubProperties
{
    public LightSource? LightSource;
    public Color BlackCol, ReducedAsRGB;
    public HSLColor NewCol;
    public float LastLightLife, LightLife, RoomLight;
    public bool IsBig, NormalLook, Born, InitGraphics;
}