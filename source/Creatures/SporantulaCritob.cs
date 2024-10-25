using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class SporantulaCritob : Critob
{
    internal SporantulaCritob() : base(CreatureTemplateType.Sporantula)
    {
        Icon = new SimpleIcon("Kill_BigSpider", new(.9f, 1f, .8f));
        RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.Sporantula);
        ShelterDanger = ShelterDanger.Hostile;
        SandboxPerformanceCost = new(.55f, .65f);
        LoadedPerformanceCost = 50f;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Dark,
        RoomAttractivenessPanel.Category.LikesInside
    ];

    public override void GraspParalyzesPlayer(Creature.Grasp grasp, ref bool paralyzing) => paralyzing = true;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.9f, 1f, .8f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "spo";

    public override IEnumerable<string> WorldFileAliases() => ["sporantula"];

    public override int ExpeditionScore() => 6;

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.SpitterSpider, Type, "Sporantula")
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(1.5f, Allowed),
                Wall = new(3f, Allowed),
                Ceiling = new(3f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(3f, Allowed),
                ReachOverGap = new(3f, Allowed),
                ReachUp = new(2f, Allowed),
                ReachDown = new(2f, Allowed),
                SemiDiagonalReach = new(2f, Allowed),
                DropToFloor = new(10f, Allowed),
                DropToWater = new(10f, Allowed),
                DropToClimb = new(10f, Allowed),
                ShortCut = new(1.5f, Allowed),
                NPCTransportation = new(3f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(5f, Allowed),
                Slope = new(1.5f, Allowed),
                CeilingSlope = new(1.5f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.SpitterSpider),
            DamageResistances = new() { Base = 2.2f },
            StunResistances = new() { Base = 1.2f },
        }.IntoTemplate();
        t.jumpAction = "Pounce and Spit";
        return t;
    }

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        var enums = CreatureTemplate.Type.values.entries;
        foreach (var val in enums)
            s.Ignores(new(val));
        s.FearedBy(CreatureTemplate.Type.BigSpider, 1f);
        s.FearedBy(CreatureTemplate.Type.SpitterSpider, 1f);
        s.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        s.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        s.FearedBy(CreatureTemplate.Type.Centipede, 1f);
        s.FearedBy(CreatureTemplate.Type.RedCentipede, 1f);
        s.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        s.FearedBy(CreatureTemplate.Type.CicadaA, 1f);
        s.FearedBy(CreatureTemplate.Type.CicadaB, 1f);
        s.FearedBy(CreatureTemplate.Type.Centiwing, 1f);
        s.FearedBy(CreatureTemplate.Type.DropBug, 1f);
        s.FearedBy(CreatureTemplate.Type.EggBug, 1f);
        s.Fears(CreatureTemplate.Type.MirosBird, 1f);
        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.Fears(CreatureTemplate.Type.PoleMimic, .7f);
        s.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
        s.Fears(CreatureTemplate.Type.Vulture, .7f);
        s.Fears(CreatureTemplate.Type.KingVulture, 1f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.Fears(CreatureTemplate.Type.LizardTemplate, 1f);
        s.Ignores(CreatureTemplate.Type.BlueLizard);
        s.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new SporantulaAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Sporantula(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.BigSpider;
}