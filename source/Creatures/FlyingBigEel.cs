using UnityEngine;

namespace LBMergedMods.Creatures;

public class FlyingBigEel : BigEel
{
    public FlyingBigEel(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        iVars.patternColorB = HSLColor.Lerp(RainWorld.GoldHSL, new(RainWorld.GoldHSL.hue, RainWorld.GoldHSL.saturation, RainWorld.GoldHSL.lightness + Random.value / 12f), .5f);
        iVars.patternColorA = RainWorld.GoldHSL;
        iVars.patternColorA.hue = .5f;
        iVars.patternColorA = HSLColor.Lerp(iVars.patternColorA, new(RainWorld.GoldHSL.hue + Random.value / 50f, RainWorld.GoldHSL.saturation + Random.value / 50f, RainWorld.GoldHSL.lightness + Random.value / 4f), .9f);
        airFriction = .98f;
        waterFriction = .999f;
        gravity = 0f;
        buoyancy = 1f;
        bounce = 0f;
        albino = false;
        var chs = bodyChunks;
        for (var i = 0; i < chs.Length; i++)
            chs[i].rad *= .75f;
        Random.state = state;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new FlyingBigEelGraphics(this);
}