using RWCustom;
using Watcher;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class AlphaOrange : Lizard
{
	public AlphaOrange(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
	{
		effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.1f, .05f, .6f), 1f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
        if (rotModule is LizardRotModule mod && LizardState.rotType != LizardState.RotType.Slight)
            effectColor = Color.Lerp(effectColor, mod.RotEyeColor, LizardState.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        jumpModule = new(this);
	}

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new AlphaOrangeGraphics(this);
        graphicsModule.Reset();
    }
}