using UnityEngine;

namespace LBMergedMods.Creatures;

public class LizardWaterSpit : LizardSpit
{
    public LizardWaterSpit(Vector2 pos, Vector2 vel, Lizard lizard) : base(pos, vel, lizard) => massLeft = .8f;
}