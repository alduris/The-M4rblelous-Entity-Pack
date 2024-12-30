using UnityEngine;

namespace LBMergedMods.Creatures;

public class HunterSeeker : Lizard
{
    public override float VisibilityBonus => graphicsModule is HunterSeekerGraphics g ? -g.Camouflaged : base.VisibilityBonus;

    public HunterSeeker(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        effectColor = lizardParams.standardColor;
        jumpModule = new(this);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new HunterSeekerGraphics(this);

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos onAppendagePos, Vector2 direction)
    {
        var res = base.SpearStick(source, dmg, chunk, onAppendagePos, direction);
        var flag = chunk.index == 0 && HitInMouth(direction);
        if (source is Spear s && !dead && !flag && jumpModule.gasLeakPower > 0f && jumpModule.gasLeakSpear is null && chunk.index < 2 && (animation == Animation.Jumping || animation == Animation.PrepareToJump || Random.value < (chunk.index == 1 ? .5f : .25f)))
            jumpModule.gasLeakSpear = s;
        return res;
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}