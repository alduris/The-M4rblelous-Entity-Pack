﻿using UnityEngine;

namespace LBMergedMods.Creatures;
// CHK
public class PolliwogAI : LizardAI
{
    public PolliwogAI(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        AddModule(yellowAI = new PolliwogCommunication(this));
        AddModule(lurkTracker = new(this, lizard));
        utilityComparer.AddComparedModule(lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
        tracker.maxTrackedCreatures = 20;
    }

    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
        if (yellowAI is PolliwogCommunication c)
            c.communicating = 0;
    }

    public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
    {
        var res = base.TravelPreference(connection, cost);
        if (lizard is Polliwog l && yellowAI is PolliwogCommunication c)
        {
            res = c.TravelPreference(connection, res);
            if (!l.room.GetTile(connection.destinationCoord).AnyWater)
                res.resistance += 5f;
        }
        return res;
    }
}