/*using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using UnityEngine;
using DevInterface;
using RWCustom;

namespace LBMergedMods.Creatures;

sealed class XyloCritob : Critob
{
    internal XyloCritob() : base(CreatureTemplateType.Xylo)
    {
        Icon = new SimpleIcon("Kill_Denture", new(.2f, 0f, .45f));
        RegisterUnlock(KillScore.Configurable(4), SandboxUnlockID.Xylo);
        SandboxPerformanceCost = new(.4f, .4f);
        LoadedPerformanceCost = 10f;
        ShelterDanger = ShelterDanger.TooLarge;
    }

    public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow) => allow = map.getTerrainProximity(tilePos) > 3;

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [];

    public override int ExpeditionScore() => 4;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.2f, 0f, .45f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "xy";

    public override IEnumerable<string> WorldFileAliases() => ["xylo"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f),
            DamageResistances = new() { Base = float.MaxValue },
            StunResistances = new() { Base = float.MaxValue },
            HasAI = false
        }.IntoTemplate();
        t.requireAImap = true;
        t.doPreBakedPathing = false;
        t.stowFoodInDen = false;
        t.offScreenSpeed = 0f;
        t.bodySize = 4f;
        t.grasps = 0;
        t.visualRadius = 0f;
        t.movementBasedVision = 0f;
        t.waterVision = 0f;
        t.throughSurfaceVision = 0f;
        t.dangerousToPlayer = .1f;
        t.communityInfluence = .0f;
        t.wormGrassImmune = true;
        t.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        t.BlizzardWanderer = true;
		t.countsAsAKill = 0;
		t.wormgrassTilesIgnored = true;
		t.BlizzardAdapted = true;
		t.BlizzardWanderer = true;
		t.shortcutColor = default;
		t.shortcutSegments = 3;
		t.scaryness = 1f;
		t.deliciousness = 0f;
		t.meatPoints = 0;
		t.canSwim = false;
		t.canFly = false;
        t.daddyCorruptionImmune = true;
        return t;
    }

	public override void EstablishRelationships()
	{
		var self = new Relationships(Type);
        var tps = CreatureTemplate.Type.values.entries;
        for (var i = 0; i < tps.Count; i++)
            self.IgnoredBy(new(tps[i]));
    }

	public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Xylo(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new HazerMomState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Snail;
}*/