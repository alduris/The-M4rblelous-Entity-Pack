using Fisobs.Creatures;
using Fisobs.Core;
using Fisobs.Sandbox;
using UnityEngine;
using System.Collections.Generic;
using DevInterface;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;
// CHK
sealed class NoodleEaterCritob : Critob, ISandboxHandler
{
    internal NoodleEaterCritob() : base(CreatureTemplateType.NoodleEater)
    {
        Icon = new SimpleIcon("Kill_NoodleEater", NoodleEater.NEatColor);
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new(.5f, .5f);
        RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.NoodleEater);
    }

    public override int ExpeditionScore() => 3;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => NoodleEater.NEatColor;

    public override string DevtoolsMapName(AbstractCreature acrit) => "Nea";

    public override IEnumerable<string> WorldFileAliases() => ["noodleeater", "noodle eater"];

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.Lizards,
        RoomAttractivenessPanel.Category.LikesInside
    ];

    public override CreatureTemplate CreateTemplate() => LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard), StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard));

    public override void EstablishRelationships()
    {
        var nl = new Relationships(Type);
        var entries = CreatureTemplate.Type.values.entries;
        for (var i = 0; i < entries.Count; i++)
            nl.Fears(new(entries[i]), 1f);
        nl.Ignores(CreatureTemplate.Type.LizardTemplate);
        nl.Ignores(CreatureTemplate.Type.TubeWorm);
        nl.Ignores(CreatureTemplate.Type.Hazer);
        nl.Ignores(CreatureTemplate.Type.VultureGrub);
        nl.Ignores(CreatureTemplate.Type.Deer);
        nl.Ignores(CreatureTemplate.Type.SmallCentipede);
        nl.Ignores(CreatureTemplate.Type.EggBug);
        nl.Ignores(CreatureTemplate.Type.Scavenger);
        nl.Ignores(CreatureTemplate.Type.Overseer);
        nl.Ignores(CreatureTemplate.Type.JetFish);
        nl.Fears(CreatureTemplate.Type.Slugcat, .25f);
        nl.Fears(CreatureTemplate.Type.GreenLizard, 1f);
        nl.Fears(CreatureTemplate.Type.CyanLizard, 1f);
        nl.Fears(CreatureTemplate.Type.RedLizard, 1f);
        nl.Fears(CreatureTemplateType.HunterSeeker, 1f);
        nl.Fears(CreatureTemplate.Type.BigNeedleWorm, .25f);
        nl.Eats(CreatureTemplate.Type.Fly, .2f);
        nl.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        nl.IgnoredBy(CreatureTemplate.Type.LizardTemplate);
        nl.IgnoredBy(CreatureTemplate.Type.Slugcat);
        nl.IgnoredBy(CreatureTemplate.Type.Deer);
        nl.IgnoredBy(CreatureTemplate.Type.Scavenger);
        nl.IgnoredBy(CreatureTemplate.Type.EggBug);
        nl.IgnoredBy(CreatureTemplate.Type.Hazer);
        nl.IgnoredBy(CreatureTemplate.Type.VultureGrub);
        nl.IgnoredBy(CreatureTemplate.Type.JetFish);
        nl.EatenBy(CreatureTemplate.Type.GreenLizard, 1f);
        nl.EatenBy(CreatureTemplate.Type.RedLizard, 1f);
        nl.EatenBy(CreatureTemplate.Type.MirosBird, 1f);
        nl.EatenBy(CreatureTemplate.Type.CyanLizard, 1f);
        nl.EatenBy(CreatureTemplateType.HunterSeeker, 1f);
        nl.EatenBy(CreatureTemplateType.WaterSpitter, 1f);
        nl.Fears(CreatureTemplateType.WaterSpitter, 1f);
        nl.EatenBy(CreatureTemplate.Type.Vulture, 1f);
        nl.EatenBy(CreatureTemplate.Type.BigEel, 1f);
        nl.FearedBy(CreatureTemplate.Type.Fly, .2f);
        nl.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 1f);
        nl.AttackedBy(CreatureTemplate.Type.BigNeedleWorm, .05f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new LizardAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new NoodleEater(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new LizardState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.GreenLizard;

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