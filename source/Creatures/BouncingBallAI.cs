using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class BouncingBallAI(AbstractCreature creature, World world) : SnailAI(creature, world)
{
    public int ShuffleDestination;

    public override void Update()
    {
        base.Update();
        if (snail is BouncingBall b && b.room is Room rm)
        {
            --ShuffleDestination;
            var narrow = b.NarrowSpace();
            if (narrow && ShuffleDestination <= 0)
            {
                ShuffleDestination = 200;
                creature?.abstractAI.SetDestination(new(rm.abstractRoom.index, Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight), -1));
            }
            if (ShuffleDestination > 0 || narrow)
                move = true;
        }
    }
}