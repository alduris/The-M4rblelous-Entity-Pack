using UnityEngine;
using RWCustom;
using Watcher;

namespace LBMergedMods.Creatures;

public class NoodleEater : Lizard
{
    public static Color NEatColor = Custom.HSL2RGB(.8333f, .9f, .7f);

    public NoodleEater(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = abstractCreature.superSizeMe ? Custom.HSL2RGB(Custom.WrappedRandomVariation(86f / 360f, .05f, .6f), Custom.WrappedRandomVariation(.95f, .05f, .1f), Custom.ClampedRandomVariation(.5f, .05f, .1f)) : Custom.HSL2RGB(Custom.WrappedRandomVariation(.8333f, .05f, .6f), Custom.WrappedRandomVariation(.9f, .05f, .1f), Custom.ClampedRandomVariation(.7f, .05f, .1f));
        Random.state = state;
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        tongue ??= new(this);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new NoodleEaterGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}