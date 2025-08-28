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

sealed class GlowpillarCritob : Critob, ISandboxHandler
{
    internal GlowpillarCritob() : base(CreatureTemplateType.Glowpillar)
    {
        Icon = new SimpleIcon("Kill_CommonCaterpillar", Color.yellow);
        RegisterUnlock(KillScore.Configurable(7), SandboxUnlockID.Glowpillar);
        SandboxPerformanceCost = new(.8f, .5f);
        ShelterDanger = ShelterDanger.Hostile;
    }

    public override int ExpeditionScore() => 7;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.yellow;

    public override string DevtoolsMapName(AbstractCreature acrit) => "gctp";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() =>
    [
        RoomAttractivenessPanel.Category.LikesInside,
        RoomAttractivenessPanel.Category.Dark
    ];

    public override IEnumerable<string> WorldFileAliases() => ["glowpillar", "glow pillar"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplateType.Killerpillar, this)
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
        t.doubleReachUpConnectionParams = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard).doubleReachUpConnectionParams;
        return t;
    }

    public override void EstablishRelationships()
    {
        var ctp = new Relationships(Type);
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
        if (Random.value < .5f)
            abstractCreature.superSizeMe = true;
        if (Random.value < .08f && AbsProps.TryGetValue(abstractCreature, out var props))
            props.Albino = true;
        Random.state = state;
        return abstractCreature;
    }
}