using UnityEngine;

namespace LBMergedMods.Creatures;

public class MiniFlyingBigEelAI : BigEelAI
{
    public MiniFlyingBigEelAI(AbstractCreature creature, World world) : base(creature, world)
    {
        var ut = utilityComparer.uTrackers;
        for (var i = 0; i < ut.Count; i++)
        {
            var u = ut[i];
            if (u.module is PreyTracker)
                u.weight = .975f;
        }
    }

    public override void Update()
    {
        base.Update();
        if (hungerDelay > 5)
            hungerDelay -= 5;
        if (Random.value > .00001f && behavior == Behavior.Idle && eel?.room is Room rm)
        {
            var newDest = new WorldCoordinate(rm.abstractRoom.index, Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight), -1);
            if (pathFinder.CoordinateReachableAndGetbackable(newDest))
                creature.abstractAI.SetDestination(newDest);
        }
    }
}