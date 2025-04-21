using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using MoreSlugcats;

namespace LBMergedMods.Creatures;
// CHK
sealed class SurfaceSwimmerCritob : Critob
{
    internal SurfaceSwimmerCritob() : base(CreatureTemplateType.SurfaceSwimmer)
    {
        Icon = new SimpleIcon("Kill_BHBug", SurfaceSwimmer.BugCol);
        RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.SurfaceSwimmer);
        SandboxPerformanceCost = new(.3f, .4f);
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesWater];

    public override void CorpseIsEdible(Player player, Creature crit, ref bool canEatMeat)
    {
        if (ModManager.MSC && (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
            canEatMeat = false;
        else
            canEatMeat = crit.dead;
    }

    public override int ExpeditionScore() => 3;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => SurfaceSwimmer.BugCol;

    public override string DevtoolsMapName(AbstractCreature acrit) => "surf";

    public override IEnumerable<string> WorldFileAliases() => ["surfacewalker", "surfaceswimmer", "surface walker", "surface swimmer"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.EggBug, Type, "Surface Swimmer")
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(1f, Allowed)
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
                SideHighway = new(1f, Allowed),
                OpenDiagonal = new(3f, Allowed),
                ReachOverGap = new(3f, Allowed),
                ReachUp = new(2f, Allowed),
                SemiDiagonalReach = new(2f, Allowed),
                ReachDown = new(2f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, .1f),
            DamageResistances = new() { Base = 1.2f, Water = 200f },
            StunResistances = new() { Base = 2f, Water = 200f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.PinkLizard)
        }.IntoTemplate();
        t.abstractedLaziness = 10;
        t.instantDeathDamageLimit = float.MaxValue;
        t.offScreenSpeed = .5f;
        t.waterPathingResistance = 1f;
        t.meatPoints = 3;
        t.communityInfluence = .3f;
        t.waterVision = 2f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        return t;
    }

    public override void EstablishRelationships()
    {
        var surf = new Relationships(Type);
        surf.IgnoredBy(CreatureTemplate.Type.Leech);
        surf.IgnoredBy(CreatureTemplate.Type.JetFish);
        surf.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new SurfaceSwimmerAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new SurfaceSwimmer(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.EggBug;
}