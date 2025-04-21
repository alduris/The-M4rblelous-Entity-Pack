using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;
// CHK
sealed class KillerpillarCritob : Critob, ISandboxHandler
{
    internal KillerpillarCritob() : base(CreatureTemplateType.Killerpillar)
    {
        Icon = new SimpleIcon("Kill_CommonCaterpillar", Ext.MenuGrey);
        RegisterUnlock(KillScore.Configurable(7), SandboxUnlockID.Killerpillar);
        SandboxPerformanceCost = new(.8f, .5f);
        ShelterDanger = ShelterDanger.Hostile;
    }

    public override int ExpeditionScore() => 7;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Ext.MenuGrey;

    public override string DevtoolsMapName(AbstractCreature acrit) => "kctp";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override IEnumerable<string> WorldFileAliases() => ["killerpillar", "killer pillar"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
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
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = 1f, Stab = .2f, Blunt = .4f },
            StunResistances = new() { Base = .4f, Blunt = .4f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BlueLizard)
        }.IntoTemplate();
        t.quickDeath = false;
        t.offScreenSpeed = .3f;
        t.grasps = 1;
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
        t.communityInfluence = .2f;
        t.lungCapacity = 900f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
        t.canSwim = true;
        t.meatPoints = 4;
        t.doPreBakedPathing = false;
        t.usesNPCTransportation = true;
        t.pickupAction = "Grab";
        t.throwAction = "Release";
        return t;
    }

    public override void EstablishRelationships()
    {
        var ctp = new Relationships(Type);
        ctp.Ignores(CreatureTemplate.Type.Overseer);
        ctp.Eats(CreatureTemplate.Type.Slugcat, .8f);
        ctp.Eats(CreatureTemplate.Type.LanternMouse, 1f);
        ctp.Fears(CreatureTemplate.Type.LizardTemplate, 1f);
        ctp.Fears(CreatureTemplate.Type.BlueLizard, .5f);
        ctp.Ignores(CreatureTemplate.Type.Fly);
        ctp.Fears(CreatureTemplate.Type.Leech, .5f);
        ctp.Eats(CreatureTemplate.Type.Snail, .6f);
        ctp.Eats(CreatureTemplate.Type.CicadaA, 1f);
        ctp.Ignores(CreatureTemplate.Type.Spider);
        ctp.Eats(CreatureTemplate.Type.JetFish, .6f);
        ctp.Eats(CreatureTemplate.Type.TubeWorm, 1f);
        ctp.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
        ctp.Eats(CreatureTemplate.Type.Scavenger, .8f);
        ctp.Eats(CreatureTemplate.Type.VultureGrub, .8f);
        ctp.Eats(CreatureTemplate.Type.Hazer, .75f);
        ctp.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        ctp.Eats(CreatureTemplate.Type.BigNeedleWorm, 1f);
        ctp.Eats(CreatureTemplate.Type.EggBug, 1f);
        ctp.Eats(CreatureTemplate.Type.BigSpider, .2f);
        ctp.Eats(CreatureTemplate.Type.DropBug, .2f);
        ctp.Fears(CreatureTemplate.Type.Vulture, 1f);
        ctp.Ignores(CreatureTemplate.Type.SpitterSpider);
        ctp.Fears(CreatureTemplate.Type.RedCentipede, 1f);
        ctp.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
        ctp.FearedBy(CreatureTemplate.Type.LanternMouse, 1f);
        ctp.EatenBy(CreatureTemplate.Type.LizardTemplate, 1f);
        ctp.EatenBy(CreatureTemplate.Type.BlueLizard, .5f);
        ctp.IgnoredBy(CreatureTemplate.Type.Fly);
        ctp.EatenBy(CreatureTemplate.Type.Leech, .5f);
        ctp.FearedBy(CreatureTemplate.Type.Snail, .6f);
        ctp.FearedBy(CreatureTemplate.Type.CicadaA, 1f);
        ctp.IgnoredBy(CreatureTemplate.Type.Spider);
        ctp.FearedBy(CreatureTemplate.Type.JetFish, .6f);
        ctp.FearedBy(CreatureTemplate.Type.TubeWorm, 1f);
        ctp.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        ctp.FearedBy(CreatureTemplate.Type.Scavenger, .8f);
        ctp.FearedBy(CreatureTemplate.Type.VultureGrub, .8f);
        ctp.FearedBy(CreatureTemplate.Type.Hazer, .75f);
        ctp.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        ctp.FearedBy(CreatureTemplate.Type.BigNeedleWorm, 1f);
        ctp.FearedBy(CreatureTemplate.Type.EggBug, 1f);
        ctp.FearedBy(CreatureTemplate.Type.BigSpider, .2f);
        ctp.FearedBy(CreatureTemplate.Type.DropBug, .2f);
        ctp.EatenBy(CreatureTemplate.Type.Vulture, 1f);
        ctp.IgnoredBy(CreatureTemplate.Type.SpitterSpider);
        ctp.EatenBy(CreatureTemplate.Type.RedCentipede, 1f);
        ctp.Fears(CreatureTemplate.Type.MirosBird, 1f);
        ctp.Fears(CreatureTemplate.Type.BigEel, 1f);
        ctp.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        ctp.Ignores(CreatureTemplate.Type.TempleGuard);
        ctp.Fears(CreatureTemplate.Type.PoleMimic, 1f);
        ctp.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
        ctp.EatenBy(CreatureTemplate.Type.MirosBird, 1f);
        ctp.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        ctp.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        ctp.IgnoredBy(CreatureTemplate.Type.TempleGuard);
        ctp.EatenBy(CreatureTemplate.Type.PoleMimic, 1f);
        ctp.EatenBy(CreatureTemplate.Type.TentaclePlant, 1f);
        if (ModManager.DLCShared)
        {
            ctp.Eats(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            ctp.FearedBy(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            ctp.Fears(DLCSharedEnums.CreatureTemplateType.AquaCenti, 1f);
            ctp.EatenBy(DLCSharedEnums.CreatureTemplateType.AquaCenti, 1f);
            ctp.Fears(DLCSharedEnums.CreatureTemplateType.BigJelly, 1f);
            ctp.EatenBy(DLCSharedEnums.CreatureTemplateType.BigJelly, 1f);
            ctp.Ignores(DLCSharedEnums.CreatureTemplateType.Inspector);
            ctp.IgnoredBy(DLCSharedEnums.CreatureTemplateType.Inspector);
        }
        ctp.Eats(CreatureTemplateType.HazerMom, 1f);
        ctp.Eats(CreatureTemplateType.Hoverfly, 1f);
        ctp.Eats(CreatureTemplateType.SurfaceSwimmer, 1f);
        ctp.Eats(CreatureTemplateType.ThornBug, 1f);
        ctp.Eats(CreatureTemplateType.TintedBeetle, 1f);
        ctp.Eats(CreatureTemplateType.WaterBlob, 1f);
        ctp.Eats(CreatureTemplateType.BouncingBall, 1f);
        ctp.Fears(CreatureTemplateType.Polliwog, .5f);
        ctp.Eats(CreatureTemplateType.DivingBeetle, .2f);
        ctp.Ignores(CreatureTemplateType.NoodleEater);
        ctp.Fears(CreatureTemplateType.MiniBlackLeech, .5f);
        ctp.Fears(CreatureTemplateType.HunterSeeker, 1f);
        ctp.Fears(CreatureTemplateType.MoleSalamander, 1f);
        ctp.Fears(CreatureTemplateType.WaterSpitter, 1f);
        ctp.Fears(CreatureTemplateType.Sporantula, 1f);
        ctp.Fears(CreatureTemplateType.FatFireFly, 1f);
        ctp.FearedBy(CreatureTemplateType.HazerMom, 1f);
        ctp.FearedBy(CreatureTemplateType.Hoverfly, 1f);
        ctp.FearedBy(CreatureTemplateType.SurfaceSwimmer, 1f);
        ctp.FearedBy(CreatureTemplateType.ThornBug, 1f);
        ctp.FearedBy(CreatureTemplateType.TintedBeetle, 1f);
        ctp.FearedBy(CreatureTemplateType.WaterBlob, 1f);
        ctp.FearedBy(CreatureTemplateType.BouncingBall, 1f);
        ctp.EatenBy(CreatureTemplateType.Polliwog, .5f);
        ctp.FearedBy(CreatureTemplateType.DivingBeetle, .2f);
        ctp.FearedBy(CreatureTemplateType.NoodleEater, .1f);
        ctp.EatenBy(CreatureTemplateType.MiniBlackLeech, .5f);
        ctp.EatenBy(CreatureTemplateType.HunterSeeker, 1f);
        ctp.EatenBy(CreatureTemplateType.MoleSalamander, 1f);
        ctp.EatenBy(CreatureTemplateType.WaterSpitter, 1f);
        ctp.EatenBy(CreatureTemplateType.Sporantula, 1f);
        ctp.EatenBy(CreatureTemplateType.FatFireFly, 1f);
        ctp.Fears(CreatureTemplateType.MiniLeviathan, 1f);
        ctp.Fears(CreatureTemplateType.MiniFlyingBigEel, 1f);
        ctp.Fears(CreatureTemplateType.FatFireFly, 1f);
        ctp.Fears(CreatureTemplateType.FlyingBigEel, 1f);
        ctp.Fears(CreatureTemplateType.Blizzor, 1f);
        ctp.Fears(CreatureTemplateType.SilverLizard, 1f);
        ctp.Ignores(CreatureTemplateType.Scutigera);
        ctp.Fears(CreatureTemplateType.RedHorrorCenti, 1f);
        ctp.Fears(CreatureTemplateType.CommonEel, .4f);
        ctp.EatenBy(CreatureTemplateType.MiniLeviathan, 1f);
        ctp.EatenBy(CreatureTemplateType.MiniFlyingBigEel, 1f);
        ctp.EatenBy(CreatureTemplateType.FatFireFly, 1f);
        ctp.EatenBy(CreatureTemplateType.FlyingBigEel, 1f);
        ctp.EatenBy(CreatureTemplateType.Blizzor, 1f);
        ctp.EatenBy(CreatureTemplateType.SilverLizard, 1f);
        ctp.IgnoredBy(CreatureTemplateType.Scutigera);
        ctp.EatenBy(CreatureTemplateType.RedHorrorCenti, 1f);
        ctp.EatenBy(CreatureTemplateType.CommonEel, .4f);
        ctp.Ignores(Type);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new CaterpillarAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Caterpillar(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Centipede;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        if (Random.value < .1f)
            abstractCreature.superSizeMe = true;
        Random.state = state;
        return abstractCreature;
    }
}