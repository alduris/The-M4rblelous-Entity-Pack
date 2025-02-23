using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class MoleSalamander : Lizard
{
    public bool Black;

    public MoleSalamander(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        buoyancy = .92f;
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        if (world.region is Region reg)
            Black = Random.value < reg.regionParams.blackSalamanderChance;
        else
            Black = Random.value < 1f / 3f;
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

    public override Color ShortCutColor() => Black ? Color.Lerp(Color.black, Color.gray, .5f) : base.ShortCutColor();

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}