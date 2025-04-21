﻿using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LBMergedMods.Items;
//CHK
[StructLayout(LayoutKind.Sequential)]
public sealed class DeathPersistentScoreData
{
    public HashSet<string> CollectedTokens = [];
    public int Score;
}