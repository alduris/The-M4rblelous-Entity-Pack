using UnityEngine;
using RWCustom;
using Watcher;

namespace LBMergedMods.Creatures;
//CHK
public class SilverLizard : Lizard
{
    public SilverLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.58f, .08f, .6f), .3f, Custom.ClampedRandomVariation(.8f, .15f, .1f));
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new SilverLizardGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}