using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class Scutigera : Centipede
{
    public Scutigera(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var chs = bodyChunks;
        var le = chs.Length;
        var sz = size;
        for (var i = 0; i < chs.Length; i++)
        {
            var num = (float)i / (le - 1);
            var num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, sz), Mathf.Lerp(4f, 6.5f, sz), Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * num), 0f, 1f), Mathf.Lerp(.7f, .3f, sz)));
            num2 = Mathf.Lerp(num2, Mathf.Lerp(2f, 3.5f, sz), .4f);
            chs[i].rad = num2;
        }
        var num3 = 0;
        for (var l = 0; l < chs.Length; l++)
        {
            for (var m = l + 1; m < chs.Length; m++)
            {
                bodyChunkConnections[num3].distance = chs[l].rad + chs[m].rad;
                num3++;
            }
        }
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new ScutigeraGraphics(this);

    public override Color ShortCutColor() => Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f);

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
    {
        if (CentiState is CentipedeState s && Random.value < .25f && chunk is not null && chunk.index >= 0 && chunk.index < s.shells.Length && (chunk.index == shellJustFellOff || s.shells[chunk.index]))
        {
            if (chunk.index == shellJustFellOff)
                shellJustFellOff = -1;
            return false;
        }
        return base.SpearStick(source, dmg, chunk, appPos, direction);
    }
}