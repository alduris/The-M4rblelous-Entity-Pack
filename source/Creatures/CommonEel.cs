using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class CommonEel : Lizard
{
    public static Color EelCol = Color.Lerp(new(46f / 51f, .05490196f, .05490196f), Color.gray, .2f);
    public float AirInLungs = 1f;

    public CommonEel(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        buoyancy = .92f;
        effectColor = Custom.HSL2RGB(abstractCreature.superSizeMe ? Custom.WrappedRandomVariation(225f / 360f, .02f, .6f) : Custom.WrappedRandomVariation(.0025f, .02f, .6f), 1f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
        abstractCreature.HypothermiaImmune = true;
        voice.myPitch *= .7f;
        var chs = bodyChunks;
        for (var i = 0; i < chs.Length; i++)
        {
            var c = chs[i];
            c.rad *= .75f;
            c.mass *= .75f;
        }
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new CommonEelGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        lungs = 1f;
        if (LizardState?.limbHealth is float[] ar)
        {
            for (var i = 0; i < ar.Length; i++)
                ar[i] = 0f;
        }
        if (Submersion < .2f)
        {
            if (AirInLungs > 0f)
                AirInLungs -= .002f;
            if (Consious)
            {
                var chs = bodyChunks;
                chs[0].pos += Custom.RNV();
                chs[1].pos += Custom.RNV();
                chs[2].pos += Custom.RNV();
            }
        }
        else
            AirInLungs = 1f;
        if (AirInLungs <= 0f)
            Die();
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}