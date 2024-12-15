using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class DivingBeetleCritob : Critob
{
    internal DivingBeetleCritob() : base(CreatureTemplateType.DivingBeetle)
    {
        Icon = new SimpleIcon("Kill_DivingBeetle", Color.Lerp(DivingBeetleGraphics.BugCol, Color.white, .2f));
        ShelterDanger = ShelterDanger.Hostile;
        RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.DivingBeetle);
        SandboxPerformanceCost = new(.5f, .5f);
        LoadedPerformanceCost = 20f;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override int ExpeditionScore() => 4;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.Lerp(DivingBeetleGraphics.BugCol, Color.white, .2f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "dvb";

    public override IEnumerable<string> WorldFileAliases() => ["divingbeetle", "diving beetle"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(1f, Allowed),
                ReachUp = new(1f, Allowed),
                ReachDown = new(1f, Allowed),
                SemiDiagonalReach = new(1f, Allowed),
                DropToWater = new(1f, Allowed),
                ShortCut = new(1.5f, Allowed),
                NPCTransportation = new(3f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(5f, Allowed),
                SeaHighway = new(1f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 1.1f, Water = 200f },
            StunResistances = new() { Base = .7f, Water = 200f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Leech)
        }.IntoTemplate();
        t.instantDeathDamageLimit = float.MaxValue;
        t.roamBetweenRoomsChance = .03f;
        t.offScreenSpeed = .3f;
        t.waterPathingResistance = 2f;
        t.communityInfluence = .15f;
        t.waterVision = 2f;
        t.movementBasedVision = .95f;
        t.dangerousToPlayer = .5f;
        t.pickupAction = "Grab";
        t.visualRadius = 1000f;
        t.scaryness = 1.1f;
        t.throughSurfaceVision = .678f;
        t.BlizzardAdapted = true;
        t.BlizzardWanderer = true;
        t.waterRelationship = CreatureTemplate.WaterRelationship.WaterOnly;
        t.abstractedLaziness = 200;
        t.AI = true;
        t.requireAImap = true;
        t.bodySize = 1.2f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 2;
        t.grasps = 1;
        t.canSwim = true;
        t.meatPoints = 3;
        t.throwAction = "Release";
        t.usesNPCTransportation = true;
        t.lungCapacity = float.PositiveInfinity;
        t.relationships = (CreatureTemplate.Relationship[])StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.DropBug).relationships.Clone();
        return t;
    }

    public override void EstablishRelationships()
    {
        var dvb = new Relationships(Type);
        dvb.IgnoredBy(CreatureTemplate.Type.Leech);
        dvb.IgnoredBy(CreatureTemplate.Type.JetFish);
        dvb.Ignores(Type);
        dvb.Ignores(CreatureTemplate.Type.Deer);
        dvb.IgnoredBy(CreatureTemplate.Type.Deer);
        dvb.Ignores(CreatureTemplate.Type.Leech);
        dvb.Ignores(CreatureTemplate.Type.JetFish);
        dvb.Ignores(CreatureTemplate.Type.Hazer);
        dvb.IgnoredBy(CreatureTemplate.Type.Hazer);
        dvb.Ignores(CreatureTemplate.Type.GarbageWorm);
        dvb.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        dvb.IgnoredBy(CreatureTemplate.Type.DropBug);
        dvb.Ignores(CreatureTemplate.Type.DropBug);
        dvb.Eats(CreatureTemplate.Type.LizardTemplate, .5f);
        dvb.Fears(CreatureTemplate.Type.RedLizard, .5f);
        dvb.Fears(CreatureTemplate.Type.GreenLizard, .25f);
        dvb.Fears(CreatureTemplate.Type.MirosBird, .8f);
        dvb.EatenBy(CreatureTemplate.Type.MirosBird, .85f);
        dvb.Fears(CreatureTemplate.Type.BigEel, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new DivingBeetleAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new DivingBeetle(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.JetFish;
}