using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using UnityEngine;
using DevInterface;
using Watcher;
using Fisobs.Sandbox;
using System;

namespace LBMergedMods.Creatures;

sealed class XyloWormCritob : Critob, ISandboxHandler
{
    internal XyloWormCritob() : base(CreatureTemplateType.XyloWorm)
    {
        Icon = new SimpleIcon("Kill_XyloWorm", new(1f, 1f, .75f));
        RegisterUnlock(KillScore.Constant(0), SandboxUnlockID.XyloWorm);
        RegisterUnlock(KillScore.Constant(0), SandboxUnlockID.BigXyloWorm, SandboxUnlockID.XyloWorm, M4R_DATA_NUMBER);
        ShelterDanger = ShelterDanger.Hostile;
        SandboxPerformanceCost = new(.35f, .1f);
        LoadedPerformanceCost = 10f;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [];

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(1f, 1f, .75f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "xyw";

    public override IEnumerable<string> WorldFileAliases() => ["xyloworm", "xylo worm"];

    public override CreatureTemplate CreateTemplate()
    {
        var t =new CreatureFormula(CreatureTemplate.Type.VultureGrub, this)
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = .3f },
            StunResistances = new() { Base = .3f },
            HasAI = false
        }.IntoTemplate();
        t.shortcutColor = new(1f, 1f, .75f);
        t.dangerousToPlayer = .1f;
        t.grasps = 1;
        return t;
    }

    public override void EstablishRelationships()
	{
		var self = new Relationships(Type);
        self.Eats(CreatureTemplate.Type.Slugcat, 1f);
        self.FearedBy(CreatureTemplate.Type.Slugcat, .1f);
        self.Eats(CreatureTemplateType.ChipChop, 1f);
        self.FearedBy(CreatureTemplateType.ChipChop, .3f);
        self.Eats(CreatureTemplateType.TintedBeetle, 1f);
        self.FearedBy(CreatureTemplateType.TintedBeetle, .3f);
        self.Eats(CreatureTemplateType.Hoverfly, 1f);
        self.FearedBy(CreatureTemplateType.Hoverfly, .3f);
        self.EatenBy(CreatureTemplateType.ThornBug, 1f);
        self.EatenBy(CreatureTemplateType.NoodleEater, .35f);
        self.EatenBy(CreatureTemplateType.DivingBeetle, 1f);
        self.EatenBy(CreatureTemplate.Type.Vulture, 1f);
        self.EatenBy(CreatureTemplateType.Sporantula, .5f);
        self.EatenBy(CreatureTemplateType.Killerpillar, .5f);
        self.Eats(CreatureTemplate.Type.LizardTemplate, 1f);
        self.FearedBy(CreatureTemplate.Type.LizardTemplate, .1f);
        self.EatenBy(CreatureTemplate.Type.BlueLizard, .35f);
        self.EatenBy(CreatureTemplate.Type.Centipede, 1f);
        self.Eats(CreatureTemplate.Type.Snail, 1f);
        self.FearedBy(CreatureTemplate.Type.Snail, .3f);
        self.Eats(CreatureTemplate.Type.DropBug, 1f);
        self.FearedBy(CreatureTemplate.Type.DropBug, .1f);
        self.Eats(CreatureTemplate.Type.CicadaA, 1f);
        self.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
        self.Eats(CreatureTemplate.Type.EggBug, 1f);
        self.FearedBy(CreatureTemplate.Type.EggBug, .3f);
        self.Eats(CreatureTemplate.Type.Deer, 1f);
        self.FearedBy(CreatureTemplate.Type.Deer, .1f);
        self.Eats(CreatureTemplate.Type.Scavenger, 1f);
        self.AttackedBy(CreatureTemplate.Type.Scavenger, .5f);
        if (ModManager.DLCShared)
        {
            self.Eats(DLCSharedEnums.CreatureTemplateType.Yeek, 1f);
            self.FearedBy(DLCSharedEnums.CreatureTemplateType.Yeek, .3f);
            self.EatenBy(DLCSharedEnums.CreatureTemplateType.ZoopLizard, .5f);
        }
        if (ModManager.Watcher)
        {
            self.Eats(WatcherEnums.CreatureTemplateType.Loach, 1f);
            self.EatenBy(WatcherEnums.CreatureTemplateType.Loach, .35f);
            self.Eats(WatcherEnums.CreatureTemplateType.Tardigrade, 1f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.Tardigrade, .3f);
            self.Eats(WatcherEnums.CreatureTemplateType.SmallMoth, 1f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.SmallMoth, .3f);
            self.Eats(WatcherEnums.CreatureTemplateType.Rat, 1f);
            self.FearedBy(WatcherEnums.CreatureTemplateType.Rat, .3f);
            self.EatenBy(WatcherEnums.CreatureTemplateType.BigSandGrub, .35f);
        }
    }

	public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new XyloWorm(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new VultureGrub.VultureGrubState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.VultureGrub;

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        if (unlock.Data == M4R_DATA_NUMBER)
            abstractCreature.superSizeMe = true;
        return abstractCreature;
    }
}