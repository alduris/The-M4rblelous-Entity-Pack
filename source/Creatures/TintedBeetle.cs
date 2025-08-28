using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Smoke;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class TintedBeetle : InsectoidCreature
{
    public static Color BugCol = Color.Lerp(Color.red, Color.yellow, .1f);
    public MovementConnection LastFollowedConnection;
    public IntVector2 SpecialMoveDestination;
    public Vector2 AntennaDir, TravelDir, AwayFromTerrainDir;
    [AllowNull] public TintedBeetleAI AI;
    public int FootingCounter, OutOfWaterFooting, SpecialMoveCounter, GasLeakTime;
    public Spear? GasLeakSpear;
    public ChunkSoundEmitter? GasLeakSound;
    public float RunSpeed, RunCycle, Hue, AntennaAttention, Shake, GasLeakPower = 1f;
    public bool Sitting, HasGas = true;
    public FireSmoke? Smoke;

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

    public TintedBeetle(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = [new(this, 0, default, 5f, .4f * (1f / 3f)), new(this, 1, default, 8.5f, .4f * (2f / 3f))];
        bodyChunkConnections = [new(bodyChunks[0], bodyChunks[1], 14f, BodyChunkConnection.Type.Normal, 1f, .5f)];
        airFriction = .999f;
        gravity = .9f;
        bounce = .1f;
        surfaceFriction = .4f;
        collisionLayer = 1;
        waterFriction = .96f;
        buoyancy = .95f;
        abstractCreature.lavaImmune = true;
        var state = Random.state;
        Random.InitState(abstractCreature.ID.RandomSeed);
        Hue = abstractCreature.superSizeMe ? Mathf.Lerp(188f / 360f, 200f / 360f, Custom.ClampedRandomVariation(.5f, .5f, 2f)) : Mathf.Lerp(-.04f, .04f, Custom.ClampedRandomVariation(.5f, .5f, 2f));
        Random.state = state;
    }

    public override Color ShortCutColor() => Custom.HSL2RGB(Custom.Decimal(Hue), 1f, .5f);

    public override void InitiateGraphicsModule()
    {
        graphicsModule ??= new TintedBeetleGraphics(this);
        graphicsModule.Reset();
    }

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
    {
        if (source is Spear s && HasGas)
        {
            GasLeakTime = Random.Range(300, 401);
            GasLeakSpear = s;
            HasGas = false;
        }
        return base.SpearStick(source, dmg, chunk, appPos, direction);
    }

    public override void Stun(int st)
    {
        base.Stun(st);
        if (st > 4 && Random.value < .5f)
            ReleaseGrasp(0);
    }

    public override void LoseAllGrasps() => ReleaseGrasp(0);

    public override void Update(bool eu)
    {
        if (room is not Room rm || rm.game is not RainWorldGame g)
            return;
        var bs = bodyChunks;
        var b0 = bs[0];
        var phys = rm.physicalObjects;
        for (var i = 0; i < phys.Length; i++)
        {
            var physObjs = phys[i];
            for (var j = 0; j < physObjs.Count; j++)
            {
                if (physObjs[j] is FirecrackerPlant pl && pl.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject))
                {
                    if (Custom.DistLess(pl.firstChunk.pos, b0.pos, 15f))
                    {
                        if (!pl.AbstrConsumable.isConsumed)
                            pl.AbstrConsumable.Consume();
                        if (pl.growPos.HasValue)
                            pl.growPos = null;
                        if (safariControlled && Consious && pl.grabbedBy?.Count is 0 && inputWithDiagonals.HasValue && inputWithDiagonals.Value.pckp && grasps[0] is null)
                            Grab(pl, 0, 0, Grasp.Shareability.CanNotShare, 1f, true, true);
                    }
                }
            }
        }
        var state = State;
        if (g.devToolsActive && Input.GetKey("b") && g.cameras[0]?.room == rm)
        {
            b0.vel += Custom.DirVec(b0.pos, (Vector2)Futile.mousePosition + g.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (GasLeakTime > 0)
        {
            --GasLeakTime;
            if (GasLeakSpear is not null)
            {
                if (GasLeakSound is null)
                {
                    GasLeakSound = room.PlaySound(SoundID.Cyan_Lizard_Gas_Leak_LOOP, GasLeakSpear.firstChunk, true, 1f, .8f);
                    GasLeakSound.requireActiveUpkeep = true;
                }
                else
                {
                    GasLeakSound.alive = true;
                    GasLeakSound.volume = 1f;
                    GasLeakSound.pitch = .5f + .75f * Mathf.Clamp01(GasLeakPower);
                }
                if (GasLeakSpear.stuckInObject is null || GasLeakSpear.stuckInChunk.owner != this || GasLeakSpear.mode != Weapon.Mode.StuckInCreature || GasLeakPower <= 0f)
                {
                    if (GasLeakPower > .1f)
                    {
                        GasLeakSpear.ChangeMode(Weapon.Mode.Free);
                        GasLeakSpear.firstChunk.vel += -GasLeakSpear.rotation * Mathf.Lerp(21f, 31f, Random.value);
                        Smoke?.EmitSmoke(GasLeakSpear.firstChunk.pos, GasLeakSpear.rotation * -20f * Random.value + Custom.RNV() * Random.value * 14f, Custom.HSL2RGB(Custom.Decimal(Hue), 1f, .5f), 10);
                    }
                    GasLeakSpear = null;
                }
                else
                {
                    GasLeakPower -= 1f / GasLeakTime;
                    float num = Mathf.Pow(Mathf.Clamp01(GasLeakPower), .5f) * Mathf.Pow(Random.value, .5f);
                    if (Random.value < num * Mathf.InverseLerp(1f, .9f, GasLeakPower) / 15f)
                    {
                        var vector = bs[Random.Range(0, 2)].pos + Custom.DegToVec(Mathf.Pow(Random.value, .25f) * 180f * (Random.value < .5f ? -1f : 1f)) * 16f * Random.value;
                        rm.PlaySound(SoundID.Cyan_Lizard_Small_Jump, mainBodyChunk, false, 1f, .8f);
                        rm.AddObject(new ShockWave(vector, 50f, .07f, 7));
                        if (Smoke is not null)
                        {
                            var col = Custom.HSL2RGB(Custom.Decimal(Hue), 1f, .5f);
                            for (var i = 0; i < 4; i++)
                                Smoke.EmitSmoke(GasLeakSpear.firstChunk.pos, GasLeakSpear.rotation * Mathf.Lerp(-7f, -20f, num) + Custom.RNV() * Random.value * 14f, col, 10);
                        }
                        for (int j = 0; j < bs.Length; j++)
                        {
                            var bc = bs[j];
                            if (!room.GetTile(bc.pos + Custom.DirVec(vector, bc.pos) * 25f).Solid)
                                bc.vel += Custom.DirVec(vector, bc.pos) * Mathf.Lerp(19f, 25f, num);
                        }
                        Stun(25);
                    }
                    GasLeakSpear.stuckInChunk.vel += 9f * GasLeakSpear.rotation * num;
                    if (Smoke is null)
                        room.AddObject(Smoke = new(room));
                    Smoke.EmitSmoke(GasLeakSpear.firstChunk.pos, GasLeakSpear.rotation * Mathf.Lerp(-20f, -60f, num) + Custom.RNV() * Random.value * 4f, Custom.HSL2RGB(Custom.Decimal(Hue), 1f, .5f), 10);
                }
            }
            else if (GasLeakSound is not null)
            {
                GasLeakSound.alive = false;
                GasLeakSound = null;
            }
            if (GasLeakTime < 100)
            {
                Explode(rm);
                return;
            }
        }
        if (!dead && state.health < 0f && Random.value < 0f - state.health && Random.value < .025f)
            Die();
        if (!dead && Random.value * .7f > state.health && Random.value < .125f)
            Stun(Random.Range(1, Random.Range(1, 27 - Custom.IntClamp((int)(20f * state.health), 0, 10))));
        if (!dead && state.health > 0f && state.health < 1f && Random.value < .02f && poison < .1f)
            state.health = Mathf.Min(1f, state.health + 1f / Mathf.Lerp(140f, 50f, state.health));
        if (OutOfWaterFooting > 0)
            --OutOfWaterFooting;
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
        if (Smoke is not null && (Smoke.slatedForDeletetion || Smoke.room != rm))
            Smoke = null;
        base.Update(eu);
        if (room?.game is null)
            return;
        if (Smoke is not null && (Smoke.slatedForDeletetion || Smoke.room != rm))
            Smoke = null;
        if (graphicsModule is TintedBeetleGraphics gr && Footing && rm.aimap is AImap map && !map.TileAccessibleToCreature(b0.pos, Template) && !map.TileAccessibleToCreature(bs[1].pos, Template))
        {
            for (var k = 0; k < 2; k++)
            {
                for (var l = 0; l < 2; l++)
                {
                    var leg = gr.Legs[k][l];
                    if (leg.reachedSnapPosition && Random.value < .5f && !Custom.DistLess(b0.pos, leg.absoluteHuntPos, TintedBeetleGraphics.LEG_LENGTH) && Custom.DistLess(b0.pos, leg.absoluteHuntPos, TintedBeetleGraphics.LEG_LENGTH + 15f))
                    {
                        var vector = Custom.DirVec(b0.pos, leg.absoluteHuntPos) * (Vector2.Distance(b0.pos, leg.absoluteHuntPos) - TintedBeetleGraphics.LEG_LENGTH);
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
                b.vel *= .85f;
                b.vel.y += gravity;
            }
        }
        TravelDir *= Sitting ? .5f : .995f;
        if (!Consious || Footing || !(AI?.Behav == TintedBeetleAI.Behavior.Flee))
            return;
        for (var num4 = 0; num4 < bs.Length; num4++)
        {
            var b = bs[num4];
            if (rm.aimap?.TileAccessibleToCreature(Room.StaticGetTilePosition(b.pos), Template) is true)
                b.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 5f;
        }
    }

    public virtual void Explode(Room rm)
    {
        var vector = mainBodyChunk.pos;
        var clr = Custom.HSL2RGB(Custom.Decimal(Hue), 1f, .5f);
        rm.AddObject(new SootMark(rm, vector, 50f, true));
        rm.AddObject(new Explosion(rm, this, vector, 5, 110f, 5f, 1.1f, 60f, .3f, this, .8f, 0f, .7f));
        for (var i = 0; i < 14; i++)
            rm.AddObject(new Explosion.ExplosionSmoke(vector, Custom.RNV() * 5f * Random.value, 1f));
        rm.AddObject(new Explosion.ExplosionLight(vector, 160f, 1f, 3, clr));
        rm.AddObject(new ExplosionSpikes(rm, vector, 9, 4f, 5f, 5f, 90f, clr));
        rm.AddObject(new ShockWave(vector, 60f, .045f, 4));
        for (var j = 0; j < 20; j++)
        {
            var vector2 = Custom.RNV();
            rm.AddObject(new Spark(vector + vector2 * Random.value * 40f, vector2 * Mathf.Lerp(4f, 30f, Random.value), clr, null, 4, 18));
        }
        rm.ScreenMovement(vector, default, .7f);
        rm.PlaySound(SoundID.Bomb_Explode, mainBodyChunk);
        Die();
        Smoke = null;
        Destroy();
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
            if (graphicsModule is TintedBeetleGraphics gr)
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
                b0.vel *= .75f;
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
            rm.PlaySound(NewSoundID.M4R_TintedBeetle_Chip, b0, false, 1f, .95f);
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
        if (graphicsModule is TintedBeetleGraphics gr)
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
        if (otherObject is TintedBeetle th)
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
        if (graphicsModule is TintedBeetleGraphics gr)
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
        room.PlaySound(NewSoundID.M4R_TintedBeetle_BigChip, firstChunk, false, 1f, .95f);
        return res;
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        if (RippleViolenceCheck(source) && type == DamageType.Explosion)
        {
            if (damage > .2f && room is Room rm)
            {
                Explode(rm);
                return;
            }
        }
        base.Violence(source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
    }

    public virtual void CarryObject(bool eu)
    {
        var g = grasps[0];
        if (g.grabbed is not FirecrackerPlant grabbed || grabbed.room is not Room rm || room is not Room rmm || rm.abstractRoom.index != rmm.abstractRoom.index)
        {
            ReleaseGrasp(0);
            return;
        }
        var bs = bodyChunks;
        var b0 = bs[0];
        var vector = b0.pos + Custom.DirVec(bs[1].pos, b0.pos) * 1f;
        var vector2 = grabbed.bodyChunks[g.chunkGrabbed].vel - b0.vel;
        grabbed.bodyChunks[g.chunkGrabbed].vel = b0.vel;
        if (!enteringShortCut.HasValue && (vector2.magnitude * grabbed.bodyChunks[g.chunkGrabbed].mass > 40f || !Custom.DistLess(vector, grabbed.bodyChunks[g.chunkGrabbed].pos, 70f + grabbed.bodyChunks[g.chunkGrabbed].rad)))
            ReleaseGrasp(0);
        else
            grabbed.bodyChunks[g.chunkGrabbed].MoveFromOutsideMyUpdate(eu, vector);
        if (g is not null)
        {
            for (var i = 0; i < 2; i++)
                g.grabbed.PushOutOf(bs[i].pos, bs[i].rad, g.chunkGrabbed);
        }
    }
}