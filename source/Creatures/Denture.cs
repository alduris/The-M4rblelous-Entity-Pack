using Noise;
using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class Denture : Creature
{
    public float JawRad;
    public int NoiseReaction, CreatureEaten;
    public float LastSuckedIntoShortcut, SuckedIntoShortcut, LastJawOpen, JawOpen;
    public IntVector2 ShortCutPos;
    public Vector2 RootPos, OutDir = Vector2.up;
    public bool CreatureMissed;

    public override float VisibilityBonus => -JawOpen * .8f;

    public Denture(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
    {
        bodyChunks = [new(this, 0, default, 2f, .2f)
        {
            collideWithTerrain = false,
            collideWithObjects = false,
            collideWithSlopes = false
        }];
        bodyChunkConnections = [];
        abstractCreature.tentacleImmune = true;
        abstractCreature.HypothermiaImmune = true;
        abstractCreature.lavaImmune = true;
        JawRad = abstractCreature.superSizeMe ? 80f : 50f;
        GoThroughFloors = true;
        airFriction = .99f;
        gravity = .98f;
        bounce = .01f;
        surfaceFriction = .47f;
        collisionLayer = 1;
        waterFriction = .94f;
        buoyancy = .01f;
    }

    public override void InitiateGraphicsModule() => graphicsModule ??= new DentureGraphics(this);

    public override void NewRoom(Room room)
    {
        base.NewRoom(room);
        var nodeCoord = room.LocalCoordinateOfNode(abstractCreature.pos.abstractNode);
        ShortCutPos = nodeCoord.Tile;
        OutDir = room.ShorcutEntranceHoleDirection(ShortCutPos).ToVector2();
        RootPos = room.MiddleOfTile(nodeCoord) + OutDir * 25f;
        JawOpen = 1f;
    }

    public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
        JawOpen = 0f;
        SuckedIntoShortcut = 1f;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room is not Room rm)
            return;
        /*if (rm.game?.devToolsActive is true && Input.GetKey("d"))
        {
            if (Input.GetKey("b"))
            {
                abstractCreature.superSizeMe = true;
                JawRad = 100f;
            }
            else if (Input.GetKey("s"))
            {
                abstractCreature.superSizeMe = false;
                JawRad = 50f;
            }
            else if (graphicsModule is DentureGraphics gr)
            {
                if (Input.GetKey("w"))
                    gr.AlbinoForm = true;
                else if (Input.GetKey("k"))
                    gr.AlbinoForm = false;
            }
        }*/
        var cs = Consious;
        if (!cs)
            NoiseReaction = 0;
        else if (NoiseReaction > 0)
            --NoiseReaction;
        LastSuckedIntoShortcut = SuckedIntoShortcut;
        LastJawOpen = JawOpen;
        abstractCreature.pos.Tile = rm.GetTilePosition(RootPos);
        if (CreatureEaten > 0)
            --CreatureEaten;
        if (enteringShortCut.HasValue)
        {
            CreatureMissed = false;
            SuckedIntoShortcut = Mathf.Lerp(SuckedIntoShortcut, 1f, .1f);
            return;
        }
        firstChunk.HardSetPosition(RootPos);
        if (CreatureEaten == 0)
            SuckedIntoShortcut = Mathf.Lerp(SuckedIntoShortcut, 0f, .1f);
        if (cs && SuckedIntoShortcut > .05f)
            JawOpen = 0f;
        else
        {
            var safari = safariControlled;
            var stayInDenNotDead = abstractCreature.WantToStayInDenUntilEndOfCycle() && !dead;
            if (cs && !safari && (NoiseReaction > 0 || stayInDenNotDead))
            {
                JawOpen = Math.Max(JawOpen * (JawOpen - (stayInDenNotDead ? .2f : .08f)), 0f);
                if (stayInDenNotDead && JawOpen < .05f)
                {
                    SuckedIntoShortcut = Mathf.Lerp(SuckedIntoShortcut, 1f, .1f);
                    enteringShortCut = ShortCutPos;
                    rm.PlaySound(SoundID.Leviathan_Crush_Non_Organic_Object, firstChunk.pos, .72f, 1.2f);
                }
            }
            else if (cs && ((!safari && TastyChunkInRange(rm) && CreatureEaten == 0 && !CreatureMissed) || (safari && !CreatureMissed && inputWithDiagonals?.pckp == true)))
            {
                JawOpen = Math.Max(JawOpen * (JawOpen - .2f), 0f);
                if (graphicsModule is DentureGraphics gr)
                    gr.MoveToFront = true;
                if (JawOpen < .05f)
                {
                    if (safari)
                        CreatureMissed = true;
                    Crush(rm);
                }
            }
            else if (CreatureEaten > 0 && (dead || !safari))
            {
                JawOpen = 0f;
                if (CreatureEaten == 1)
                {
                    SuckedIntoShortcut = Mathf.Lerp(SuckedIntoShortcut, 1f, .1f);
                    enteringShortCut = ShortCutPos;
                    if (!dead)
                        abstractCreature.remainInDenCounter = 145;
                }
            }
            else
            {
                JawOpen = Mathf.Lerp(JawOpen, 1f, .1f);
                if (JawOpen >= .95f)
                    CreatureMissed = false;
            }
        }
    }

    public virtual float GraphicsAngleDir()
    {
        if (OutDir == Vector2.up)
            return 0f;
        if (OutDir == Vector2.down)
            return 180f;
        if (OutDir == Vector2.left)
            return 270f;
        return 90f;
    }

    public virtual bool WantsToEat(Creature c)
    {
        var rel = Template.CreatureRelationship(c).type;
        return rel == CreatureTemplate.Relationship.Type.Eats || (abstractCreature.superSizeMe && rel == CreatureTemplate.Relationship.Type.Attacks);
    }

    public virtual bool TastyChunkInRange(Room rm)
    {
        var rd = JawRad * (abstractCreature.superSizeMe ? .6f : .7f);
        var fcp = firstChunk.pos;
        var crits = rm.abstractRoom.creatures;
        for (var i = 0; i < crits.Count; i++)
        {
            if (crits[i]?.realizedCreature is Creature c && WantsToEat(c) && BiggestCreatureChunk(c) is BodyChunk ch && DistLess(ch.pos, fcp, rd))
            {
                bool flag;
                if (OutDir == Vector2.up)
                    flag = fcp.y < ch.pos.y - 5f;
                else if (OutDir == Vector2.down)
                    flag = fcp.y > ch.pos.y + 5f;
                else if (OutDir == Vector2.left)
                    flag = fcp.x > ch.pos.x + 5f;
                else
                    flag = fcp.x < ch.pos.x - 5f;
                if (flag)
                    return true;
            }
        }
        return false;
    }

    public virtual void Crush(Room rm)
    {
        var rd = JawRad * (abstractCreature.superSizeMe ? .6f : .7f);
        var fcp = firstChunk.pos;
        var crits = rm.abstractRoom.creatures;
        bool playerCrush = false, badFood = false;
        for (var i = 0; i < crits.Count; i++)
        {
            if (crits[i]?.realizedCreature is Creature c && WantsToEat(c) && BiggestCreatureChunk(c) is BodyChunk ch && DistLess(ch.pos, fcp, rd))
            {
                c.Die();
                c.Destroy();
                CreatureEaten = 8;
                if (c is Player)
                    playerCrush = true;
                else if (c is ThornBug or Sporantula)
                    badFood = true;
            }
        }
        if (CreatureEaten > 0)
        {
            rm.PlaySound(playerCrush ? SoundID.Leviathan_Crush_Player : SoundID.Leviathan_Crush_NPC, fcp, .82f, 1.2f);
            for (var i = 0; i < 10; i++)
            {
                rm.AddObject(new WaterDrip(fcp, new Vector2(Random.value, Random.value).normalized, false));
                rm.AddObject(new Bubble(fcp, new Vector2(Random.value, Random.value).normalized, false, false));
            }
            if (badFood)
                DieOfIndigestion();
        }
        else
        {
            CreatureMissed = true;
            rm.PlaySound(SoundID.Leviathan_Crush_Non_Organic_Object, fcp, .72f, 1.2f);
        }
    }

    public override void Stun(int st) { }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk) { }

    public override void HitByWeapon(Weapon weapon) { }

    public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk) { }

    public override void HeardNoise(InGameNoise noise)
    {
        if (room?.VisualContact(firstChunk.pos + OutDir * 20f, noise.pos) is true && noise.strength > 160f)
            NoiseReaction = 2;
    }

    public override bool AllowableControlledAIOverride(MovementConnection.MovementType movementType) => base.AllowableControlledAIOverride(movementType) && Consious;

    public override void Blind(int blnd) { }

    public override bool CanBeGrabbed(Creature grabber) => false;

    public override void Deafen(int df) { }

    public override void Die() { }

    public virtual void DieOfIndigestion()
    {
        if (!dead)
        {
            Custom.LogImportant("Die!", Template.name);
            dead = true;
            abstractCreature.Die();
        }
    }

    public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus) { }

    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact) { }

    public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction) => false;

    public override bool Grab(PhysicalObject obj, int graspUsed, int chunkGrabbed, Grasp.Shareability shareability, float dominance, bool overrideEquallyDominant, bool pacifying) => false;

    public override void Grabbed(Grasp grasp) => grasp?.Release();

    public override void GrabbedObjectSnatched(PhysicalObject grabbedObject, Creature thief) { }

    public override void LoseAllGrasps() { }

    public override Color ShortCutColor() => new(.1f, .1f, .1f);

    public override void ReleaseGrasp(int grasp) { }

    public override void PlaceInRoom(Room placeRoom)
    {
        placeRoom.AddObject(this);
        NewRoom(placeRoom);
    }

    public static BodyChunk? BiggestCreatureChunk(Creature c)
    {
        var chs = c.bodyChunks;
        if (chs.Length == 0)
            return null;
        BodyChunk biggestChunk = chs[0];
        for (var i = 1; i < chs.Length; i++)
        {
            var ch = chs[i];
            if (ch.rad > biggestChunk.rad)
                biggestChunk = ch;
        }
        return biggestChunk;
    }

    public override void PushOutOf(Vector2 pos, float rad, int exceptedChunk) { }

    public static bool DistLess(Vector2 a, Vector2 b, float dst)
    {
        var x = b.x - a.x;
        var y = b.y - a.y;
        return x * x + y * y < dst * dst;
    }
}