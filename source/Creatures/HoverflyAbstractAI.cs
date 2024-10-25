namespace LBMergedMods.Creatures;

public class HoverflyAbstractAI(World world, AbstractCreature parent) : AbstractCreatureAI(world, parent)
{
    public override void AbstractBehavior(int time)
    {
        if (parent.realizedCreature is null)
        {
            if (path.Count > 0)
                FollowPath(time);
            else if (denPosition.HasValue && parent.pos.room != denPosition.Value.room)
                GoToDen();
        }
    }
}