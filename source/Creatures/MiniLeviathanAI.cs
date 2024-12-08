namespace LBMergedMods.Creatures;

public class MiniLeviathanAI : BigEelAI
{
    public MiniLeviathanAI(AbstractCreature creature, World world) : base(creature, world)
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
    }
}