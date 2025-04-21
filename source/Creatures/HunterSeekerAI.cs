using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class HunterSeekerAI : LizardAI
{
    public HunterSeekerAI(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        AddModule(lurkTracker = new(this, lizard));
        utilityComparer.AddComparedModule(lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
    }
}