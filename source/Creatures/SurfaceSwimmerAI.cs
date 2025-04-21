namespace LBMergedMods.Creatures;
//CHK
public class SurfaceSwimmerAI(AbstractCreature creature, World world) : EggBugAI(creature, world)
{
    public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
    {
        var res = base.TravelPreference(coord, cost);
        var coord2 = bug?.room?.GetTile(coord.destinationCoord);
        if (coord2 is not null && !coord2.WaterSurface && !coord2.AnyBeam)
            res.resistance += 5f;
        return res;
    }
}