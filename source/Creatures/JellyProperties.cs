using System.Runtime.InteropServices;
using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
[StructLayout(LayoutKind.Sequential)]
public sealed class JellyProperties
{
    public Color Color;
    public float IconRadBonus;
    public bool IsJelly, Born;
}