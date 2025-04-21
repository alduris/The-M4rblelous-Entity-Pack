using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using System.Collections.Generic;
using UnityEngine;
using static PathCost.Legality;
using static MovementConnection.MovementType;
using Fisobs.Sandbox;

namespace LBMergedMods.Creatures;
// CHK
sealed class BouncingBallCritob : Critob
{
    internal BouncingBallCritob() : base(CreatureTemplateType.BouncingBall)
    {
        Icon = new SimpleIcon("Kill_BouncingBall", Color.white);
        LoadedPerformanceCost = 20f;
        SandboxPerformanceCost = new(.5f, .6f);
        RegisterUnlock(KillScore.Configurable(2), SandboxUnlockID.BouncingBall);
    }

    public override int ExpeditionScore() => 2;

    public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.Snail;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.white;

    public override string DevtoolsMapName(AbstractCreature acrit) => "BoB";

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => [RoomAttractivenessPanel.Category.LikesWater];

    public override CreatureTemplate CreateTemplate() => new CreatureFormula(CreatureTemplate.Type.Snail, Type, "Bouncing Ball")
    {
        TileResistances = new()
        {
            Floor = new(1f, Allowed),
            Corridor = new(1f, Allowed),
            Climb = new(1f, Allowed),
            Wall = new(1f, Allowed),
            Ceiling = new(1f, Allowed),
            OffScreen = new(1f, Allowed)
        },
        ConnectionResistances = new()
        {
            Standard = new(1f, Allowed),
            OpenDiagonal = new(2f, Allowed),
            ShortCut = new(.2f, Allowed),
            NPCTransportation = new(20f, Allowed),
            Slope = new(1.6f, Allowed),
            CeilingSlope = new(1.6f, Allowed),
            DropToFloor = new(10f, Allowed),
            OffScreenMovement = new(1f, Allowed),
            BetweenRooms = new(10f, Allowed)
        },
        Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Snail),
        HasAI = true,
        DamageResistances = new() { Base = .4f, Explosion = 102f },
        StunResistances = new() { Base = .8f, Explosion = 102f }
    }.IntoTemplate();

    public override void EstablishRelationships()
    {
        var b = new Relationships(Type);
        b.EatenBy(CreatureTemplate.Type.Leech, .5f);
        b.IgnoredBy(CreatureTemplate.Type.Snail);
        b.MakesUncomfortable(CreatureTemplate.Type.Scavenger, .6f);
        b.Ignores(Type);
        b.Ignores(CreatureTemplate.Type.Snail);
        b.Fears(CreatureTemplate.Type.BrotherLongLegs, .8f);
        b.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
    }

    public override IEnumerable<string> WorldFileAliases() => ["bouncingball", "bouncing ball", "bob"];

    public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow) => allow = !(connection.type == DropToFloor && !map.room.GetTile(connection.DestTile).DeepWater);

    public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new BouncingBallAI(acrit, acrit.world);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new BouncingBall(acrit, acrit.world);

    public override void LoadResources(RainWorld rainWorld) { }
}