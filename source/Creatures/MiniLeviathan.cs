using Fisobs.Core;
using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class MiniLeviathan : BigEel
{
    public static Color LeviColor = Ext.MenuGrey;

    public MiniLeviathan(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var flag = !abstractCreature.superSizeMe;
        albino = flag;
        if (flag)
        {
            iVars.patternColorB = new(0f, .6f, .75f);
            iVars.patternColorA.hue = .5f;
            iVars.patternColorA = HSLColor.Lerp(iVars.patternColorA, new(.97f, .8f, .75f), .9f);
        }
        abstractCreature.lavaImmune = true;
        collisionRange = 200f;
        var bs = bodyChunks = new BodyChunk[8];
        var bscon = bodyChunkConnections = new BodyChunkConnection[7];
        for (var i = 0; i < bs.Length; i++)
        {
            var num = i / 7f;
            num = (1f - num) * .5f + Mathf.Sin(Mathf.Pow(num, .5f) * Mathf.PI) * .5f;
            bs[i] = new(this, i, default, Mathf.Lerp(4f, 12f, num), Mathf.Lerp(.1f, .8f, num))
            {
                restrictInRoomRange = 2000f,
                defaultRestrictInRoomRange = 2000f
            };
        }
        for (var j = 0; j < bscon.Length; j++)
            bscon[j] = new(bs[j], bs[j + 1], Mathf.Max(bs[j].rad, bs[j + 1].rad), BodyChunkConnection.Type.Normal, 1f, -1f);
    }

    public override Color ShortCutColor() => LeviColor;

    public override void InitiateGraphicsModule() => graphicsModule ??= new MiniLeviathanGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}