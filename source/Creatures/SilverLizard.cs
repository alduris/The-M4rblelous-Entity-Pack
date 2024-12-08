using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class SilverLizard : Lizard
{
    public SilverLizard(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.58f, .08f, .6f), .3f, Custom.ClampedRandomVariation(.8f, .15f, .1f));
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new SilverLizardGraphics(this);
}