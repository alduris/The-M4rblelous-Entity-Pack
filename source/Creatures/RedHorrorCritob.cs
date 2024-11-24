using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using MoreSlugcats;

namespace LBMergedMods.Creatures;

sealed class RedHorrorCritob : Critob
{
    internal RedHorrorCritob() : base(CreatureTemplateType.RedHorrorCenti)
    {
        Icon = new SimpleIcon("Kill_Centiwing", new(46f / 51f, .05490196f, .05490196f));
        RegisterUnlock(KillScore.Configurable(29), SandboxUnlockID.RedHorrorCenti);
        SandboxPerformanceCost = new(1f, .7f);
        ShelterDanger = ShelterDanger.TooLarge;
    }

    public override int ExpeditionScore() => 29;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(46f / 51f, .05490196f, .05490196f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "RHC";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override void CorpseIsEdible(Player player, Creature crit, ref bool canEatMeat)
    {
        if (ModManager.MSC && (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
            canEatMeat = false;
        else
            canEatMeat = crit.dead;
    }

    public override IEnumerable<string> WorldFileAliases() => ["redhorrorcenti", "redhorror", "redhorrorcentipede"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.RedCentipede, Type, "RedHorrorCentipede")
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(1f, Allowed),
                Wall = new(1f, Allowed),
                Ceiling = new(1f, Allowed),
                Air = new(1f, Allowed)
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(3f, Allowed),
                ReachOverGap = new(3f, Allowed),
                DoubleReachUp = new(2f, Allowed),
                SemiDiagonalReach = new(2f, Allowed),
                NPCTransportation = new(25f, Allowed),
                OffScreenMovement = new(1f, Allowed),
                BetweenRooms = new(10f, Allowed),
                Slope = new(1.5f, Allowed),
                DropToFloor = new(5f, Allowed),
                DropToClimb = new(5f, Allowed),
                ShortCut = new(1f, Allowed),
                ReachUp = new(1.1f, Allowed),
                ReachDown = new(1.1f, Allowed),
                CeilingSlope = new(2f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1f),
            DamageResistances = new() { Base = 1f, Electric = 102f },
            StunResistances = new() { Base = .75f, Electric = 102f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BlueLizard)
        }.IntoTemplate();
        t.canFly = true;
        t.dangerousToPlayer = 1f;
        t.visualRadius = 1200f;
        t.offScreenSpeed = .45f;
        t.abstractedLaziness = 50;
        t.waterVision = .6f;
        t.lungCapacity = 1100f;
        return t;
    }

    public override void EstablishRelationships()
    {
        var rhc = new Relationships(Type);
        rhc.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new RedHorrorAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new RedHorror(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new Centipede.CentipedeState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centiwing;
}