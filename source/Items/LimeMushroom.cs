using UnityEngine;
using RWCustom;
using MoreSlugcats;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;
//CHK
public class LimeMushroom : PlayerCarryableItem, IDrawable, IPlayerEdible, IHaveAStalkState, IHaveAStalk
{
    public struct StalkPart
    {
        public LimeMushroom Owner;
        public Vector2 Pos, LastPos, Vel;

        public StalkPart(LimeMushroom owner)
        {
            Owner = owner;
            var fc = owner.firstChunk;
            Pos = fc.pos;
            LastPos = fc.pos;
        }

        public void Update()
        {
            LastPos = Pos;
            Pos += Vel;
            if (Owner.room.PointSubmerged(Pos))
                Vel *= .6f;
            else
                Vel *= .85f;
        }

        public void Reset()
        {
            LastPos = Owner.firstChunk.pos;
            Pos = Owner.firstChunk.pos;
            Vel *= 0f;
        }
    }

    public const int STALK_SPRITE = 0, HAT_SPRITE = 1, EFFECT_SPRITE = 3, TOTAL_SPRITES = 4;
    public StalkPart[] Stalk;
    public Vector2 Rotation, LastRotation, HoverPos;
    public float Darkness, LastDarkness, HoverDirAdd, Hue;
    public Color StalkColor;
    public Vector2? SetRotation, GrowPos;

    public virtual bool StalkActive => GrowPos is not null;

    public virtual AbstractConsumable AbstrConsumable => (abstractPhysicalObject as AbstractConsumable)!;

    public virtual int BitesLeft => 1;

    public virtual int FoodPoints => 0;

    public virtual bool Edible => true;

    public virtual bool AutomaticPickUp => false;

    public LimeMushroom(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        bodyChunks = [new(this, 0, default, 2f, .05f)];
        bodyChunkConnections = [];
        airFriction = .998f;
        gravity = .9f;
        bounce = .2f;
        surfaceFriction = .7f;
        collisionLayer = 0;
        waterFriction = .95f;
        buoyancy = .9f;
        Hue = 68f / 360f + 14f / 360f * abstractPhysicalObject.world.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed);
        var stk = Stalk = new StalkPart[6];
        for (var i = 0; i < stk.Length; i++)
            stk[i] = new(this);
    }

    public virtual void ResetParts()
    {
        var stk = Stalk;
        for (var i = 0; i < stk.Length; i++)
            stk[i].Reset();
    }

    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        ResetParts();
    }

    public override void Update(bool eu)
    {
        base.Update(eu);
        var fc = firstChunk;
        var crits = room.abstractRoom.creatures;
        for (var i = 0; i < crits.Count; i++)
        {
            var cr = crits[i];
            if (abstractPhysicalObject.SameRippleLayer(cr) && cr.realizedCreature is Creature c && c.Consious && c.mainBodyChunk is BodyChunk ch && Custom.DistLess(ch.pos, fc.pos, 70f) && (c is ThornBug or DropBug or TintedBeetle or NeedleWorm or SurfaceSwimmer or EggBug or DivingBeetle or Caterpillar || c.Template.type.value.Contains("Mosquito")))
                c.mainBodyChunk.vel += Custom.DirVec(fc.pos, ch.pos) * 6f;
        }
        LastDarkness = Darkness;
        Darkness = room.Darkness(fc.pos);
        LastRotation = Rotation;
        if (grabbedBy.Count > 0)
        {
            Rotation = Custom.PerpendicularVector(Custom.DirVec(fc.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            Rotation.y = Math.Abs(Rotation.y);
            DetatchStalk();
        }
        else if (!GrowPos.HasValue && fc.ContactPoint.y == 0 && fc.ContactPoint.x == 0)
        {
            Rotation += fc.pos - Stalk[2].Pos;
            Rotation.Normalize();
        }
        if (SetRotation.HasValue)
        {
            Rotation = SetRotation.Value;
            SetRotation = null;
        }
        if (fc.ContactPoint.y < 0)
        {
            Rotation = (Rotation - Custom.PerpendicularVector(Rotation) * .1f * fc.vel.x).normalized;
            fc.vel.x *= .8f;
        }
        var stk = Stalk;
        for (var i = 0; i < stk.Length; i++)
        {
            ref var st = ref stk[i];
            st.Update();
            if (!GrowPos.HasValue)
                st.Vel.y -= Mathf.InverseLerp(0f, stk.Length - 1, i) * .4f;
        }
        for (var j = 0; j < stk.Length; j++)
            ConnectStalkSegment(j);
        for (var num = stk.Length - 1; num >= 0; num--)
            ConnectStalkSegment(num);
        for (var k = 0; k < 4; k++)
        {
            ref var st = ref stk[k];
            st.Vel -= Rotation * Mathf.InverseLerp(4f, 0f, k);
            var vector = fc.pos - Rotation * (3 + k) * 5f;
            var val = Vector2.Dot((fc.pos - st.Pos).normalized, (fc.pos - vector).normalized);
            st.Vel = Vector2.Lerp(st.Vel, fc.pos - fc.lastPos, Custom.LerpMap(val, 1f, -1f, 0f, 1f) * Mathf.Pow(Mathf.InverseLerp(4f, 0f, k), .2f));
            st.Vel += (vector - st.Pos) / Custom.LerpMap(val, -1f, 1f, 3f, 30f) * Mathf.InverseLerp(4f, 0f, k);
            st.Pos += (vector - st.Pos) / Custom.LerpMap(val, -1f, 1f, 3f, 60f) * Mathf.InverseLerp(4f, 0f, k);
        }
        for (var l = 0; l < stk.Length; l++)
            ConnectStalkSegment(l);
        for (var num2 = stk.Length - 1; num2 >= 0; num2--)
            ConnectStalkSegment(num2);
        if (GrowPos.HasValue)
        {
            ref var st = ref stk[stk.Length - 1];
            st.Pos = GrowPos.Value;
            st.Vel *= 0f;
            fc.vel.y += gravity;
            fc.vel *= .7f;
            fc.vel += (HoverPos - fc.pos) / 20f;
            Rotation = Custom.DegToVec(Custom.AimFromOneVectorToAnother(GrowPos.Value, fc.pos) + HoverDirAdd);
        }
        for (var m = 2; m < stk.Length; m++)
        {
            ref var st = ref stk[m];
            ref var stm2 = ref stk[m - 2];
            var vector2 = Custom.DirVec(stm2.Pos, st.Pos);
            st.Vel += vector2 * 3.3f;
            stm2.Vel -= vector2 * 3.3f;
        }
    }

    public virtual void ConnectStalkSegment(int i)
    {
        ref var st = ref Stalk[i];
        if (i == 0)
        {
            var vec = (2.5f - Vector2.Distance(st.Pos, firstChunk.pos)) * Custom.DirVec(st.Pos, firstChunk.pos);
            st.Pos -= vec;
            st.Vel -= vec;
        }
        else
        {
            ref var stm1 = ref Stalk[i - 1];
            var vec = (2.5f - Vector2.Distance(st.Pos, stm1.Pos)) * Custom.DirVec(st.Pos, stm1.Pos) * .5f;
            st.Pos -= vec;
            st.Vel -= vec;
            stm1.Pos += vec;
            stm1.Vel += vec;
        }
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        var fc = firstChunk;
        if ((!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count) || (ModManager.MMF && placeRoom.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || placeRoom.game.GetArenaGameSession.chMeta is not null) && placeRoom.game.GetArenaGameSession.counter < 10))
        {
            if (ModManager.MMF && placeRoom.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta is not null))
                fc.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            else
                fc.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
            var tlPos = Room.StaticGetTilePosition(fc.pos);
            int x = tlPos.x, y = tlPos.y;
            while (y >= 0 && y >= tlPos.y - 4)
            {
                if (!placeRoom.GetTile(x, y).Solid && placeRoom.GetTile(x, y - 1).Solid)
                {
                    var mid = placeRoom.MiddleOfTile(x, y);
                    GrowPos = new(mid.x + Mathf.Lerp(-9f, 9f, placeRoom.game.SeededRandom((int)fc.pos.x + (int)fc.pos.y)), mid.y - 10f);
                    HoverPos = new(GrowPos.Value.x + Mathf.Lerp(-7f, 7f, placeRoom.game.SeededRandom((int)fc.pos.x - (int)fc.pos.y)), GrowPos.Value.y + Mathf.Lerp(18f, 36f, placeRoom.game.SeededRandom((int)fc.pos.y - (int)fc.pos.x)));
                    HoverDirAdd = Mathf.Lerp(-25f, 25f, placeRoom.game.SeededRandom((int)fc.pos.x));
                    fc.HardSetPosition(HoverPos);
                }
                --y;
            }
        }
        else
        {
            fc.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            Rotation = Custom.RNV();
            LastRotation = Rotation;
        }
        ResetParts();
    }

    public override void HitByWeapon(Weapon weapon)
    {
        base.HitByWeapon(weapon);
        DetatchStalk();
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprs = sLeaser.sprites = new FSprite[TOTAL_SPRITES];
        sprs[STALK_SPRITE] = TriangleMesh.MakeLongMesh(Stalk.Length, false, false);
        sprs[HAT_SPRITE] = new("MushroomA") { scaleY = 1.8f };
        sprs[HAT_SPRITE + 1] = new("MushroomB") { scaleY = 1.8f };
        sprs[EFFECT_SPRITE] = new("Futile_White") { shader = Custom.rainWorld.Shaders["FlatLightBehindTerrain"] };
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var flag = blink > 0 && Random.value < .5f;
        Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker),
            v = Vector3.Slerp(LastRotation, Rotation, timeStacker);
        var hat = sLeaser.sprites[HAT_SPRITE];
        var hatCol = sLeaser.sprites[HAT_SPRITE + 1];
        hat.SetPosition(vector - camPos);
        hatCol.SetPosition(vector - camPos);
        hatCol.rotation = hat.rotation = Custom.VecToDeg(v);
        var num = Mathf.Lerp(LastDarkness, Darkness, timeStacker);
        hat.color = StalkColor;
        hatCol.color = flag ? blinkColor : color;
        var effect = sLeaser.sprites[EFFECT_SPRITE];
        effect.SetPosition(vector - camPos);
        effect.scale = Mathf.Lerp(15f, 30f, num) / 16f;
        effect.alpha = .65f;
        effect.color = Custom.HSL2RGB(Hue, .9f + .1f * num, .4f + .2f * num);
        var vector2 = vector;
        var num2 = .75f;
        var stk = Stalk;
        var stalkSpr = (sLeaser.sprites[STALK_SPRITE] as TriangleMesh)!;
        stalkSpr.color = StalkColor;
        for (var i = 0; i < stk.Length; i++)
        {
            ref readonly var st = ref stk[i];
            Vector2 vector3 = Vector2.Lerp(st.LastPos, st.Pos, timeStacker),
                normalized = (vector3 - vector2).normalized,
                vector4 = Custom.PerpendicularVector(normalized);
            var num3 = Vector2.Distance(vector3, vector2) / 5f;
            if (i == 0)
            {
                stalkSpr.MoveVertice(i * 4, vector2 - vector4 * num2 - camPos);
                stalkSpr.MoveVertice(i * 4 + 1, vector2 + vector4 * num2 - camPos);
            }
            else
            {
                stalkSpr.MoveVertice(i * 4, vector2 - vector4 * num2 + normalized * num3 - camPos);
                stalkSpr.MoveVertice(i * 4 + 1, vector2 + vector4 * num2 + normalized * num3 - camPos);
            }
            stalkSpr.MoveVertice(i * 4 + 2, vector3 - vector4 * num2 - normalized * num3 - camPos);
            stalkSpr.MoveVertice(i * 4 + 3, vector3 + vector4 * num2 - normalized * num3 - camPos);
            vector2 = vector3;
        }
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        color = Custom.HSL2RGB(Hue, 1f, .5f);
        StalkColor = palette.blackColor;
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer = rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var sprite = sprs[i];
            sprite.RemoveFromContainer();
            if (i == EFFECT_SPRITE)
                rCam.ReturnFContainer("Foreground").AddChild(sprite);
            else
                newContainer.AddChild(sprite);
        }
    }

    public virtual void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        grasp.grabber.Stun(75);
        if (grasp.grabber is Player p)
        {
            if (p.FoodInStomach > 0)
                p.AddFood(-1);
            p.ObjectEaten(this);
        }
        grasp.Release();
        Destroy();
    }

    public virtual void ThrowByPlayer() { }

    public virtual void DetatchStalk()
    {
        if (!AbstrConsumable.isConsumed)
            AbstrConsumable.Consume();
        if (GrowPos.HasValue)
            GrowPos = null;
    }
}