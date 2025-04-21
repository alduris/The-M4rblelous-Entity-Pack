using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class RedHorror : Centipede
{
    public RedHorror(AbstractCreature abstractCreature, World world) : base(abstractCreature, world) => flying = true;

    public override void InitiateGraphicsModule() => graphicsModule ??= new RedHorrorGraphics(this);

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (Consious)
        {
            var chs = bodyChunks;
            if (flying)
            {
                for (var i = 0; i < chs.Length; i++)
                    chs[i].vel *= 1.04f;
            }
            else
            {
                for (var i = 0; i < chs.Length; i++)
                    chs[i].vel *= 1.02f;
            }
        }
    }

    public override Color ShortCutColor() => abstractCreature.IsVoided() ? RainWorld.SaturatedGold : Color.red;
}