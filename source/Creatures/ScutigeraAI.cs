using UnityEngine;

namespace LBMergedMods.Creatures;

public class ScutigeraAI : CentipedeAI
{
    public ScutigeraAI(AbstractCreature creature, World world) : base(creature, world) => pathFinder.stepsPerFrame = 15;

    public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
    {
        base.CreatureSpotted(firstSpot, creatureRep);
        if (creatureRep.representedCreature is AbstractCreature acrit && acrit.realizedCreature is Creature c)
        {
            var tp = StaticRelationship(acrit).type;
            if (!c.dead && centipede is Scutigera cent && cent.room is Room rm && !cent.dead && DoIWantToShockCreature(acrit) && (tp == CreatureTemplate.Relationship.Type.Eats || tp == CreatureTemplate.Relationship.Type.Attacks) && cent.bodyChunks is BodyChunk[] cAr)
            {
                for (var i = 0; i < cAr.Length; i++)
                {
                    var b = cAr[i];
                    if (Random.value < .1f)
                        rm.AddObject(new ScutigeraFlash(b.pos, b.rad / (b.rad * 30f)));
                }
            }
        }
    }
}