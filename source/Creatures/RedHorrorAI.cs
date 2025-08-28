using UnityEngine;

namespace LBMergedMods.Creatures;

public class RedHorrorAI(AbstractCreature creature, World world) : CentipedeAI(creature, world)
{
    public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
    {
        base.CreatureSpotted(firstSpot, creatureRep);
        if (creatureRep.representedCreature is AbstractCreature acrit && acrit.SameRippleLayer(creature) && acrit.realizedCreature is Creature c && c.NoCamo())
        {
            var tp = StaticRelationship(acrit).type;
            if (!c.dead && centipede is RedHorror cent && cent.room is Room rm && !cent.dead && DoIWantToShockCreature(acrit) && (tp == CreatureTemplate.Relationship.Type.Eats || tp == CreatureTemplate.Relationship.Type.Attacks) && cent.bodyChunks is BodyChunk[] cAr)
            {
                for(var i = 0; i < cAr.Length; i++)
                {
                    var b = cAr[i];
                    if (Random.value < .1f)
                        rm.AddObject(new RedHorrorFlash(b.pos, b.rad / (b.rad * 30f)));
                }
            }
        }
    }
}