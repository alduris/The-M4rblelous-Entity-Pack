using System.Runtime.InteropServices;
using UnityEngine;

namespace LBMergedMods.Creatures;

[StructLayout(LayoutKind.Sequential)]
public sealed class HVFlyData
{
    public int BiteWait, CanEatRootDelay = Random.Range(4600, 5000), Hunger = 3;
    public bool CanEatRootRnd = Random.value < .5f;

    public bool CanEatRoot => CanEatRootRnd && CanEatRootDelay == 0 && Hunger == 3;

    public bool CanEat => Hunger > 0;
}