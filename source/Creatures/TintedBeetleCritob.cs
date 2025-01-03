using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class TintedBeetleCritob : Critob
{
    internal TintedBeetleCritob() : base(CreatureTemplateType.TintedBeetle)
    {
        Icon = new SimpleIcon("Kill_TintedBeetle", TintedBeetle.BugCol);
        RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.TintedBeetle);
        SandboxPerformanceCost = new(.3f, .4f);
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override int ExpeditionScore() => 3;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => TintedBeetle.BugCol;

    public override string DevtoolsMapName(AbstractCreature acrit) => "tbe";

    public override IEnumerable<string> WorldFileAliases() => ["tintedbeetle", "tinted beetle"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.EggBug, this)
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
        t.instantDeathDamageLimit = .9f;
        t.offScreenSpeed = .5f;
        t.meatPoints = 2;
        t.communityInfluence = .3f;
        t.waterVision = 2f;
        t.pickupAction = "Grab";
        t.throwAction = "Release";
        t.jumpAction = "N/A";
        t.grasps = 1;
        t.movementBasedVision = 1f;
        t.scaryness = .4f;
        t.deliciousness = .2f;
        t.bodySize = .4f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 2;
        t.visualRadius = 1000f;
        t.communityInfluence = 1f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.waterPathingResistance = 40f;
        t.canSwim = true;
        t.usesNPCTransportation = true;
        t.communityID = CommunityID.TintedBeetles;
        t.socialMemory = true;
        t.dangerousToPlayer = 0f;
        return t;
    }

    public override void EstablishRelationships()
    {
        var t = new Relationships(Type);
        t.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 1f);
        t.IsInPack(Type, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new TintedBeetleAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new TintedBeetle(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.EggBug;
}