using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class MoleSalamander : Lizard
{
    public MoleSalamander(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        buoyancy = .92f;
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.9f, .15f, .6f), 1f, Custom.ClampedRandomVariation(.4f, .15f, .2f));
        Random.state = state;
        abstractCreature.HypothermiaImmune = true;
        firstChunk.rad *= 1.15f;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new MoleSalamanderGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        lungs = 1f;
    }

    public override Color ShortCutColor() => graphicsModule is MoleSalamanderGraphics { blackSalamander: true } ? Color.Lerp(Color.black, Color.gray, .5f) : base.ShortCutColor();
}