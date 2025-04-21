using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class CommonEelAI : LizardAI
{
    public CommonEelAI(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        AddModule(lurkTracker = new(this, lizard));
        utilityComparer.AddComparedModule(lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
        preyTracker.giveUpOnUnreachablePrey = 1800;
    }

    public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
    {
        var res = base.TravelPreference(connection, cost);
        if (lizard is CommonEel eel && !eel.room.GetTile(connection.destinationCoord).AnyWater)
        {
            res.legality = PathCost.Legality.Unallowed;
            res.resistance = float.MaxValue;
        }
        return res;
    }
}