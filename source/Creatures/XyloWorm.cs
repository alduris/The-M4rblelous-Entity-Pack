/*using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Creatures;

public class XyloWorm : Creature, IPlayerEdible
{
    public Vector2 LookDir, BodyDir;
    public IntVector2 LastAirTile;
    public float Lungs, Wiggle, Swallowed;
    public int Bites = 2;
    public bool Big;

    public virtual int BitesLeft => Bites;

    public virtual int FoodPoints => Big ? 2 : 1;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => true;

    public XyloWorm(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        Big = abstractCreature.superSizeMe;
        var fac = Big ? 2.5f : 1f;
        var chs = bodyChunks =
        [
            new(this, 1, default, 3f * fac, .0625f * fac),
            new(this, 0, default, 3.5f * fac, .0625f * fac),
            new(this, 2, default, 3f * fac, .0625f * fac)
        ];
        bodyChunkConnections =
        [
            new(chs[1], chs[0], 7f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(chs[0], chs[2], 7f, BodyChunkConnection.Type.Normal, 1f, -1f),
            new(chs[1], chs[2], 3.5f, BodyChunkConnection.Type.Push, 1f, -1f)
        ];
        airFriction = .995f;
        gravity = .9f;
        bounce = .1f;
        surfaceFriction = .1f;
        collisionLayer = 1;
        waterFriction = .96f;
        buoyancy = .95f;
        LookDir = Custom.RNV() * Random.value;
        BodyDir = Custom.RNV() * Random.value;
    }

    public BodyChunk ChunkInOrder(int i) => bodyChunks[i switch
    {
        1 => 0,
        0 => 1,
        _ => 2,
    }];

    public override void InitiateGraphicsModule() => graphicsModule ??= new XyloWormGraphics(this);

    public override void Update(bool eu)
    {
        var flag1 = grabbedBy.Count == 0;
        CollideWithTerrain = flag1;
        GoThroughFloors = !flag1;
        CollideWithObjects = flag1;
        var chs = bodyChunks;
        WeightedPush(1, 2, Custom.DirVec(chs[2].pos, chs[1].pos), Custom.LerpMap(Vector2.Distance(chs[2].pos, chs[1].pos), 3.5f, 8f, 1f, 0f));
        if (!room.GetTile(chs[0].pos).Solid)
            LastAirTile = room.GetTilePosition(chs[0].pos);
        else
        {
            for (var i = 0; i < chs.Length; i++)
                chs[i].HardSetPosition(room.MiddleOfTile(LastAirTile) + Custom.RNV());
        }
        base.Update(eu);
        if (room is not Room rm)
            return;
        if (rm.game.devToolsActive && Input.GetKey("b") && rm.game.cameras[0].room == rm)
        {
            chs[0].vel += Custom.DirVec(chs[0].pos, (Vector2)Futile.mousePosition + rm.game.cameras[0].pos) * 14f;
            Stun(12);
        }
        if (grabbedBy.Count > 0)
        {
            var dir = Custom.PerpendicularVector(Custom.DirVec(chs[0].pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            dir.y = Mathf.Abs(dir.y);
            WeightedPush(1, 2, dir, 4f);
        }
        if (!dead)
            Act();
        if (!dead && chs[0].submersion > .5f)
        {
            Lungs = Mathf.Max(Lungs - 1f / 180f, 0f);
            if (Lungs == 0f)
                Die();
        }
        else
            Lungs = Mathf.Min(Lungs + .02f, 1f);
        var flag = false;
        if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player p && p.swallowAndRegurgitateCounter > 50 && p.objectInStomach is null && p.input[0].pckp)
        {
            var num = -1;
            for (var j = 0; j < 2; j++)
            {
                if (p.grasps[j] is Grasp g && p.CanBeSwallowed(g.grabbed))
                {
                    num = j;
                    break;
                }
            }
            if (num > -1 && p.grasps[num] is Grasp g1 && g1.grabbed == this)
                flag = true;
        }
        Swallowed = Custom.LerpAndTick(Swallowed, flag ? 1f : 0f, .05f, .05f);
    }

    public virtual void Act()
    {
        var chs = bodyChunks;
        if (!safariControlled || (safariControlled && inputWithoutDiagonals is Player.InputPackage pack && (pack.x != 0 || pack.y != 0)))
        {
            LookDir += Custom.RNV() * Random.value * .25f + new Vector2(0f, .025f * room.gravity * Random.value * Random.value);
            BodyDir += Custom.RNV() * Random.value * .25f;
        }
        else
            BodyDir = default;
        LookDir = Vector2.ClampMagnitude(LookDir, 1f);
        BodyDir = Vector2.ClampMagnitude(BodyDir, 1f);
        var norm = LookDir.normalized;
        var mag = LookDir.magnitude;
        WeightedPush(1, 0, norm, mag);
        WeightedPush(1, 2, norm, mag);
        norm = BodyDir * .2f;
        chs[0].vel += norm;
        chs[1].vel -= norm;
        chs[2].vel -= norm;
        norm = BodyDir.normalized;
        mag = BodyDir.magnitude;
        WeightedPush(0, 1, norm, mag);
        WeightedPush(0, 2, norm, mag);
        if (safariControlled)
        {
            if (inputWithoutDiagonals is not Player.InputPackage pack2)
                return;
            if (pack2.y != 0 || pack2.x != 0)
            {
                Wiggle = Mathf.Max(Random.Range(0f, .001f), Custom.LerpAndTick(Wiggle, 0f, .05f, .025f));
                if (Random.value < 1f / 60f)
                    Wiggle = Mathf.Max(Wiggle, Random.value);
                Vector2 vector = Custom.RNV(),
                    vector2 = Custom.RNV();
                var wiggle = Mathf.Pow(Wiggle, .1f) * 5f;
                var sgn = Math.Sign(pack2.x);
                chs[1].vel += new Vector2(Mathf.Abs(vector.x) * sgn * Math.Abs(pack2.x), vector.y) * Random.value * wiggle;
                chs[2].vel += new Vector2(Mathf.Abs(vector2.x) * sgn * Mathf.Abs(pack2.x), vector2.y) * Random.value * wiggle;
                if (pack2.x != 0)
                {
                    LookDir.x = Mathf.Abs(LookDir.x) * sgn;
                    BodyDir.x = Mathf.Abs(BodyDir.x) * sgn;
                }
            }
            else
                Wiggle = 0f;
        }
        else
        {
            Wiggle = Custom.LerpAndTick(Wiggle, 0f, .05f, .025f);
            if (Random.value < 1f / 60f)
                Wiggle = Mathf.Max(Wiggle, Random.value);
            if (Wiggle > 0f)
            {
                var wiggle = Mathf.Pow(Wiggle, .1f) * 5f;
                chs[1].vel += Custom.RNV() * Random.value * wiggle;
                chs[2].vel += Custom.RNV() * Random.value * wiggle;
            }
        }
    }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        var vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
        var chs = bodyChunks;
        for (var i = 0; i < chs.Length; i++)
        {
            chs[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + i) * 15f;
            chs[i].lastPos = newRoom.MiddleOfTile(pos);
            chs[i].vel = vector * 8f;
        }
        graphicsModule?.Reset();
    }

    public virtual void BitByPlayer(Grasp grasp, bool eu)
    {
        --Bites;
        if (!dead)
            Die();
        room.PlaySound(SoundID.Slugcat_Eat_Centipede, firstChunk);
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (Bites < 1)
        {
            (grasp.grabber as Player)?.ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public virtual void ThrowByPlayer() { }
}*/