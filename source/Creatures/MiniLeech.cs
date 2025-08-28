using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class MiniLeech : Leech
{
    public MiniLeech(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        var fs = firstChunk;
        fs.rad *= .25f;
        fs.mass *= .33f;
        waterFriction = .95f;
    }

    public override void Update(bool eu)
    {
        chargeCounter = 0;
        base.Update(eu);
        if (room is null || enteringShortCut.HasValue)
            return;
        if (Consious && firstChunk.submersion >= .5f)
        {
            if (grasps[0] is null)
            {
                if (huntPrey is Creature c && c.Submersion > 0f && c.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject) && c.NoCamo())
                {
                    var flag = false;
                    if (c.grasps is Grasp[] grs)
                    {
                        for (var i = 0; i < grs.Length; i++)
                        {
                            if (grs[i]?.grabbed is Snail or BouncingBall or StarLemon)
                            {
                                flag = true;
                                break;
                            }
                        }    
                    }
                    firstChunk.vel *= 1.03f;
                    if (!flag)
                    {
                        var preyChunks = c.bodyChunks;
                        for (var i = 0; i < preyChunks.Length; i++)
                        {
                            var bodyChunk = preyChunks[i];
                            if (Custom.DistLess(firstChunk.pos, bodyChunk.pos, bodyChunk.rad + firstChunk.rad + 1f))
                            {
                                Grab(bodyChunk.owner, 0, bodyChunk.index, Grasp.Shareability.NonExclusive, 0f, false, false);
                                room.PlaySound((c is Player) ? SoundID.Leech_Attatch_Player : SoundID.Leech_Attatch_NPC, firstChunk);
                                graphicsModule?.BringSpritesToFront();
                                if (c.dead)
                                    school.BitDeadPrey(c);
                                c.Violence(firstChunk, firstChunk.vel * firstChunk.mass, bodyChunk, null, DamageType.Bite, .015f, 2f);
                                huntPrey = null;
                                if (!Controlled)
                                    landWalkDir = Random.value >= .5f ? 1 : -1;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    var flag = false;
                    var crits = room.abstractRoom.creatures;
                    for (var i = 0; i < crits.Count; i++)
                    {
                        if (crits[i] is AbstractCreature acr && acr.SameRippleLayer(abstractPhysicalObject) && acr.realizedCreature is Creature cr && cr.NoCamo() && !cr.dead && cr.Submersion >= .5f && cr.mainBodyChunk is BodyChunk b && Custom.DistLess(b.pos, firstChunk.pos, 250f + b.rad) && Template.CreatureRelationship(cr).type == CreatureTemplate.Relationship.Type.Afraid)
                        {
                            flag = true;
                            break;
                        }
                    }
                    firstChunk.vel *= safariControlled || flag || fleeFromRain ? 1.03f : .1f;
                }
            }
        }
        chargeCounter = 0;
        airDrown *= 1.02f;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new MiniLeechGraphics(this);

    public override void LoseAllGrasps() => ReleaseGrasp(0);
}