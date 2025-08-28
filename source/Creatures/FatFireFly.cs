using UnityEngine;
using MoreSlugcats;

namespace LBMergedMods.Creatures;

public class FatFireFly : Vulture, IProvideWarmth
{
    public virtual Room loadedRoom => room;

    public virtual float warmth => !dead ? RainWorldGame.DefaultHeatSourceWarmth * 2f : 0f;

    public virtual float range => 600f;

    public FatFireFly(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var num = .8f;
        var bs = bodyChunks;
        bs[0].mass *= num;
        bs[1].mass *= num;
        bs[2].mass *= num;
        bs[3].mass *= num;
        bs[0].rad *= num;
        bs[1].rad *= num;
        bs[2].rad *= num;
        bs[3].rad *= num;
        var ts = tentacles;
        int j;
        for (j = 0; j < ts.Length; j++)
            ts[j].idealLength = 100f;
        neck.idealLength = 80f;
        var neckTCh = neck.tChunks;
        for (j = 0; j < neckTCh.Length; j++)
            neckTCh[j].rad = 4f;
        neckTCh[neckTCh.Length - 1].rad = 6f;
        bodyChunkConnections[6].distance = 50f;
        abstractCreature.HypothermiaImmune = true;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new FatFireFlyGraphics(this);

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
    {
        if (!RippleViolenceCheck(source))
            return;
        var baseStun = stun;
        if (type == DamageType.Explosion)
            return;
        base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        if (Stunned)
            stun = Mathf.Clamp(stun - 1, baseStun, int.MaxValue);
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (jetSound is ChunkSoundEmitter s)
            s.volume = 0f;
    }

    public override Color ShortCutColor() => abstractCreature.superSizeMe ? new(33f / 255f, 155f / 255f, 217f / 255f) : new(.75f, .15f, 0f);

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        if (!dead && otherObject is Creature c and not FatFireFly and not MirosBird and not BigEel and not JetFish && (!ModManager.DLCShared || c.Template.type != DLCSharedEnums.CreatureTemplateType.MirosVulture))
            c.Stun(25);
        base.Collide(otherObject, myChunk, otherChunk);
        if (!Snapping || myChunk != 4 || grasps[0] is not null)
            return;
        if (otherObject is Creature cr)
        {
            var ch = bodyChunks[myChunk];
            cr.Violence(ch, ch.vel * 2f, otherObject.bodyChunks[otherChunk], null, DamageType.Bite, 5f, 0f);
        }
    }

    public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        if (obj is FatFireFly)
            return false;
        var res = base.Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        if (res && obj is Creature c)
        {
            if (AI?.StaticRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                c.Die();
            else
                c.Stun(100);
        }
        return res;
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public virtual Vector2 Position() => mainBodyChunk.pos;
}