using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;
//CHK
public class MoleSalamanderAI : LizardAI
{
    public MoleSalamanderAI(AbstractCreature creature, World world) : base(creature, world)
    {
        AddModule(new SuperHearing(this, tracker, 350f));
        AddModule(lurkTracker = new(this, lizard));
        utilityComparer.AddComparedModule(lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
    }

    public override void Update()
    {
        base.Update();
        if (noiseTracker is NoiseTracker n)
            n.hearingSkill = 2f;
    }

    public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
    {
        var res = base.TravelPreference(connection, cost);
        if (lizard is MoleSalamander l && !l.room.GetTile(connection.destinationCoord).AnyWater)
            res.resistance += 5f;
        return res;
    }

    public override float VisualScore(Vector2 lookAtPoint, float bonus)
    {
        if (creature?.realizedCreature is MoleSalamander l && l.room is Room rm && rm.GetTile(lookAtPoint).DeepWater && rm.GetTile(l.VisionPoint).DeepWater && Custom.DistLess(l.VisionPoint, lookAtPoint, 8000f * bonus))
            return 1f;
        return base.VisualScore(lookAtPoint, bonus);
    }
}