using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class SporantulaAI : BigSpiderAI
{
    public SporantulaAI(AbstractCreature creature, World world) : base(creature, world)
    {
        if (preyTracker is PreyTracker p)
        {
            p.frustrationSpeed /= 4f;
            p.giveUpOnUnreachablePrey = 1200;
            p.persistanceBias *= 2f;
        }
    }

    public override void Update()
    {
        base.Update();
        stayAway = false;
        if (SporeMemory.TryGetValue(creature, out var mem) && creature.Room is AbstractRoom rm)
        {
            var crits = rm.creatures;
            for (var i = 0; i < crits.Count; i++)
            {
                var cr = crits[i];
                if (cr.SameRippleLayer(creature) && cr.NoCamo() && !mem.Contains(cr))
                {
                    var tp = StaticRelationship(cr).type;
                    if ((tp == CreatureTemplate.Relationship.Type.Attacks || tp == CreatureTemplate.Relationship.Type.Eats || tp == CreatureTemplate.Relationship.Type.Ignores) && DoIWantToKill(cr))
                        mem.Add(cr);
                }
            }
            if (mem.Count > 0)
            {
                ref var ptW = ref utilityComparer.GetUtilityTracker(preyTracker).weight;
                ptW = Mathf.Clamp(ptW * 2f, 0f, 100f);
                if (behavior != Behavior.EscapeRain && behavior != Behavior.ReturnPrey)
                    behavior = Behavior.Hunt;
            }
        }
        shyLightCycle = 0f;
    }

    public virtual bool DoIWantToKill(AbstractCreature cr)
    {
        if (cr.SameRippleLayer(creature) && cr.NoCamo() && cr.creatureTemplate.type != CreatureTemplateType.Sporantula)
        {
            if (cr.realizedCreature is InsectoidCreature)
                return true;
            if (bug?.State?.health < .75f)
                return true;
            if (cr.realizedCreature?.grasps is Creature.Grasp[] gr)
            {
                for (var i = 0; i < gr.Length; i++)
                {
                    if (gr[i]?.grabbed is InsectoidCreature or PuffBall or SmallPuffBall)
                        return true;
                }
            }
        }
        return false;
    }
}