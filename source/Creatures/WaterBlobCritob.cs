using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class WaterBlobCritob : Critob
{
    internal WaterBlobCritob() : base(CreatureTemplateType.WaterBlob)
    {
        Icon = new SimpleIcon("Kill_WaterBlob", new(.8f, .8f, 1f));
        RegisterUnlock(KillScore.Configurable(2), SandboxUnlockID.WaterBlob);
        SandboxPerformanceCost = new(.2f, .3f);
        LoadedPerformanceCost = 20f;
    }

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(.8f, .8f, 1f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "blob";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesWater,
        RoomAttractivenessPanel.Category.Swimming
    ];

    public override IEnumerable<string> WorldFileAliases() => ["waterblob", "blob", "water blob"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(this)
        {
            TileResistances = new()
            {
                Floor = new(1f, Allowed),
                Corridor = new(1.5f, Allowed),
            },
            ConnectionResistances = new()
            {
                Standard = new(1f, Allowed),
                OpenDiagonal = new(10f, Allowed),
                ReachOverGap = new(5f, Allowed),
                ReachUp = new(10f, Allowed),
                ReachDown = new(10f, Allowed),
                SemiDiagonalReach = new(10f, Allowed),
                DropToFloor = new(2f, Allowed),
                DropToWater = new(2f, Allowed),
                ShortCut = new(1.5f, Allowed),
                NPCTransportation = new(3f, Allowed),
                Slope = new(1.5f, Allowed)
            },
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, .1f),
            DamageResistances = new() { Base = .15f },
            StunResistances = new() { Base = .3f },
            HasAI = true,
            InstantDeathDamage = .7f,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Snail)
        }.IntoTemplate();
        t.offScreenSpeed = .5f;
        t.abstractedLaziness = 50;
        t.bodySize = 1f;
        t.shortcutSegments = 1;
        t.grasps = 0;
        t.visualRadius = 800f;
        t.communityInfluence = .05f;
        t.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        t.canSwim = true;
        t.saveCreature = true;
        t.roamBetweenRoomsChance = .001f;
        t.jumpAction = "Jump";
        return t;
    }

    public override void EstablishRelationships()
    {
        var b = new Relationships(Type);
        b.Fears(CreatureTemplate.Type.Slugcat, 1f);
        b.Fears(CreatureTemplate.Type.Scavenger, .9f);
        b.Fears(CreatureTemplate.Type.LizardTemplate, .8f);
        b.Fears(CreatureTemplate.Type.JetFish, .9f);
        b.AttackedBy(CreatureTemplate.Type.LizardTemplate, .1f);
        b.EatenBy(CreatureTemplate.Type.JetFish, .6f);
        b.AttackedBy(CreatureTemplate.Type.Scavenger, .3f);
        b.Eats(CreatureTemplate.Type.Fly, 1f);
        b.FearedBy(CreatureTemplate.Type.Fly, 1f);
        b.IsInPack(Type, 1f);
    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new WaterBlobAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new WaterBlob(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new WaterBlob.WaterBlobState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Snail;
}