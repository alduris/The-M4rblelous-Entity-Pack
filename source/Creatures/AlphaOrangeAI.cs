namespace LBMergedMods.Creatures;

public class AlphaOrangeAI : LizardAI
{
	public AlphaOrangeAI(AbstractCreature creature) : base(creature, creature.world) => AddModule(yellowAI = new(this));

	public override PathCost TravelPreference(MovementConnection connection, PathCost cost) => yellowAI?.TravelPreference(connection, cost) ?? cost;
}