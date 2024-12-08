using UnityEngine;

namespace LBMergedMods.Creatures;

public class MiniFlyingBigEel : BigEel
{
    public MiniFlyingBigEel(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
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
        collisionRange = 200f;
        var bs = bodyChunks = new BodyChunk[8];
        var bscon = bodyChunkConnections = new BodyChunkConnection[7];
        for (var i = 0; i < bs.Length; i++)
        {
            var num = i / 7f;
            num = (1f - num) * .5f + Mathf.Sin(Mathf.Pow(num, .5f) * Mathf.PI) * .5f;
            bs[i] = new(this, i, default, Mathf.Lerp(4f, 12f, num) * .95f, Mathf.Lerp(.1f, .8f, num))
            {
                restrictInRoomRange = 2000f,
                defaultRestrictInRoomRange = 2000f
            };
        }
        for (var j = 0; j < bscon.Length; j++)
            bscon[j] = new(bs[j], bs[j + 1], Mathf.Max(bs[j].rad, bs[j + 1].rad), BodyChunkConnection.Type.Normal, 1f, -1f);
        Random.state = state;
    }

    public override Color ShortCutColor() => Template.shortcutColor;

    public override void InitiateGraphicsModule() => graphicsModule ??= new MiniFlyingBigEelGraphics(this);
}