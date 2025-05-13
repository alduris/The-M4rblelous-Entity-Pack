using MoreSlugcats;
using RWCustom;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class Durian : Rock, IHaveAStalk, IHaveAStalkState
{
    public class Stalk : UpdatableAndDeletable, IDrawable
    {
        public Durian? Fruit;
        public Vector2[] Displacements;
        public Vector2[][] Segs;
        public Vector2 StuckPos, BaseFruitPos;
        public float RopeLength, ConnRad;
        public int ReleaseCounter;

        public Stalk(Durian fruit, Room room, Vector2 fruitPos)
        {
            Fruit = fruit;
            fruit.firstChunk.HardSetPosition(fruitPos);
            StuckPos.x = fruitPos.x;
            RopeLength = -1f;
            BaseFruitPos = fruitPos;
            var fpos = Room.StaticGetTilePosition(fruitPos);
            var x = fpos.x;
            int i, height = room.TileHeight;
            for (i = fpos.y; i < height; i++)
            {
                if (room.GetTile(x, i).Solid)
                {
                    StuckPos.y = room.MiddleOfTile(x, i).y - 10f;
                    RopeLength = Math.Abs(StuckPos.y - fruitPos.y);
                    break;
                }
            }
            var segs = Segs = new Vector2[Math.Max(1, (int)(RopeLength / 20f))][];
            height = segs.Length;
            var lf = (float)(height - 1);
            var sp = StuckPos;
            for (i = 0; i < segs.Length; i++)
            {
                var seg = segs[i] = new Vector2[3];
                seg[0] = Vector2.Lerp(sp, fruitPos, i / lf);
                seg[1] = seg[0];
            }
            ConnRad = RopeLength / Mathf.Pow(height, 1.1f);
            var disps = Displacements = new Vector2[height];
            var state = Random.state;
            Random.InitState(fruit.abstractPhysicalObject.ID.RandomSeed);
            for (i = 0; i < disps.Length; i++)
                disps[i] = Custom.RNV();
            Random.state = state;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (RopeLength == -1f)
            {
                if (Fruit is Durian fruit1)
                    fruit1.MyStalk = null;
                Destroy();
                return;
            }
            ConnectSegments(true);
            ConnectSegments(false);
            var segs = Segs;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                seg[1] = seg[0];
                seg[0] += seg[2];
                seg[2] *= .99f;
                seg[2].y -= .9f;
            }
            ConnectSegments(false);
            ConnectSegments(true);
            if (ReleaseCounter > 0)
                --ReleaseCounter;
            if (Fruit is Durian fruit)
            {
                var fc = fruit.firstChunk;
                fruit.setRotation = Custom.DirVec(fc.pos, segs[segs.Length - 1][0]);
                var dst = Vector2.Distance(fc.pos, BaseFruitPos);
                if (dst > .05f)
                    fc.vel += Custom.DirVec(fc.pos, BaseFruitPos) * dst * .02f;
                if (!Custom.DistLess(fc.pos, StuckPos, RopeLength * 1.8f + 10f) || fruit.slatedForDeletetion || fruit.room != room || ReleaseCounter == 1)
                {
                    fruit.AbstrDurian.Consume();
                    fruit.MyStalk = null;
                    Fruit = null;
                }
            }
        }

        public virtual void ConnectSegments(bool dir)
        {
            var segs = Segs;
            var sp = StuckPos;
            var crad = ConnRad;
            var l = segs.Length;
            var num = dir ? 0 : l - 1;
            var flag = false;
            while (!flag)
            {
                var seg = segs[num];
                if (num == 0)
                {
                    if (!Custom.DistLess(seg[0], sp, crad))
                    {
                        var vector = Custom.DirVec(seg[0], sp) * (Vector2.Distance(seg[0], sp) - crad);
                        seg[0] += vector;
                        seg[2] += vector;
                    }
                }
                else
                {
                    var segm1 = segs[num - 1];
                    if (!Custom.DistLess(seg[0], segm1[0], crad))
                    {
                        var vector2 = Custom.DirVec(seg[0], segm1[0]) * (Vector2.Distance(seg[0], segm1[0]) - crad) * .5f;
                        seg[0] += vector2;
                        seg[2] += vector2;
                        segm1[0] -= vector2;
                        segm1[2] -= vector2;
                    }
                    if (num == l - 1 && Fruit is Durian fruit && !Custom.DistLess(segm1[0], fruit.firstChunk.pos, crad))
                    {
                        var fc = fruit.firstChunk;
                        Vector2 vector3 = Custom.DirVec(seg[0], fc.pos) * (Vector2.Distance(seg[0], fc.pos) - crad);
                        seg[0] += vector3 * .75f;
                        seg[2] += vector3 * .75f;
                        fc.vel -= vector3 * .25f;
                    }
                }
                num += dir ? 1 : -1;
                if (dir && num >= l)
                    flag = true;
                else if (!dir && num < 0)
                    flag = true;
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = [TriangleMesh.MakeLongMesh(Segs.Length, false, false)];
            AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            var vector = StuckPos;
            var num = 3f;
            var mesh = (TriangleMesh)sLeaser.sprites[0];
            var segs = Segs;
            var l = segs.Length - 1;
            var crad = ConnRad;
            for (var i = 0; i < segs.Length; i++)
            {
                var seg = segs[i];
                float num2 = i / (float)l,
                    num3 = Custom.LerpMap(num2, 0f, .5f, 1f, 0f) + Mathf.Lerp(1f, .5f, Mathf.Sin(Mathf.Pow(num2, 3.5f) * Mathf.PI));
                Vector2 vector2;
                if (i == l && Fruit is Durian fruit)
                    vector2 = Vector2.Lerp(fruit.firstChunk.lastPos, fruit.firstChunk.pos, timeStacker);
                else
                    vector2 = Vector2.Lerp(seg[1], seg[0], timeStacker);
                Vector2 normalized = (vector - vector2).normalized,
                    vector3 = Custom.PerpendicularVector(normalized);
                if (i < l)
                {
                    var disp = Displacements[i];
                    vector2 += (normalized * disp.y + vector3 * disp.x) * Custom.LerpMap(Vector2.Distance(vector, vector2), crad, crad * 5f, 4f, 0f);
                }
                vector2 = new Vector2(Mathf.Floor(vector2.x) + .5f, Mathf.Floor(vector2.y) + .5f);
                var i4 = i * 4;
                mesh.MoveVertice(i4, vector - vector3 * num - camPos);
                mesh.MoveVertice(i4 + 1, vector + vector3 * num - camPos);
                mesh.MoveVertice(i4 + 2, vector2 - vector3 * num3 - camPos);
                mesh.MoveVertice(i4 + 3, vector2 + vector3 * num3 - camPos);
                vector = vector2;
                num = num3;
            }
            if (slatedForDeletetion || room != rCam.room)
                sLeaser.CleanSpritesAndRemove();
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => sLeaser.sprites[0].color = palette.blackColor;

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            var spr = sLeaser.sprites[0];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
            spr.MoveToBack();
        }
    }

    public Stalk? MyStalk;

    public virtual bool StalkActive => MyStalk is not null;

    public virtual AbstractConsumable AbstrDurian => (abstractPhysicalObject as AbstractConsumable)!;

    public Durian(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject, abstractPhysicalObject.world) => buoyancy = 1.6f;

    public override void Update(bool eu)
    {
        base.Update(eu);
        if (mode != Mode.Thrown)
        {
            var crits = room.abstractRoom.creatures;
            var fc = firstChunk;
            for (var i = 0; i < crits.Count; i++)
            {
                var cr = crits[i];
                if (abstractPhysicalObject.SameRippleLayer(cr) && cr.realizedCreature is Lizard c && c is not CommonEel && c.Consious && c.mainBodyChunk is BodyChunk ch && Custom.DistLess(ch.pos, fc.pos, 70f))
                    c.mainBodyChunk.vel += Custom.DirVec(fc.pos, ch.pos);
            }
        }
    }

    public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
    {
        if (result.obj is not PhysicalObject obj)
            return false;
        if (!obj.abstractPhysicalObject.SameRippleLayer(abstractPhysicalObject))
            return false;
        if (thrownBy is Scavenger scav && scav.AI is ScavengerAI AI)
            AI.HitAnObjectWithWeapon(this, obj);
        vibrate = 20;
        var fc = firstChunk;
        if (obj is Creature cr)
        {
            var stunBonus = 45f;
            if ((ModManager.MMF && MMF.cfgIncreaseStuns.Value && (obj is Cicada or LanternMouse || (ModManager.MSC && obj is Yeek))) || (ModManager.MSC && room.game.session is ArenaGameSession sess && sess.chMeta is not null))
                stunBonus = 90f;
            var lizardHead = obj is Lizard and not CommonEel && result.chunk?.index == 0;
            if (lizardHead)
            {
                stunBonus *= 2.5f;
                cr.Stun(60);
            }
            cr.Violence(fc, fc.vel * fc.mass, result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, lizardHead ? .2f : .075f, stunBonus);
        }
        else if (result.chunk is BodyChunk b)
        {
            room.AddObject(new ExplosionSpikes(room, b.pos + Custom.DirVec(b.pos, result.collisionPoint) * b.rad, 5, 2f, 4f, 4.5f, 30f, new(1f, 1f, 1f, .5f)));
            b.vel += fc.vel * fc.mass / b.mass;
        }
        else if (result.onAppendagePos is Appendage.Pos ps)
            (obj as IHaveAppendages)!.ApplyForceOnAppendage(ps, fc.vel * fc.mass);
        ChangeMode(Mode.Free);
        fc.vel = fc.vel * -.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(.1f, .4f, Random.value) * fc.vel.magnitude;
        room.PlaySound(SoundID.Rock_Hit_Creature, fc);
        SetRandomSpin();
        return true;
    }

    public override void PlaceInRoom(Room placeRoom)
    {
        base.PlaceInRoom(placeRoom);
        if (ModManager.MMF && room.game.session is ArenaGameSession sess && (MMF.cfgSandboxItemStems.Value || sess.chMeta is not null) && sess.counter < 10)
        {
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            placeRoom.AddObject(MyStalk = new Stalk(this, placeRoom, firstChunk.pos));
        }
        else if (!AbstrDurian.isConsumed && AbstrDurian.placedObjectIndex >= 0 && AbstrDurian.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
        {
            firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrDurian.placedObjectIndex].pos);
            placeRoom.AddObject(MyStalk = new Stalk(this, placeRoom, firstChunk.pos));
        }
        else
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
        rotationSpeed = 0f;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("DurianA"), new("DurianB")];
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var ps = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker) - camPos;
        if (vibrate > 0)
            ps += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
        var s0 = sLeaser.sprites[0];
        var s1 = sLeaser.sprites[1];
        s0.SetPosition(ps);
        s1.SetPosition(ps);
        var rot = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
        if (blink > 0 && Random.value < .5f)
            s0.color = blinkColor;
        else
            s0.color = color;
        s1.rotation = s0.rotation = rot;
        if (slatedForDeletetion || room != rCam.room)
            sLeaser.CleanSpritesAndRemove();
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        sLeaser.sprites[1].color = palette.blackColor;
        color = Color.Lerp(new(121f / 255f, 130f / 255f, 15f / 255f), palette.blackColor, Mathf.Lerp(0f, .5f, rCam.PaletteDarkness()));
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Items");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public virtual void DetatchStalk()
    {
        if (MyStalk is Stalk st && st.ReleaseCounter == 0)
            st.ReleaseCounter = 2;
    }
}