using UnityEngine;

namespace LBMergedMods.Creatures;

public class WaterBlobAI : ArtificialIntelligence, IUseARelationshipTracker
{
    public class Behavior(string value, bool register = false) : ExtEnum<Behavior>(value, register)
    {
        public static readonly Behavior Idle = new(nameof(Idle), true);

        public static readonly Behavior Hunting = new(nameof(Hunting), true);

        public static readonly Behavior Fleeing = new(nameof(Fleeing), true);

        public static readonly Behavior GoHome = new(nameof(GoHome), true);
    }

    public Behavior? Behav;
    public int JumpUrgency;
    public float CurrentUtility;

    public virtual WaterBlob Blob => (WaterBlob)creature.realizedCreature;

    public virtual AbstractCreature? Prey => preyTracker?.currentPrey?.critRep?.representedCreature;

    public virtual AbstractCreature? Threat => threatTracker?.mostThreateningCreature?.representedCreature;

    public WaterBlobAI(AbstractCreature creature, World world) : base(creature, world)
    {
        AddModule(new ThreatTracker(this, 3));
        AddModule(new PreyTracker(this, 6, 1f, 8f, 60f, .75f));
        AddModule(new Tracker(this, 5, 5, 30, .5f, 5, 5, 10));
        AddModule(new RainTracker(this));
        AddModule(new UtilityComparer(this));
        AddModule(new StandardPather(this, world, creature));
        utilityComparer.AddComparedModule(threatTracker, null, 1f, 1.1f);
        utilityComparer.AddComparedModule(preyTracker, null, .4f, 1.1f);
        utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
        AddModule(new DenFinder(this, creature));
        AddModule(new RelationshipTracker(this, tracker));
    }

    public override bool WantToStayInDenUntilEndOfCycle() => base.WantToStayInDenUntilEndOfCycle() || Blob?.Saturated == 1f;

    public virtual RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new();

    public virtual CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
    {
        var crit = dRelation.trackerRep.representedCreature;
        if (dRelation.trackerRep.VisualContact)
            dRelation.state.alive = crit.state.alive;
        var result = StaticRelationship(crit);
        if (result.type == CreatureTemplate.Relationship.Type.Eats && !dRelation.state.alive)
            result.intensity = 0f;
        return result;
    }

    public virtual AIModule? ModuleToTrackRelationship(CreatureTemplate.Relationship relationship) => relationship.type.value switch
    {
        nameof(CreatureTemplate.Relationship.Type.Afraid) => threatTracker,
        nameof(CreatureTemplate.Relationship.Type.Eats) => preyTracker,
        _ => null,
    };

    public override void Update()
    {
        base.Update();
        if (Blob is WaterBlob blob)
        {
            var aIModule = utilityComparer.HighestUtilityModule();
            CurrentUtility = utilityComparer.HighestUtility();
            JumpUrgency = (int)Mathf.Lerp(35f, 10f, CurrentUtility);
            utilityComparer.GetUtilityTracker(preyTracker).weight = blob.Saturated < 1f ? 1 : 0;
            switch (aIModule)
            {
                case ThreatTracker:
                    Behav = Behavior.Fleeing;
                    break;
                case PreyTracker:
                    Behav = Behavior.Hunting;
                    break;
                case RainTracker:
                    Behav = Behavior.GoHome;
                    break;
            }
            if (CurrentUtility < .02f)
                Behav = !(blob.Saturated < 1f) ? Behavior.GoHome : Behavior.Idle;
            switch (Behav?.value)
            {
                case nameof(Behavior.Idle):
                    SetDestination(creature.pos);
                    break;
                case nameof(Behavior.Hunting):
                    SetDestination(preyTracker.currentPrey.critRep.BestGuessForPosition());
                    break;
                case nameof(Behavior.Fleeing):
                    SetDestination(threatTracker.FleeTo(creature.pos, 10, 30, false));
                    break;
                case nameof(Behavior.GoHome):
                    if (denFinder.denPosition is WorldCoordinate coord)
                    {
                        SetDestination(coord);
                        break;
                    }
                    Behav = Behavior.Idle;
                    JumpUrgency = 10;
                    break;
            }
            if (Behav == Behavior.Hunting && Prey?.realizedCreature is Creature c && Vector2.Distance(blob.firstChunk.pos, c.firstChunk.pos) <= 15f)
                blob.EatSomething(c);
        }
    }
}