namespace LBMergedMods.Creatures;
//CHK
public class HoverflyAbstractAI(World world, AbstractCreature parent) : AbstractCreatureAI(world, parent)
{
    public override void AbstractBehavior(int time)
    {
        if (parent.realizedCreature is null)
        {
            if (path.Count > 0)
                FollowPath(time);
            else if (denPosition is WorldCoordinate w && parent.pos.room != w.room)
                GoToDen();
        }
    }
}