using UnityEngine;
using System.Runtime.InteropServices;

namespace LBMergedMods.Items;
//CHK
[StructLayout(LayoutKind.Sequential)]
public sealed class MiniFruitSpawnerProperties()
{
    public Vector2 RootPos;
    public int NumberOfFruits;
}