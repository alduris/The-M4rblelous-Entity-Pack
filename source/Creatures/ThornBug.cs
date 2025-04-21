using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using MoreSlugcats;

namespace LBMergedMods.Creatures;
//CHK
public class ThornBug : InsectoidCreature
{
    public static Color BugCol = Color.Lerp(Color.red, Color.yellow, .3f);
    public MovementConnection LastFollowedConnection;
    public IntVector2 SpecialMoveDestination;
    public Vector2 AntennaDir, TravelDir, AwayFromTerrainDir;
    [AllowNull] public ThornBugAI AI;
    public int FootingCounter, OutOfWaterFooting, SpecialMoveCounter, StunCounter;
    public float RunSpeed, RunCycle, Hue;
    public float NoJumps;
    public float AntennaAttention, Shake;
    public bool Sitting;

    public virtual new HealthState State => (abstractCreature.state as HealthState)!;

    public virtual bool Footing
    {
        get
        {
            if (FootingCounter <= 20)
                return OutOfWaterFooting > 0;
            return true;
        }
    }

    public ThornBug(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = [new(this, 0, default, 5f, .4f * (1f / 3f)), new(this, 1, default, 8f, .4f * (2f / 3f))];
        bodyChunkConnections = [new(bodyChunks[0], bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, .5f)];
        airFriction = .999f;
        gravity = .9f;
        bounce = .1f;
        surfaceFriction = .4f;
        collisionLayer = 1;
        waterFriction = .96f;
        buoyancy = .95f;
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        Hue = abstractCreature.superSizeMe ? Mathf.Lerp(.5f, .56f, Custom.ClampedRandomVariation(.5f, .5f, 2f)) : Mathf.Lerp(0f, .15f, Custom.ClampedRandomVariation(.5f, .5f, 2f));
        Random.state = state;
    }

    public override Color ShortCutColor() => Custom.HSL2RGB(Custom.Decimal(Hue + ThornBugGraphics.HUE_OFF), 1f, .5f);

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new ThornBugGraphics(this);
        graphicsModule.Reset();
    }

    public override void Stun(int st)
    {
        base.Stun(st);
        if (st > 4 && Random.value < .5f)
            ReleaseGrasp(0);
    }

    public override void Update(bool eu)
    {
        if (room is not Room rm || rm.game is not RainWorldGame g)
            return;
        var bs = bodyChunks;
        var b0 = bs[0];
        var state = State;
        if (g.devToolsActive && Input.GetKey("b") && g.cameras[0]?.room == rm)
        {
            b0.vel += Custom.DirVec(b0.pos, (Vector2)Futile.mousePosition + g.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (!dead && state.health < 0f && Random.value < 0f - state.health && Random.value < .025f)
            Die();
        if (!dead && Random.value * .7f > state.health && Random.value < .125f)
            Stun(Random.Range(1, Random.Range(1, 27 - Custom.IntClamp((int)(20f * state.health), 0, 10))));
        if (!dead && state.health > 0f && state.health < 1f && Random.value < .02f && poison < .1f)
            state.health = Mathf.Min(1f, state.health + 1f / Mathf.Lerp(140f, 50f, state.health));
        if (OutOfWaterFooting > 0)
            --OutOfWaterFooting;
        if (NoJumps > 0 && Footing)
            --NoJumps;
        if (!dead && stun > Random.Range(20, 80))
        {
            Shake = Math.Max(Shake, 10);
            for (var i = 0; i < bs.Length; i++)
            {
                var b = bs[i];
                if (b.ContactPoint.x != 0 || b.ContactPoint.y != 0)
                    b.vel += (Custom.RNV() - b.ContactPoint.ToVector2()) * Random.value * 3f;
            }
        }
        if (Shake > 0)
        {
            --Shake;
            if (!dead)
            {
                for (var j = 0; j < bs.Length; j++)
                {
                    var b = bs[j];
                    if (rm.aimap?.TileAccessibleToCreature(b.pos, Template) is true)
                        b.vel += Custom.RNV() * 2f;
                }
            }
        }
        base.Update(eu);
        if (room?.game is null)
            return;
        if (graphicsModule is ThornBugGraphics gr && Footing && rm.aimap is AImap map && !map.TileAccessibleToCreature(b0.pos, Template) && !map.TileAccessibleToCreature(bs[1].pos, Template))
        {
            for (var k = 0; k < 2; k++)
            {
                for (var l = 0; l < 2; l++)
                {
                    var leg = gr.Legs[k][l];
                    if (leg.reachedSnapPosition && Random.value < .5f && !Custom.DistLess(b0.pos, leg.absoluteHuntPos, gr.LegLength) && Custom.DistLess(b0.pos, leg.absoluteHuntPos, gr.LegLength + 15f))
                    {
                        var vector = Custom.DirVec(b0.pos, leg.absoluteHuntPos) * (Vector2.Distance(b0.pos, leg.absoluteHuntPos) - gr.LegLength);
                        b0.pos += vector;
                        b0.vel += vector;
                    }
                }
            }
        }
        Sitting = false;
        AntennaAttention = Mathf.Max(0f, AntennaAttention - 1f / 60f);
        if (grabbedBy?.Count > 0)
        {
            if (!dead)
            {
                for (var m = 0; m < bs.Length; m++)
                    bs[m].vel += Custom.RNV() * 2f;
                AI?.Update();
            }
            FootingCounter = 0;
            TravelDir *= 0f;
        }
        if (Consious)
        {
            ++FootingCounter;
            if (safariControlled && inputWithDiagonals.HasValue && inputWithDiagonals.Value.thrw)
                ReleaseGrasp(0);
            Act();
        }
        else
            FootingCounter = 0;
        if (grasps[0] is not null)
            CarryObject(eu);
        if (Footing)
        {
            for (var num3 = 0; num3 < bs.Length; num3++)
            {
                var b = bs[num3];
                b.vel *= .765f;
                b.vel.y += gravity;
            }
        }
        TravelDir *= Sitting ? .5f : .995f;
        if (!Consious || Footing || !(AI?.Behav == ThornBugAI.Behavior.Flee))
            return;
        for (var num4 = 0; num4 < bs.Length; num4++)
        {
            var b = bs[num4];
            if (rm.aimap?.TileAccessibleToCreature(Room.StaticGetTilePosition(b.pos), Template) is true)
                b.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 5f;
        }
    }

    public virtual void Swim()
    {
        if (room is not Room rm)
            return;
        var pathFinder = (AI.pathFinder as StandardPather)!;
        var bs = bodyChunks;
        var b0 = bs[0];
        b0.vel *= 1f - .05f * b0.submersion;
        bs[1].vel *= 1f - .1f * bs[1].submersion;
        GoThroughFloors = true;
        var movementConnection = pathFinder.FollowPath(rm.GetWorldCoordinate(b0.pos), true);
        if (movementConnection == default)
            movementConnection = pathFinder.FollowPath(rm.GetWorldCoordinate(bs[1].pos), true);
        var waterlvl = rm.DefaultWaterLevel(abstractCreature.pos.Tile);
        if (movementConnection == default && Math.Abs(abstractCreature.pos.y - waterlvl) < 4)
            movementConnection = pathFinder.FollowPath(abstractCreature.pos with { y = waterlvl }, true);
        if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)))
        {
            movementConnection = default;
            if (inputWithDiagonals.HasValue)
            {
                var type = MovementConnection.MovementType.Standard;
                if (rm.GetTile(b0.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                    type = MovementConnection.MovementType.ShortCut;
                if (inputWithDiagonals.Value.AnyDirectionalInput)
                    movementConnection = new MovementConnection(type, rm.GetWorldCoordinate(b0.pos), rm.GetWorldCoordinate(b0.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
            }
        }
        if (movementConnection != default)
        {
            if (graphicsModule is ThornBugGraphics gr)
            {
                gr.Flip = Mathf.Lerp(gr.Flip, Mathf.Sign(rm.MiddleOfTile(movementConnection.StartTile).x - rm.MiddleOfTile(movementConnection.DestTile).x), .25f);
                for (var i = 0; i < 2; i++)
                {
                    for (var j = 0; j < 2; j++)
                        gr.Legs[i][j].vel += Custom.DirVec(bs[1].pos, b0.pos) * Mathf.Lerp(-10f, 10f, Random.value);
                }
            }
            if (movementConnection.StartTile.y == movementConnection.DestTile.y && movementConnection.DestTile.y == rm.DefaultWaterLevel(movementConnection.DestTile))
            {
                var sgn = Mathf.Sign(rm.MiddleOfTile(movementConnection.StartTile).x - rm.MiddleOfTile(movementConnection.DestTile).x);
                b0.vel.x -= sgn * 1.6f * b0.submersion;
                bs[1].vel.x += sgn * .5f * bs[1].submersion;
                FootingCounter = 0;
                return;
            }
            b0.vel *= .9f;
            b0.vel += Custom.DirVec(b0.pos, rm.MiddleOfTile(movementConnection.destinationCoord)) * 1.4f;
            if (!safariControlled || Submersion < .5f)
            {
                FootingCounter = Math.Max(FootingCounter, 25);
                Run(movementConnection);
                OutOfWaterFooting = 20;
            }
            else
            {
                b0.vel *= .765f;
                FootingCounter = 0;
                Run(movementConnection);
                OutOfWaterFooting = 0;
            }
        }
        else
            b0.vel.y += .5f;
    }

    public virtual void Act()
    {
        if (Submersion > .3f)
        {
            Swim();
            AI?.Update();
            return;
        }
        if (AI is null || room is not Room rm)
            return;
        var bs = bodyChunks;
        var b0 = bs[0];
        if (SpecialMoveCounter > 0)
        {
            SpecialMoveCounter--;
            MoveTowards(rm.MiddleOfTile(SpecialMoveDestination));
            TravelDir = Vector2.Lerp(TravelDir, Custom.DirVec(b0.pos, rm.MiddleOfTile(SpecialMoveDestination)), .4f);
            if (Custom.DistLess(b0.pos, rm.MiddleOfTile(SpecialMoveDestination), 5f))
                SpecialMoveCounter = 0;
        }
        else
        {
            if (rm.aimap is AImap map && !map.TileAccessibleToCreature(b0.pos, Template) && !map.TileAccessibleToCreature(bs[1].pos, Template))
                FootingCounter = Custom.IntClamp(FootingCounter - 3, 0, 35);
            if (!safariControlled && (rm.GetWorldCoordinate(b0.pos) == AI.pathFinder.GetDestination || rm.GetWorldCoordinate(bs[1].pos) == AI.pathFinder.GetDestination) && AI.threatTracker.Utility() < .5f)
            {
                Sitting = true;
                GoThroughFloors = false;
            }
            else
            {
                var movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(rm.GetWorldCoordinate(b0.pos), true);
                if (movementConnection == default)
                    movementConnection = (AI.pathFinder as StandardPather)!.FollowPath(rm.GetWorldCoordinate(bs[1].pos), true);
                if (safariControlled && (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type)))
                {
                    movementConnection = default;
                    if (inputWithDiagonals.HasValue)
                    {
                        var type = MovementConnection.MovementType.Standard;
                        if (rm.GetTile(b0.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                            type = MovementConnection.MovementType.ShortCut;
                        if (inputWithDiagonals.Value.AnyDirectionalInput)
                            movementConnection = new(type, rm.GetWorldCoordinate(b0.pos), rm.GetWorldCoordinate(b0.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
                        if (inputWithDiagonals.Value.y < 0)
                            GoThroughFloors = true;
                        else
                            GoThroughFloors = false;
                        if (inputWithDiagonals.Value.jmp && lastInputWithDiagonals?.jmp is null or false)
                        {
                            var vector = TravelDir * 40f;
                            if (inputWithDiagonals.Value.AnyDirectionalInput)
                                vector = new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f;
                            vector.y = 0f;
                            TryJump(b0.pos + vector, false);
                        }
                    }
                }
                if (movementConnection != default)
                {
                    Run(movementConnection);
                    TravelDir = Vector2.Lerp(TravelDir, Custom.DirVec(b0.pos, rm.MiddleOfTile(movementConnection.destinationCoord)), .4f);
                }
                else
                    GoThroughFloors = false;
            }
        }
        AI.Update();
        var num = RunCycle;
        if (Consious && !Custom.DistLess(b0.pos, b0.lastPos, 5f))
            RunCycle += RunSpeed * .09f;
        if (num < Mathf.Floor(RunCycle))
            rm.PlaySound(NewSoundID.M4R_GenericBug_Chip, b0, false, .9f, 1f);
        if (Sitting)
        {
            Vector2 vector2 = default;
            for (var i = 0; i < 8; i++)
            {
                var dr = Custom.eightDirections[i];
                if (rm.GetTile(abstractCreature.pos.Tile + dr).Solid)
                    vector2 -= dr.ToVector2();
            }
            AwayFromTerrainDir = Vector2.Lerp(AwayFromTerrainDir, vector2.normalized, .1f);
        }
        else
            AwayFromTerrainDir *= .7f;
    }

    public virtual void TryJump(Vector2 targetPoint, bool away)
    {
        var bs = bodyChunks;
        var b0 = bs[0];
        if (Consious && NoJumps <= 0 && room?.aimap is AImap map && (map.TileAccessibleToCreature(b0.pos, Template) || map.TileAccessibleToCreature(bs[1].pos, Template)) && !map.getAItile(bs[1].pos).narrowSpace)
        {
            room.PlaySound(NewSoundID.M4R_GenericBug_BigChip, b0, false, .9f, 1.2f + Random.value * .1f);
            var vector = Custom.DirVec(targetPoint, (b0.pos + bs[1].pos) / 2f);
            if (!away)
            {
                vector.x *= -1f;
                vector.y *= -.4f;
            }
            vector += Custom.RNV() * .3f;
            vector.Normalize();
            vector = Vector3.Slerp(vector, new(0f, 1f), Custom.LerpMap(vector.y, -.5f, .5f, .7f, .3f));
            b0.vel *= .5f;
            bs[1].vel *= .5f;
            b0.vel += vector * 15f + Custom.RNV() * 5f * Random.value;
            bs[1].vel += vector * 15f + Custom.RNV() * 5f * Random.value;
            FootingCounter = 0;
            var vector2 = Custom.PerpendicularVector(vector) * (Random.value < .5f ? -1f : 1f);
            b0.vel += vector2 * 11f;
            bs[1].vel -= vector2 * 11f;
            if (!safariControlled)
                NoJumps = 90;
            else
                NoJumps = 10;
        }
    }

    public virtual void Run(MovementConnection followingConnection)
    {
        if (followingConnection.type == MovementConnection.MovementType.ReachUp)
            (AI.pathFinder as StandardPather)!.pastConnections.Clear();
        if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
        {
            enteringShortCut = followingConnection.StartTile;
            if (abstractCreature.controlled)
            {
                var flag = false;
                List<IntVector2> list = [];
                var shortcuts = room.shortcuts;
                for (int i = 0; i < shortcuts.Length; i++)
                {
                    ref readonly var shortcutData = ref shortcuts[i];
                    if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
                        list.Add(shortcutData.StartTile);
                    if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
                        flag = true;
                }
                if (flag)
                {
                    if (list.Count > 0)
                    {
                        list.Shuffle();
                        NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
                    }
                    else
                        NPCTransportationDestination = followingConnection.destinationCoord;
                }
            }
            else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
                NPCTransportationDestination = followingConnection.destinationCoord;
        }
        else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
        {
            SpecialMoveCounter = 30;
            SpecialMoveDestination = followingConnection.DestTile;
        }
        else
        {
            var bs = bodyChunks;
            var vector = room.MiddleOfTile(followingConnection.DestTile);
            if (LastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
                bs[0].vel += Custom.DirVec(bs[0].pos, vector) * 4f;
            if (Footing)
            {
                for (var j = 0; j < bs.Length; j++)
                {
                    var b = bs[j];
                    if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
                        b.vel.x += Mathf.Min((vector.x - b.pos.x) / 8f, 1.2f);
                    else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
                        b.vel.y += Mathf.Min((vector.y - b.pos.y) / 8f, 1.2f);
                }
            }
            if ((Footing || room.aimap.TileAccessibleToCreature(bs[0].pos, Template)) && (followingConnection.startCoord.x != followingConnection.destinationCoord.x && LastFollowedConnection.startCoord.x == LastFollowedConnection.destinationCoord.x || followingConnection.startCoord.y != followingConnection.destinationCoord.y && LastFollowedConnection.startCoord.y == LastFollowedConnection.destinationCoord.y))
            {
                bs[0].vel *= .7f;
                bs[1].vel *= .5f;
            }
            if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
                FootingCounter = 0;
            MoveTowards(vector);
        }
        LastFollowedConnection = followingConnection;
    }

    public virtual void MoveTowards(Vector2 moveTo)
    {
        var bs = bodyChunks;
        var b0 = bs[0];
        var vector = Custom.DirVec(b0.pos, moveTo);
        if (!Footing)
            vector *= .3f;
        if (IsTileSolid(1, 0, -1) && (vector.x < -.5f && b0.pos.x > bs[1].pos.x + 5f || vector.x > .5f && b0.pos.x < bs[1].pos.x - 5f))
        {
            b0.vel.x -= (vector.x < 0f ? -1f : 1f) * 1.3f;
            bs[1].vel.x += (vector.x < 0f ? -1f : 1f) * .5f;
            if (!IsTileSolid(0, 0, 1))
                b0.vel.y += 3.2f;
        }
        var num = .6f;
        if (graphicsModule is ThornBugGraphics gr)
        {
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    if (gr.Legs[i][j].OverLappingHuntPos)
                        num += .1f;
                }
            }
        }
        else
            num = .85f;
        num = Mathf.Pow(num, .6f);
        if (safariControlled && Footing)
            vector *= 1.5f;
        b0.vel += 6.2f * .84f * vector * RunSpeed * num;
        bs[1].vel -= vector * .84f * RunSpeed * num;
        GoThroughFloors = moveTo.y < b0.pos.y - 5f;
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);
        if (safariControlled && Consious && otherObject is Creature c && AI?.DynamicRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp && grasps[0] is null)
            Grab(c, 0, otherChunk, Grasp.Shareability.CanNotShare, 1f, true, true);
        else if (otherObject is ThornBug th)
        {
            AI?.CollideWithKin(th);
            if (bodyChunks[myChunk].pos.y > otherObject.bodyChunks[otherChunk].pos.y)
            {
                bodyChunks[myChunk].vel.y += 2f;
                otherObject.bodyChunks[otherChunk].vel.y -= 2f;
            }
        }
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        var bs = bodyChunks;
        for (var i = 0; i < bs.Length; i++)
        {
            var b = bs[i];
            b.pos = newRoom.MiddleOfTile(pos) - vector * (i - 1.5f) * 15f;
            b.lastPos = newRoom.MiddleOfTile(pos);
            b.vel = vector * 2f;
        }
        if (graphicsModule is ThornBugGraphics gr)
            gr.Reset();
    }

    public virtual void Suprise(Vector2 surprisePos)
    {
        if (!Consious)
            return;
        var bs = bodyChunks;
        if (Custom.DistLess(surprisePos, bs[0].pos, 300f))
        {
            for (var i = 0; i < bs.Length; i++)
            {
                var b = bs[i];
                if (room.aimap.TileAccessibleToCreature(b.pos, Template))
                    b.vel += (Custom.RNV() * 4f + Custom.DirVec(surprisePos, b.pos) * 2f) * (.5f + .5f * AI.Fear);
            }
        }
        Shake = Math.Max(Shake, Random.Range(5, 15));
        AI.Fear = Custom.LerpAndTick(AI.Fear, 1f, .3f, 1f / 7f);
    }

    public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying)
    {
        var res = base.Grab(obj, graspUsed, chunkGrabbed, shareability, dominance, overrideEquallyDominant, pacifying);
        if (res && obj is Creature cr && room is Room rm)
        {
            var b0 = bodyChunks[0];
            var crCh = cr.bodyChunks[chunkGrabbed];
            cr.Violence(b0, Custom.DirVec(b0.pos, crCh.pos) * 10f, crCh, null, DamageType.Stab, .5f, 0f);
            rm.AddObject(new CreatureSpasmer(cr, false, 54));
            rm.AddObject(new CreatureSpasmer(this, true, 164));
            b0.vel += Custom.RNV() * 5f;
            rm.PlaySound(SoundID.Spear_Stick_In_Creature, b0);
            if (rm.BeingViewed)
            {
                for (var j = 0; j < 8; j++)
                    rm.AddObject(new WaterDrip(crCh.pos, -b0.vel * Random.value * .5f + Custom.DegToVec(360f * Random.value) * b0.vel.magnitude * Random.value * .5f, false));
            }
        }
        return res;
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (type == DamageType.Explosion)
            damage *= 5f;
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    public override void ReleaseGrasp(int grasp)
    {
        base.ReleaseGrasp(grasp);
        StunCounter = 0;
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public virtual void CarryObject(bool eu)
    {
        var g = grasps[0];
        if (g.grabbed.room is not Room rm || room is not Room rmm || rm.abstractRoom.index != rmm.abstractRoom.index || Random.value < .025f && (g.grabbed is not Creature c || AI.DynamicRelationship(c.abstractCreature).type != CreatureTemplate.Relationship.Type.Eats))
        {
            ReleaseGrasp(0);
            return;
        }
        var bs = bodyChunks;
        var b0 = bs[0];
        var vector = b0.pos + Custom.DirVec(bs[1].pos, b0.pos) * 1f;
        var grabbed = g.grabbed;
        var vector2 = grabbed.bodyChunks[g.chunkGrabbed].vel - b0.vel;
        grabbed.bodyChunks[g.chunkGrabbed].vel = b0.vel;
        if (!enteringShortCut.HasValue && (vector2.magnitude * grabbed.bodyChunks[g.chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[g.chunkGrabbed].pos, 70f + grabbed.bodyChunks[g.chunkGrabbed].rad)))
            ReleaseGrasp(0);
        else
            grabbed.bodyChunks[g.chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector);
        if (g is not null)
        {
            for (var i = 0; i < 2; i++)
                g.grabbed.PushOutOf(bs[i].pos, bs[i].rad, g.chunkGrabbed);
            if (g.grabbed is Creature cr && !cr.dead)
            {
                if (StunCounter < 60)
                    ++StunCounter;
                else if (StunCounter == 60)
                {
                    if (cr is Player p)
                    {
                        if (!ModManager.MSC || (p.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear && p.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
                            p.Regurgitate();
                    }
                    cr.Stun(10);
                    cr.LoseAllGrasps();
                    b0.vel += Custom.RNV();
                    rm.PlaySound(SoundID.Spear_Stick_In_Creature, b0, false, .5f, 1f);
                }
                ++StunCounter;
            }
            else
                StunCounter = 0;
        }
    }
}