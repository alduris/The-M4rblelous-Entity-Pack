using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class ThornBugCritob : Critob
{
    internal ThornBugCritob() : base(CreatureTemplateType.ThornBug)
    {
        Icon = new SimpleIcon("Kill_ThornBug", ThornBug.BugCol);
        RegisterUnlock(KillScore.Configurable(4), SandboxUnlockID.ThornBug);
        SandboxPerformanceCost = new(.3f, .4f);
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override int ExpeditionScore() => 4;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => ThornBug.BugCol;

    public override string DevtoolsMapName(AbstractCreature acrit) => "thbug";

    public override IEnumerable<string> WorldFileAliases() => ["thornbug"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(3f, Allowed),
                Wall = new(4f, Allowed),
                Ceiling = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                DropToFloor = new(10f, Allowed),
                DropToWater = new(10f, Allowed),
                DropToClimb = new(10f, Allowed),
                ShortCut = new(1.5f, Allowed),
                NPCTransportation = new(3f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(5f, Allowed),
                Slope = new(1.5f, Allowed),
                OpenDiagonal = new(3f, Allowed),
                ReachOverGap = new(3f, Allowed),
                ReachUp = new(2f, Allowed),
                SemiDiagonalReach = new(2f, Allowed),
                ReachDown = new(2f, Allowed),
                CeilingSlope = new(2f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, .1f),
            DamageResistances = new() { Base = 1.1f },
            StunResistances = new() { Base = 1.1f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.WhiteLizard)
        }.IntoTemplate();
        t.abstractedLaziness = 30;
        t.instantDeathDamageLimit = 1.2f;
        t.offScreenSpeed = .5f;
        t.meatPoints = 2;
        t.communityInfluence = .3f;
        t.waterVision = 2f;
        t.pickupAction = "Grab";
        t.throwAction = "Release";
        t.jumpAction = "Jump";
        t.grasps = 1;
        t.wormGrassImmune = true;
        t.wormgrassTilesIgnored = true;
        t.movementBasedVision = 1f;
        t.dangerousToPlayer = .7f;
        t.scaryness = .5f;
        t.deliciousness = 0f;
        t.bodySize = .4f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 2;
        t.visualRadius = 1200f;
        t.communityInfluence = .1f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.waterPathingResistance = 40f;
        t.canSwim = true;
        t.usesNPCTransportation = true;
        return t;
    }

    public override void EstablishRelationships()
    {
        var t = new Relationships(Type);
        t.Ignores(Type);
        t.Eats(CreatureTemplate.Type.Slugcat, .6f);
        t.Fears(CreatureTemplate.Type.LizardTemplate, 1f);
        t.Ignores(CreatureTemplate.Type.Fly);
        t.Fears(CreatureTemplate.Type.Leech, .3f);
        t.Fears(CreatureTemplate.Type.SeaLeech, .5f);
        t.Eats(CreatureTemplate.Type.Snail, .6f);
        t.Ignores(CreatureTemplate.Type.GarbageWorm);
        t.Eats(CreatureTemplate.Type.LanternMouse, 1f);
        t.Ignores(CreatureTemplate.Type.Spider);
        t.Eats(CreatureTemplate.Type.CicadaA, 1f);
        t.Eats(CreatureTemplate.Type.CicadaB, 1f);
        t.Eats(CreatureTemplate.Type.JetFish, 1f);
        t.Fears(CreatureTemplate.Type.BigEel, 1f);
        t.Ignores(CreatureTemplate.Type.Deer);
        t.Eats(CreatureTemplate.Type.TubeWorm, 1f);
        t.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        t.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
        t.Fears(CreatureTemplate.Type.PoleMimic, 1f);
        t.Fears(CreatureTemplate.Type.MirosBird, 1f);
        t.Ignores(CreatureTemplate.Type.TempleGuard);
        t.Eats(CreatureTemplate.Type.Centipede, .4f);
        t.Eats(CreatureTemplate.Type.Centiwing, .3f);
        t.Eats(CreatureTemplate.Type.SmallCentipede, .2f);
        t.Fears(CreatureTemplate.Type.RedCentipede, 1f);
        t.Fears(CreatureTemplate.Type.Scavenger, .6f);
        t.Ignores(CreatureTemplate.Type.Overseer);
        t.Ignores(CreatureTemplate.Type.EggBug);
        t.Ignores(CreatureTemplate.Type.BigSpider);
        t.Ignores(CreatureTemplate.Type.SpitterSpider);
        t.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        t.Eats(CreatureTemplate.Type.SmallNeedleWorm, .7f);
        t.Ignores(CreatureTemplate.Type.DropBug);
        t.Fears(CreatureTemplate.Type.Vulture, .8f);
        t.Fears(CreatureTemplate.Type.KingVulture, 1f);
        t.Eats(CreatureTemplate.Type.VultureGrub, .8f);
        t.Eats(CreatureTemplate.Type.Hazer, 1f);
        t.FearedBy(CreatureTemplate.Type.Slugcat, .6f);
        t.EatenBy(CreatureTemplate.Type.LizardTemplate, 1f);
        t.IgnoredBy(CreatureTemplate.Type.BlueLizard);
        t.FearedBy(CreatureTemplate.Type.Fly, 1f);
        t.EatenBy(CreatureTemplate.Type.Leech, .3f);
        t.EatenBy(CreatureTemplate.Type.SeaLeech, .5f);
        t.FearedBy(CreatureTemplate.Type.Snail, .6f);
        t.FearedBy(CreatureTemplate.Type.GarbageWorm, 1f);
        t.FearedBy(CreatureTemplate.Type.LanternMouse, 1f);
        t.IgnoredBy(CreatureTemplate.Type.Spider);
        t.FearedBy(CreatureTemplate.Type.CicadaA, 1f);
        t.FearedBy(CreatureTemplate.Type.CicadaB, 1f);
        t.FearedBy(CreatureTemplate.Type.JetFish, 1f);
        t.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        t.IgnoredBy(CreatureTemplate.Type.Deer);
        t.FearedBy(CreatureTemplate.Type.TubeWorm, 1f);
        t.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        t.EatenBy(CreatureTemplate.Type.TentaclePlant, 1f);
        t.EatenBy(CreatureTemplate.Type.PoleMimic, 1f);
        t.EatenBy(CreatureTemplate.Type.MirosBird, 1f);
        t.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        t.FearedBy(CreatureTemplate.Type.Centipede, .4f);
        t.FearedBy(CreatureTemplate.Type.Centiwing, .3f);
        t.FearedBy(CreatureTemplate.Type.SmallCentipede, .2f);
        t.EatenBy(CreatureTemplate.Type.RedCentipede, 1f);
        t.AttackedBy(CreatureTemplate.Type.Scavenger, .6f);
        t.IgnoredBy(CreatureTemplate.Type.EggBug);
        t.IgnoredBy(CreatureTemplate.Type.BigSpider);
        t.IgnoredBy(CreatureTemplate.Type.SpitterSpider);
        t.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        t.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, .7f);
        t.IgnoredBy(CreatureTemplate.Type.DropBug);
        t.EatenBy(CreatureTemplate.Type.Vulture, .8f);
        t.EatenBy(CreatureTemplate.Type.KingVulture, 1f);
        t.FearedBy(CreatureTemplate.Type.VultureGrub, .8f);
        t.FearedBy(CreatureTemplate.Type.Hazer, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ThornBugAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new ThornBug(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.EggBug;
}