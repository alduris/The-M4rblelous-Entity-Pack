namespace LBMergedMods.Creatures;

public class ScavengerSentinelAI : ScavengerAI
{
    public ScavengerSentinelAI(AbstractCreature creature, World world) : base(creature, world)
    {
        preyTracker.sureToGetPreyDistance = 80f;
        preyTracker.sureToLosePreyDistance = 600f;
    }
}