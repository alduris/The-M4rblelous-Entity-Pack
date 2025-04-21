using System.Collections.Generic;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using UnityEngine;
using MoreSlugcats;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;
// CHK
sealed class HazerMomCritob : Critob, ISandboxHandler
{
	internal HazerMomCritob() : base(CreatureTemplateType.HazerMom)
	{
		Icon = new SimpleIcon("Kill_HazerMom", Color.white);
		LoadedPerformanceCost = 10f;
		SandboxPerformanceCost = new(.4f, .4f);
		RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.HazerMom, null, M4R_DATA_NUMBER);
	}

    public override void CorpseIsEdible(Player player, Creature crit, ref bool canEatMeat)
    {
        if (ModManager.MSC && (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear))
            canEatMeat = false;
        else
            canEatMeat = crit.dead;
    }

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new HazerMom(acrit, acrit.world);

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => null;

    public override CreatureTemplate CreateTemplate()
	{
        var t = new CreatureFormula(CreatureTemplate.Type.Hazer, this)
        {
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.GreenLizard),
            DamageResistances = new() { Base = 2.1f, Water = 100f },
            StunResistances = new() { Base = 4f, Water = 200f },
            InstantDeathDamage = 3f,
            HasAI = false,
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0f)
        }.IntoTemplate();
        t.offScreenSpeed = .5f;
        t.bodySize = 1f;
        t.countsAsAKill = 2;
        t.jumpAction = "N/A";
		t.wormGrassImmune = true;
		t.wormgrassTilesIgnored = true;
		t.waterVision = 1f;
		t.throughSurfaceVision = .1f;
		t.visualRadius = 100f;
		t.waterPathingResistance = .1f;
		t.BlizzardWanderer = true;
		t.canSwim = true;
		t.deliciousness = .5f;
		t.abstractedLaziness = 100;
		t.BlizzardAdapted = true;
		t.lungCapacity = float.PositiveInfinity;
		t.shortcutColor = Color.white;
		t.meatPoints = 3;
        return t;
    }

    public override string DevtoolsMapName(AbstractCreature acrit) => "hmum";

	public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;

	public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.Swimming, RoomAttractivenessPanel.Category.LikesWater];

	public override IEnumerable<string> WorldFileAliases() => ["hazer mom", "hazermom"];

	public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Hazer;

	public override int ExpeditionScore() => 3;

	public override CreatureState CreateState(AbstractCreature acrit) => new HazerMomState(acrit);

	public override void EstablishRelationships() { }

    public override void LoadResources(RainWorld rainWorld) { }

    AbstractWorldEntity ISandboxHandler.ParseFromSandbox(World world, EntitySaveData data, SandboxUnlock unlock)
    {
        var text = data.CustomData + "SandboxData<cC>" + unlock.Data + "<cB>";
        var abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(data.Type.CritType), null, data.Pos, data.ID) { pos = data.Pos };
        abstractCreature.state.LoadFromString(text.Split(["<cB>"], StringSplitOptions.RemoveEmptyEntries));
        abstractCreature.setCustomFlags();
        var state = Random.state;
        Random.InitState(data.ID.RandomSeed);
        abstractCreature.superSizeMe = Random.value >= .08f;
        Random.state = state;
        return abstractCreature;
    }
}