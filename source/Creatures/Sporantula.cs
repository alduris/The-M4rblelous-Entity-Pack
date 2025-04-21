using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;
//CHK
public class Sporantula : BigSpider
{
    public Sporantula(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        spitter = true;
        yellowCol = new(.9f, 1f, .8f);
        if (!SporeMemory.TryGetValue(abstractCreature, out _))
            SporeMemory.Add(abstractCreature, []);
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new SporantulaGraphics(this);

    public override void Die()
    {
        if (!dead)
        {
            if (SporeMemory.TryGetValue(abstractCreature, out var hs))
                hs.Clear();
            Explode();
        }
        base.Die();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var bs = bodyChunks;
        if (Consious)
        {
            for (var i = 0; i < bs.Length; i++)
                bs[i].vel *= 1.02f;
        }
        if (dead && bs[1] is BodyChunk b && b.rad != 0f)
        {
            abstractPhysicalObject.LoseAllStuckObjects();
            b.rad = 0f;
            b.mass = bs[0].mass;
            b.loudness = 0f;
            bodyChunkConnections[0].distance = 5f;
        }
        buoyancy = dead ? 1.9f : .95f;
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (source is not null && RippleViolenceCheck(source) && SporeMemory.TryGetValue(abstractCreature, out var hs))
        {
            if (source.owner is Creature c)
                hs.Add(c.abstractCreature);
            else if (source.owner is Weapon w && w.thrownBy is Creature cr)
                hs.Add(cr.abstractCreature);
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    public virtual void Angry()
    {
        if (slatedForDeletetion || room is not Room rm || rm.game is not RainWorldGame g)
            return;
        var color = Color.Lerp(new(.9f, 1f, .8f), g.cameras[0].currentPalette.texture.GetPixel(11, 4), .5f);
        var sporeColor = Color.Lerp(color, new(.02f, .1f, .08f), .85f);
        var chunk = mainBodyChunk;
        rm.AddObject(new SporeCloud(chunk.pos, -chunk.vel * 3f, sporeColor, .7f, abstractCreature, 0, null, abstractPhysicalObject.rippleLayer));
        rm.PlaySound(SoundID.Puffball_Eplode, chunk, false, .5f, 1f);
    }

    public virtual void Explode()
    {
        if (slatedForDeletetion || room is not Room rm || rm.game is not RainWorldGame g)
            return;
        ReleaseAllGrabChunks();
        InsectCoordinator? smallInsects = null;
        var ulist = rm.updateList;
        for (var i = 0; i < ulist.Count; i++)
        {
            if (ulist[i] is InsectCoordinator ins)
            {
                smallInsects = ins;
                break;
            }
        }
        var color = Color.Lerp(new(.9f, 1f, .8f), g.cameras[0].currentPalette.texture.GetPixel(11, 4), .5f);
        var sporeColor = Color.Lerp(color, new(.02f, .1f, .08f), .85f);
        var ps = mainBodyChunk.pos;
        for (var j = 0; j < 100; j++)
            rm.AddObject(new SporeCloud(ps, Custom.RNV() * Random.value * 10f, sporeColor, 1f, abstractCreature, j % 20, smallInsects, abstractPhysicalObject.rippleLayer));
        rm.AddObject(new SporePuffVisionObscurer(ps, abstractPhysicalObject.rippleLayer));
        rm.PlaySound(SoundID.Puffball_Eplode, mainBodyChunk);
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}