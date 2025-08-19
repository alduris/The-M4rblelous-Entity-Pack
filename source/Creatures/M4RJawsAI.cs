using UnityEngine;
using System;

namespace LBMergedMods.Creatures;

public class M4RJawsAI : ArtificialIntelligence, IUseARelationshipTracker, ILookingAtCreatures
{
	public class Behavior(string value, bool register = false) : ExtEnum<Behavior>(value, register)
	{
		public static Behavior Idle = new(nameof(Idle), true),
			Flee = new(nameof(Flee), true),
			Hunt = new(nameof(Hunt), true),
			EscapeRain = new(nameof(EscapeRain), true),
			ReturnPrey = new(nameof(ReturnPrey), true),
			GetUnstuck = new(nameof(GetUnstuck), true),
            Disencouraged = new(nameof(Disencouraged), true);
    }

    public class DisencouragedTracker(ArtificialIntelligence AI) : AIModule(AI)
    {
        public float Disencouraged;

        public override void Update()
        {
            base.Update();
            if (Disencouraged > 1f)
                Disencouraged = Mathf.Lerp(Disencouraged, 1f, .075f);
        }

        public override float Utility() => Mathf.Pow(Mathf.Clamp(Disencouraged, 0f, 1f), 3f);
    }

    public DisencouragedTracker MyDisencouragedTracker;
    public M4RJaws Ow;
	public CreatureLooker Looker;
    public Behavior Behav = Behavior.Idle;
    public Tracker.CreatureRepresentation? FocusCreature;
    //public DebugDestinationVisualizer DebugDestinationVisualizer;
    public float CurrentUtility;
	public bool EnteredRoom;

    public virtual float Disencouraged
    {
        get => MyDisencouragedTracker.Disencouraged;
        set => MyDisencouragedTracker.Disencouraged = value;
    }

    public M4RJawsAI(AbstractCreature creature, World world) : base(creature, world)
	{
		Ow = (creature.realizedCreature as M4RJaws)!;
		Ow.AI = this;
		AddModule(new M4RJawsPather(this, world, creature));
        AddModule(MyDisencouragedTracker = new(this));
        AddModule(new Tracker(this, 4, 10, 160, .25f, 5, 1, 5));
		AddModule(new PreyTracker(this, 5, 1f, 5f, 15f, .95f));
		AddModule(new ThreatTracker(this, 3));
		AddModule(new RainTracker(this));
		AddModule(new DenFinder(this, creature));
		AddModule(new StuckTracker(this, true, false));
		stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(stuckTracker));
		AddModule(new RelationshipTracker(this, tracker));
		AddModule(new UtilityComparer(this));
		utilityComparer.AddComparedModule(threatTracker, null, .9f, 1.1f);
		utilityComparer.AddComparedModule(preyTracker, null, .5f, 1.1f);
		utilityComparer.AddComparedModule(rainTracker, null, .9f, 1.1f);
		utilityComparer.AddComparedModule(stuckTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(MyDisencouragedTracker, null, .95f, 1.1f);
		Looker = new(this, tracker, creature.realizedCreature, .0025f, 30);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		EnteredRoom = false;
	}

	public override void Update()
	{
		base.Update();
        Disencouraged = Math.Max(0f, Disencouraged - 1f / Mathf.Lerp(600f, 4800f, Disencouraged));
        Looker.Update();
		var trk = tracker;
		for (var num = tracker.CreaturesCount - 1; num >= 0; num--)
		{
			var rep = trk.GetRep(num);
            if (rep.TicksSinceSeen > 160)
                trk.ForgetCreature(rep.representedCreature);
		}
		if (!EnteredRoom && Ow.room is Room rm && creature.pos.x > 2 && creature.pos.x < rm.TileWidth - 3)
			EnteredRoom = true;
		var aIModule = utilityComparer.HighestUtilityModule();
		CurrentUtility = utilityComparer.HighestUtility();
		if (aIModule != null)
		{
			if (aIModule is ThreatTracker)
				Behav = Behavior.Flee;
			else if (aIModule is RainTracker)
				Behav = Behavior.EscapeRain;
			else if (aIModule is PreyTracker)
				Behav = Behavior.Hunt;
			else if (aIModule is StuckTracker)
				Behav = Behavior.GetUnstuck;
			else if (aIModule is DisencouragedTracker)
				Behav = Behavior.Disencouraged;
		}
		if (CurrentUtility < .1f)
			Behav = Behavior.Idle;
		if (Ow.grasps[0] is not null && Behav != Behavior.Flee && Behav != Behavior.EscapeRain)
			Behav = Behavior.ReturnPrey;
		if (Behav == Behavior.Idle && denFinder.GetDenPosition() is WorldCoordinate w)
			creature.abstractAI.SetDestination(w);
		else if (Behav == Behavior.Flee)
		{
			var destination = threatTracker.FleeTo(creature.pos, 5, 30, CurrentUtility > .3f);
			if (threatTracker.mostThreateningCreature is Tracker.CreatureRepresentation crit)
				FocusCreature = crit;
			creature.abstractAI.SetDestination(destination);
		}
		else if (Behav == Behavior.EscapeRain || Behav == Behavior.Disencouraged)
		{
			FocusCreature = null;
			if (denFinder.GetDenPosition() is WorldCoordinate w2)
				creature.abstractAI.SetDestination(w2);
		}
        else if (Behav == Behavior.ReturnPrey)
        {
            if (denFinder.GetDenPosition() is WorldCoordinate w2)
                creature.abstractAI.SetDestination(w2);
        }
        else if (Behav == Behavior.Hunt)
		{
			FocusCreature = preyTracker.MostAttractivePrey;
			creature.abstractAI.SetDestination(FocusCreature.BestGuessForPosition());
		}
		else if (Behav == Behavior.GetUnstuck)
			creature.abstractAI.SetDestination(stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
	}

	public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
	{
		var neckTipPos = Ow.Neck.Tip.pos;
        return base.VisualScore(lookAtPoint, targetSpeed) - Mathf.InverseLerp(.7f, .3f, Vector2.Dot((neckTipPos - lookAtPoint).normalized, (neckTipPos - Ow.Head.pos).normalized));
	}

	public virtual bool DoIWantToBiteCreature(AbstractCreature creature)
	{
		var rel = DynamicRelationship(creature).type;
        if ((rel != CreatureTemplate.Relationship.Type.Eats && rel != CreatureTemplate.Relationship.Type.Attacks) || creature.creatureTemplate.smallCreature)
			return false;
		return true;
	}

	public override bool WantToStayInDenUntilEndOfCycle() => rainTracker.Utility() > .01f;

	public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep) => Looker.ReevaluateLookObject(creatureRep, firstSpot ? 6f : 2f);

	public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
	{
		if (otherCreature.creatureTemplate.smallCreature)
			return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false);
		return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
	}

	public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
	{
		if (Ow.room is not Room rm || !coord.destinationCoord.TileDefined || coord.destinationCoord.room != rm.abstractRoom.index)
			return cost;
		return new(cost.resistance + Math.Abs(5f - rm.aimap.getAItile(coord.DestTile).floorAltitude) * 30f * Mathf.InverseLerp(150f, 40f, Ow.StuckCounter), cost.legality);
	}

	public virtual AIModule? ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
	{
		if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
			return threatTracker;
		if (relationship.type == CreatureTemplate.Relationship.Type.Eats)
			return preyTracker;
		return null;
	}

    public virtual RelationshipTracker.TrackedCreatureState? CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => null;

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation) => dRelation.currentRelationship;

	public virtual float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		if (crit.representedCreature.creatureTemplate.smallCreature)
			return -1f;
		if (crit == FocusCreature)
			return score * 10f;
		var critPos = crit.BestGuessForPosition();
        if (critPos.room == creature.pos.room && critPos.x < creature.pos.x == Ow.MoveDir.x > 0f && Math.Abs(Ow.MoveDir.x) > .2f)
			return -1f;
		return score;
	}

	public virtual Tracker.CreatureRepresentation? ForcedLookCreature() => null;

	public virtual void LookAtNothing() { }
}
