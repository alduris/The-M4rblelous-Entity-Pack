using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using DevInterface;
using UnityEngine;
using System.Collections.Generic;

namespace LBMergedMods.Creatures;
// CHK
sealed class WaterSpitterCritob : Critob
{
    internal WaterSpitterCritob() : base(CreatureTemplateType.WaterSpitter)
    {
        Icon = new SimpleIcon("Kill_WaterSpitter", Color.white);
        LoadedPerformanceCost = 50f;
        SandboxPerformanceCost = new(.5f, .5f);
        RegisterUnlock(KillScore.Configurable(9), SandboxUnlockID.WaterSpitter);
    }

    public override int ExpeditionScore() => 9;

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Salamander;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;

    public override string DevtoolsMapName(AbstractCreature acrit) => "WatSp";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var w = new Relationships(Type);
        w.Eats(CreatureTemplate.Type.CicadaA, .3f);
        w.Eats(CreatureTemplate.Type.LanternMouse, .5f);
        w.Eats(CreatureTemplate.Type.BigSpider, .4f);
        w.Eats(CreatureTemplate.Type.EggBug, .6f);
        w.Eats(CreatureTemplate.Type.JetFish, .05f);
        w.Ignores(CreatureTemplate.Type.BigEel);
        w.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        w.Eats(CreatureTemplate.Type.Centipede, .9f);
        w.Eats(CreatureTemplate.Type.Centiwing, .4f);
        w.Eats(CreatureTemplate.Type.BigNeedleWorm, .3f);
        w.Eats(CreatureTemplate.Type.SmallNeedleWorm, .15f);
        w.Eats(CreatureTemplate.Type.DropBug, .4f);
        w.Fears(CreatureTemplate.Type.RedCentipede, .7f);
        w.Fears(CreatureTemplate.Type.TentaclePlant, .4f);
        w.Eats(CreatureTemplate.Type.Hazer, .4f);
        w.Ignores(CreatureTemplate.Type.Vulture);
        w.Fears(CreatureTemplate.Type.KingVulture, .5f);
        w.Fears(CreatureTemplate.Type.MirosBird, .5f);
        w.Rivals(CreatureTemplate.Type.LizardTemplate, .3f);
        w.Rivals(Type, .05f);
        w.Ignores(CreatureTemplate.Type.BlueLizard);
        w.Rivals(CreatureTemplate.Type.Salamander, .05f);
        w.Ignores(CreatureTemplate.Type.Leech);
        w.Eats(CreatureTemplate.Type.SmallCentipede, .5f);
        w.Eats(CreatureTemplate.Type.Scavenger, .6f);
        w.Eats(CreatureTemplate.Type.Snail, .5f);
        w.Eats(CreatureTemplate.Type.SpitterSpider, .5f);
        w.Ignores(CreatureTemplate.Type.GarbageWorm);
        w.Eats(CreatureTemplate.Type.VultureGrub, .5f);
        w.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .6f);
        w.IgnoredBy(CreatureTemplate.Type.BlueLizard);
        w.FearedBy(CreatureTemplate.Type.BigSpider, .4f);
        w.FearedBy(CreatureTemplate.Type.DropBug, .4f);
        w.IgnoredBy(CreatureTemplate.Type.Vulture);
        w.EatenBy(CreatureTemplate.Type.KingVulture, .5f);
        w.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        w.FearedBy(CreatureTemplate.Type.LanternMouse, .8f);
        w.FearedBy(CreatureTemplate.Type.JetFish, .1f);
        w.IgnoredBy(CreatureTemplate.Type.BigEel);
        w.FearedBy(CreatureTemplate.Type.Centipede, .9f);
        w.FearedBy(CreatureTemplate.Type.Centiwing, .4f);
        w.FearedBy(CreatureTemplate.Type.BigNeedleWorm, .3f);
        w.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, .4f);
        w.EatenBy(CreatureTemplate.Type.RedCentipede, .5f);
        w.EatenBy(CreatureTemplate.Type.TentaclePlant, .4f);
        w.FearedBy(CreatureTemplate.Type.Hazer, .8f);
        w.EatenBy(CreatureTemplate.Type.MirosBird, .5f);
        w.IgnoredBy(CreatureTemplate.Type.Leech);
        w.FearedBy(CreatureTemplate.Type.SmallCentipede, 1f);
        w.FearedBy(CreatureTemplate.Type.Scavenger, .4f);
        w.FearedBy(CreatureTemplate.Type.Snail, 1f);
        w.FearedBy(CreatureTemplate.Type.SpitterSpider, .3f);
        w.IgnoredBy(CreatureTemplate.Type.GarbageWorm);
        w.FearedBy(CreatureTemplate.Type.VultureGrub, 1f);
        w.FearedBy(CreatureTemplate.Type.Slugcat, .9f);
        if (ModManager.DLCShared)
        {
            w.IgnoredBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
            w.Ignores(DLCSharedEnums.CreatureTemplateType.ZoopLizard);
        }
    }

    public override IEnumerable<string> WorldFileAliases() => ["waterspitter", "water spitter"];

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new WaterSpitterAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new WaterSpitter(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }
}