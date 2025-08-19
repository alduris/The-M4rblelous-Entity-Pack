using MoreSlugcats;
using RWCustom;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using RagPart = (UnityEngine.Vector2 Pos, UnityEngine.Vector2 LastPos, UnityEngine.Vector2 Vel, UnityEngine.Vector2 Rot, UnityEngine.Vector2 LastRot, UnityEngine.Vector2 RotVel);
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class M4RScavMaskGraphics : VultureMaskGraphics
{
    [StructLayout(LayoutKind.Sequential)]
    public class Rag
    {
        public const float CONNEC_RAD = 7f;
        public M4RScavMaskGraphics Owner;
        public RagPart[] Parts;
        public Vector2 AttachPos;
        public int StartSprite, LastLayer, Layer, Index;
        public float Darkness;
        public SharedPhysics.TerrainCollisionData ScratchTerrainCollisionData;

        public Rag(M4RScavMaskGraphics ow, int length, int startSprite, int index)
        {
            Owner = ow;
            Index = index;
            StartSprite = startSprite;
            Parts = new RagPart[length];
            ScratchTerrainCollisionData.goThroughFloors = true;
            ScratchTerrainCollisionData.rad = 1f;
            Reset();
        }

        public virtual void Reset()
        {
            AttachPos = Owner.RagAttachPos(Index);
            var rag = Parts;
            for (var i = 0; i < rag.Length; i++)
            {
                ref var r = ref rag[i];
                r.LastPos = r.Pos = AttachPos;
                r.Vel = default;
            }
        }

        public virtual void Update()
        {
            var parts = Parts;
            int i;
            for (i = 0; i < parts.Length; i++)
            {
                ref var r = ref parts[i];
                var t = (float)i / (parts.Length - 1);
                r.LastPos = r.Pos;
                r.Pos += r.Vel;
                r.Vel -= Owner.rotationA * Mathf.InverseLerp(1f, 0f, i) * .8f;
                r.LastRot = r.Rot;
                var dist = Vector2.Distance(r.Pos, r.LastPos);
                r.Rot = (r.Rot + r.RotVel * Custom.LerpMap(dist, 1f, 18f, .05f, .3f)).normalized;
                r.RotVel = (r.RotVel + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, dist), .3f)).normalized;
                if (Owner.Room?.PointSubmerged(r.Pos) is true)
                {
                    r.Vel *= Custom.LerpMap(r.Vel.magnitude, 1f, 10f, 1f, .5f, Mathf.Lerp(1.4f, .4f, t));
                    r.Vel.y += .05f;
                    r.Vel += Custom.RNV() * .1f;
                    continue;
                }
                r.Vel *= Custom.LerpMap(dist, 1f, 6f, .999f, .7f, Mathf.Lerp(1.5f, .5f, t));
                r.Vel.y -= (Owner.Room?.gravity ?? 1f) * Custom.LerpMap(dist, 1f, 6f, .6f, 0f);
                if (i % 3 == 2 || i == parts.Length - 1)
                {
                    ref var cd = ref ScratchTerrainCollisionData;
                    cd.pos = r.Pos;
                    cd.vel = r.Vel;
                    cd.lastPos = r.LastPos;
                    if (Owner.Room is Room room)
                    {
                        cd = SharedPhysics.HorizontalCollision(room, cd);
                        cd = SharedPhysics.VerticalCollision(room, cd);
                        cd = SharedPhysics.SlopesVertically(room, cd);
                    }
                    r.Pos = cd.pos;
                    r.Vel = cd.vel;
                    if (cd.contactPoint.x != 0)
                        r.Vel.y *= .6f;
                    if (cd.contactPoint.y != 0)
                        r.Vel.x *= .6f;
                }
            }
            for (i = 0; i < parts.Length; i++)
            {
                ref var r = ref parts[i];
                if (i > 0)
                {
                    ref var r2 = ref parts[i - 1];
                    var segDir = (r.Pos - r2.Pos).normalized;
                    var segLgt = Vector2.Distance(r.Pos, r2.Pos);
                    var distAff = segLgt > CONNEC_RAD ? .5f : .25f;
                    var a = segDir * (CONNEC_RAD - segLgt) * distAff;
                    r.Pos += a;
                    r.Vel += a;
                    r2.Pos -= a;
                    r2.Vel -= a;
                    if (i > 1)
                    {
                        r2 = ref parts[i - 2];
                        segDir = (r.Pos - r2.Pos).normalized;
                        r.Vel += segDir * .2f;
                        r2.Vel -= segDir * .2f;
                    }
                    if (i < parts.Length - 1)
                    {
                        ref var r3 = ref parts[i + 1];
                        r.Rot = Vector3.Slerp(r.Rot, (r2.Rot * 2f + r3.Rot) / 3f, .1f);
                        r.RotVel = Vector3.Slerp(r.RotVel, (r2.RotVel * 2f + r3.RotVel) / 3f, Custom.LerpMap(Vector2.Distance(r.LastPos, r.Pos), 1f, 8f, .05f, .5f));
                    }
                }
                else
                {
                    r.Pos = AttachPos;
                    r.Vel = default;
                }
            }
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var s = sLeaser.sprites[StartSprite] = TriangleMesh.MakeLongMesh(Parts.Length, false, false);
            s.shader = Custom.rainWorld.Shaders["JaggedSquare"];
            s.alpha = rCam.game.SeededRandom(Owner.ColorSeed);
            LastLayer = -1;
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 maskPos, Vector2 maskAnchor)
        {
            var mesh = (sLeaser.sprites[StartSprite] as TriangleMesh)!;
            mesh.color = Owner.RagColor;
            var pastRt = 0f;
            var attachPos = AttachPos = Owner.RagAttachPos(Index, maskPos, maskAnchor);
            var parts = Parts;
            for (var i = 0; i < parts.Length; i++)
            {
                ref var r = ref parts[i];
                var t = (float)i / (parts.Length - 1);
                var segPos = Vector2.Lerp(r.LastPos, r.Pos, timeStacker);
                var rt = (2f + 2f * Mathf.Sin(Mathf.Pow(t, 2f) * Mathf.PI)) * Vector3.Slerp(r.LastRot, r.Rot, timeStacker).x;
                var affDir = (attachPos - segPos).normalized;
                var perp = Custom.PerpendicularVector(affDir);
                var affDist = Vector2.Distance(attachPos, segPos) * .2f;
                mesh.MoveVertice(i * 4, attachPos - affDir * affDist - perp * (rt + pastRt) * .5f - camPos);
                mesh.MoveVertice(i * 4 + 1, attachPos - affDir * affDist + perp * (rt + pastRt) * .5f - camPos);
                mesh.MoveVertice(i * 4 + 2, segPos + affDir * affDist - perp * rt - camPos);
                mesh.MoveVertice(i * 4 + 3, segPos + affDir * affDist + perp * rt - camPos);
                attachPos = segPos;
                pastRt = rt;
            }
            if (LastLayer != Layer)
            {
                LastLayer = Layer;
                if (Layer == 0)
                {
                    mesh.RemoveFromContainer();
                    rCam.ReturnFContainer("Midground").AddChild(mesh);
                    mesh.MoveToBack();
                }
                else
                {
                    mesh.RemoveFromContainer();
                    rCam.ReturnFContainer("Items").AddChild(mesh);
                    mesh.MoveToFront();
                }
            }
        }
    }

    public string TrueSpriteOverride, Mode;
    public Rag? Rag1, Rag2;
    public Room? Room;
    public int ColorSeed;
    public Color RagColor = new(1f, .05f, .04f);

    public M4RScavMaskGraphics(PhysicalObject attached, VultureMask.AbstractVultureMask abstractMask, int firstSprite) : base(attached, abstractMask, firstSprite)
    {
        maskType = VultureMask.MaskType.NORMAL;
        overrideSprite ??= string.Empty;
        ColorSeed = abstractMask.colorSeed;
        var ar = overrideSprite.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
        if (ar.Length >= 2)
        {
            TrueSpriteOverride = ar[0];
            var md = ar[1];
            var state = Random.state;
            Random.InitState(ColorSeed);
            if (md == "A")
            {
                Mode = "B";
                Rag1 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 1);
            }
            else if (md == "B")
            {
                Mode = "A";
                Rag2 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 2);
            }
            else
            {
                Mode = "C";
                Rag1 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 1);
                Rag2 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 5, 2);
            }
            Random.state = state;
        }
        else
        {
            Mode = "C";
            TrueSpriteOverride = overrideSprite;
            Rag1 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 1);
            Rag2 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 5, 2);
        }
    }

    public M4RScavMaskGraphics(PhysicalObject attached, int firstSprite, string overrideSprite) : base(attached, VultureMask.MaskType.NORMAL, firstSprite, overrideSprite)
    {
        overrideSprite ??= string.Empty;
        this.overrideSprite ??= string.Empty;
        if (attached is not null)
            ColorSeed = attached.abstractPhysicalObject.ID.RandomSeed;
        var ar = overrideSprite.Split(['_'], StringSplitOptions.RemoveEmptyEntries);
        if (ar.Length >= 2)
        {
            TrueSpriteOverride = ar[0];
            var md = ar[1];
            var state = Random.state;
            Random.InitState(ColorSeed);
            if (md == "A")
            {
                Mode = "B";
                Rag1 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 1);
            }
            else if (md == "B")
            {
                Mode = "A";
                Rag2 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 2);
            }
            else
            {
                Mode = "C";
                Rag1 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 1);
                Rag2 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 5, 2);
            }
            Random.state = state;
        }
        else
        {
            Mode = "C";
            Rag1 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 4, 1);
            Rag2 = new(this, Random.Range(4, Random.Range(4, 10)), firstSprite + 5, 2);
            TrueSpriteOverride = overrideSprite;
        }
    }

    public virtual void Reset()
    {
        Rag1?.Reset();
        Rag2?.Reset();
    }

    public virtual Vector2 RagOffset(int spriteIndex, int index, bool invert, Vector2 anchor)
    {
        if (invert)
        {
            return spriteIndex switch
            {
                0 => index == 1 ? new(42.5f, 61f - 12.5f) : new(16.5f, 61f - 5.5f),
                1 => index == 1 ? new(43.5f, 61f - 17.5f) : new(20.5f, 61f - 7.5f),
                2 => index == 1 ? new(35.5f, 61f - 15.5f) : new(16.5f, 61f - 10.5f),
                3 => index == 1 ? new(44.5f, 61f - 12.5f) : new(17.5f, 61f - 8.5f),
                4 => index == 1 ? new(32.5f, 61f - 11.5f) : new(16.5f, 61f - 10.5f),
                5 => index == 1 ? new(21.5f, 61f - 14.5f) : new(9.5f, 61f - 21.5f),
                6 => index == 1 ? new(20.5f, 61f - 17.5f) : new(8.5f, 61f - 24.5f),
                7 => index == 1 ? new(27.5f, 61f - 11.5f) : new(15.5f, 61f - 4.5f),
                _ => index == 1 ? new(43.5f, 61f - 16.5f) : new(19.5f, 61f - 11.5f)
            } - anchor * 61f;
        }
        return spriteIndex switch
        {
            0 => index == 1 ? new(42.5f, 61f - 12.5f) : new(16.5f, 61f - 5.5f),
            1 => index == 1 ? new(36.5f, 61f - 18.5f) : new(18.5f, 61f - 9.5f),
            2 => index == 1 ? new(40.5f, 61f - 18.5f) : new(25.5f, 61f - 9.5f),
            3 => index == 1 ? new(34.5f, 61f - 13.5f) : new(22.5f, 61f - 6.5f),
            4 => index == 1 ? new(35.5f, 61f - 17.5f) : new(35.5f, 61f - 9.5f),
            5 => index == 1 ? new(41.5f, 61f - 21.5f) : new(50.5f, 61f - 17.5f),
            6 => index == 1 ? new(42.5f, 61f - 22.5f) : new(51.5f, 61f - 23.5f),
            7 => index == 1 ? new(39.5f, 61f - 13.5f) : new(38.5f, 61f - 4.5f),
            _ => index == 1 ? new(43.5f, 61f - 16.5f) : new(19.5f, 61f - 11.5f)
        } - anchor * 61f;
    }

    public virtual Vector2 RagAttachPos(int index) => RagAttachPos(index, attachedTo?.firstChunk.pos ?? default, new(.5f, .5f));

    public virtual Vector2 RagAttachPos(int index, Vector2 maskPos, Vector2 maskAnchor)
    {
        var rtB = rotationB;
        if (overrideAnchorVector.HasValue)
            rtB = overrideAnchorVector.Value;
        var rtA = rotationA;
        if (overrideRotationVector.HasValue)
            rtA = overrideRotationVector.Value;
        var sprIndex = SpriteIndex;
        var sgnB = Math.Sign(rtB.x);
        if (sgnB == 0 || sprIndex is 0 or 8)
            sgnB = 1;
        var offset = RagOffset(sprIndex, index, sgnB == -1, maskAnchor);
        var f = -Mathf.PI / 180f * Custom.VecToDeg(rtA);
        return maskPos + new Vector2(offset.x * Mathf.Cos(f) - offset.y * Mathf.Sin(f), offset.x * Mathf.Sin(f) + offset.y * Mathf.Cos(f));
    }
}