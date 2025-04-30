/*using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using UnityEngine;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class XyloWormCritob : Critob
{
    internal XyloWormCritob() : base(CreatureTemplateType.XyloWorm)
    {
        Icon = new SimpleIcon("Kill_Denture", new(1f, 1f, 0f));
        SandboxPerformanceCost = new(.35f, .1f);
        LoadedPerformanceCost = 10f;
    }

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [];

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(1f, 1f, 0f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "xyw";

    public override IEnumerable<string> WorldFileAliases() => ["xyloworm", "xylo worm"];

    public override CreatureTemplate CreateTemplate() => new CreatureFormula(CreatureTemplate.Type.VultureGrub, this)
    {
        DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
        DamageResistances = new() { Base = .3f },
        StunResistances = new() { Base = .3f },
        HasAI = false
    }.IntoTemplate();

    public override void EstablishRelationships()
	{
		var self = new Relationships(Type);
        self.Ignores(Type);
        self.IgnoredBy(CreatureTemplateType.Xylo);
        self.Ignores(CreatureTemplateType.Xylo);
    }

	public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new XyloWorm(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.VultureGrub;
}*/