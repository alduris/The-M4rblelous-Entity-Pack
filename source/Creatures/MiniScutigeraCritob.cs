using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using RWCustom;
using UnityEngine;
using DevInterface;

namespace LBMergedMods.Creatures;

sealed class MiniScutigeraCritob : Critob
{
    internal MiniScutigeraCritob() : base(CreatureTemplateType.MiniScutigera)
    {
        Icon = new SimpleIcon("Kill_MiniScutigera", Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f));
        RegisterUnlock(KillScore.Constant(0), SandboxUnlockID.MiniScutigera);
        SandboxPerformanceCost = new(.3f, .3f);
    }

    public override int ExpeditionScore() => 2;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "msct";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesInside];

    public override IEnumerable<string> WorldFileAliases() => ["miniscut", "miniscutigera", "miniscuti", "mini scut", "mini scutigera", "mini scuti"];

    public override CreatureTemplate CreateTemplate()
    {
        var t = new CreatureFormula(CreatureTemplate.Type.SmallCentipede, Type, "MiniScutigera")
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
            DamageResistances = new() { Base = 1f, Electric = 102f },
            StunResistances = new() { Base = .6f, Electric = 102f },
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.BlueLizard)
        }.IntoTemplate();
        t.doubleReachUpConnectionParams = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard).doubleReachUpConnectionParams;
        t.shortcutSegments = 2;
        return t;
    }

    public override void EstablishRelationships()
    {
        var ce = new Relationships(Type);
        ce.IgnoredBy(CreatureTemplate.Type.Centipede);
        ce.Ignores(CreatureTemplate.Type.Centipede);
        ce.Ignores(CreatureTemplate.Type.Deer);
        ce.Ignores(CreatureTemplate.Type.GarbageWorm);
        ce.Ignores(CreatureTemplate.Type.PoleMimic);
        ce.Ignores(Type);
        ce.EatenBy(CreatureTemplateType.ThornBug, .15f);
        ce.Fears(CreatureTemplateType.ThornBug, .15f);
        ce.IgnoredBy(CreatureTemplateType.Scutigera);
        ce.Ignores(CreatureTemplateType.Scutigera);

    }

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new CentipedeAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new MiniScutigera(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new HealthState(acrit);

    public override void LoadResources(RainWorld rainWorld) { }

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.SmallCentipede;
}