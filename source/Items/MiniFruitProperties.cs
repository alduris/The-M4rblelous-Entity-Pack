﻿using System.Runtime.InteropServices;
using UnityEngine;

namespace LBMergedMods.Items;
//CHK
[StructLayout(LayoutKind.Sequential)]
public sealed class MiniFruitProperties
{
    public AbstractConsumable? Spawner;
    public Vector2 FruitPos;
}