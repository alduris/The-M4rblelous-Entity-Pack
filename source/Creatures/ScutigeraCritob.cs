﻿using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using RWCustom;
using UnityEngine;
using DevInterface;
using MoreSlugcats;

namespace LBMergedMods.Creatures;
// CHK
sealed class ScutigeraCritob : Critob
{
    internal ScutigeraCritob() : base(CreatureTemplateType.Scutigera)
    {
        Icon = new SimpleIcon("Kill_Scutigera", Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f));
        RegisterUnlock(KillScore.Configurable(13), SandboxUnlockID.Scutigera);
        SandboxPerformanceCost = new(.8f, .5f);
        ShelterDanger = ShelterDanger.Hostile;
    }

    public override int ExpeditionScore() => 13;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "scut";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override void CorpseIsEdible(Player player, Creature crit, ref bool canEatMeat)
    {
        if (ModManager.MSC && (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
            canEatMeat = false;
        else
            canEatMeat = crit.dead;
    }

    public override IEnumerable<string> WorldFileAliases() => ["scut", "scutigera", "scuti"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.Centipede, Type, "Scutigera")
        {
            TileResistances = new()
            {
                OffScreen = new(1f, Allowed),
                Floor = new(1f, Allowed),
                Corridor = new(1f, Allowed),
                Climb = new(1f, Allowed),
                Wall = new(1f, Allowed),
                Ceiling = new(1f, Allowed)
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
            DamageResistances = new() { Base = .75f, Stab = .4f, Blunt = .4f, Water = .5f, Explosion = .5f, Electric = 102f },
            StunResistances = new() { Base = .6f, Explosion = .3f, Blunt = .4f, Water = 1f, Electric = 102f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BlueLizard)
        }.IntoTemplate();
        t.quickDeath = false;
        t.offScreenSpeed = .3f;
        t.grasps = 2;
        t.abstractedLaziness = 150;
        t.requireAImap = true;
        t.bodySize = 1.2f;
        t.stowFoodInDen = true;
        t.shortcutSegments = 3;
        t.doubleReachUpConnectionParams = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard).doubleReachUpConnectionParams;
        t.visualRadius = 2000f;
        t.waterVision = .4f;
        t.throughSurfaceVision = .85f;
        t.movementBasedVision = 0f;
        t.dangerousToPlayer = .4f;
        t.communityInfluence = .25f;
        t.meatPoints = 1;
        t.lungCapacity = 900f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.canSwim = true;
        return t;
    }

    public override void EstablishRelationships()
    {
        var scut = new Relationships(Type);
        scut.EatenBy(CreatureTemplate.Type.BigSpider, .3f);
        scut.EatenBy(CreatureTemplate.Type.LizardTemplate, .8f);
        scut.EatenBy(CreatureTemplate.Type.MirosBird, .8f);
        scut.EatenBy(CreatureTemplate.Type.Vulture, .5f);
        scut.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        scut.FearedBy(CreatureTemplate.Type.Scavenger, .4f);
        scut.FearedBy(CreatureTemplate.Type.Slugcat, .3f);
        scut.Eats(CreatureTemplate.Type.BigSpider, .25f);
        scut.Fears(CreatureTemplate.Type.BigEel, .6f);
        scut.Fears(CreatureTemplate.Type.DaddyLongLegs, .6f);
        scut.Fears(CreatureTemplate.Type.DropBug, .2f);
        scut.Fears(CreatureTemplate.Type.LizardTemplate, .4f);
        scut.Fears(CreatureTemplate.Type.MirosBird, .7f);
        scut.Fears(CreatureTemplate.Type.SpitterSpider, .2f);
        scut.Fears(CreatureTemplate.Type.TempleGuard, .4f);
        scut.Fears(CreatureTemplate.Type.TentaclePlant, .5f);
        scut.Fears(CreatureTemplate.Type.Vulture, .6f);
        scut.IgnoredBy(CreatureTemplate.Type.Centipede);
        scut.Ignores(CreatureTemplate.Type.Centipede);
        scut.Ignores(CreatureTemplate.Type.Deer);
        scut.Ignores(CreatureTemplate.Type.GarbageWorm);
        scut.Ignores(CreatureTemplate.Type.PoleMimic);
        scut.Ignores(Type);
        scut.IgnoredBy(CreatureTemplateType.ThornBug);
        scut.Ignores(CreatureTemplateType.ThornBug);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new ScutigeraAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Scutigera(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new Centipede.CentipedeState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centipede;
}