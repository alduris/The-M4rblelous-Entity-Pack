using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
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
        if (eel?.room is not Room rm)
            return;
        if (rm.IsPositionInsideBoundries(creature.pos.Tile))
        {
            if (hungerDelay > 0)
                --hungerDelay;
            if (hungerDelay > 0)
                --hungerDelay;
            if (hungerDelay > 0)
                --hungerDelay;
        }
        if (Random.value > .00001f && behavior == Behavior.Idle)
        {
            var newDest = new WorldCoordinate(rm.abstractRoom.index, Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight), -1);
            if (pathFinder.CoordinateReachableAndGetbackable(newDest))
                creature.abstractAI.SetDestination(newDest);
        }
    }
}