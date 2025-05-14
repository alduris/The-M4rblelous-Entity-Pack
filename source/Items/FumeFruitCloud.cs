using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Items;

public class FumeFruitCloud : CosmeticSprite
{
    public class CloudVisionObscurer(Vector2 pos, int rippleLayer) : VisionObscurer(pos, 70f, 140f, 1f, rippleLayer)
    {
        public float Progress;

        public override void Update(bool eu)
        {
            base.Update(eu);
            Progress += 1f / 130f;
            obscureFac = Mathf.InverseLerp(1f, .3f, Progress);
            rad = Mathf.Lerp(70f, 140f, Mathf.Pow(Progress, .5f));
            var crits = room.abstractRoom.creatures;
            if (crits.Count > 0)
            {
                var crit = crits[Random.Range(0, crits.Count)];
                if (crit.realizedCreature is Creature cr && (crit.rippleLayer == rippleLayer || crit.rippleBothSides) && Custom.DistLess(cr.mainBodyChunk.pos, pos, rad))
                    cr.Blind((int)(Progress * crits.Count * 10f));
            }
            if (Progress > 1f)
                Destroy();
        }
    }

    public AbstractCreature? KillTag;
    public InsectCoordinator? SmallInsects;
    public Vector2 GetToPos;
    public float Life, LastLife, LifeTime, Rotation, LastRotation, RotVel, Rad;
    public int CheckInsectsDelay, RippleLayer;

    public FumeFruitCloud(Vector2 pos, Vector2 vel, float size, AbstractCreature? killTag, int checkInsectsDelay, InsectCoordinator? smallInsects, int rippleLayer)
    {
        CheckInsectsDelay = checkInsectsDelay;
        SmallInsects = smallInsects;
        RippleLayer = rippleLayer;
        LastLife = Life = size * 1.5f;
        lastPos = pos;
        this.vel = vel;
        KillTag = killTag;
        GetToPos = pos + new Vector2(Mathf.Lerp(-50f, 50f, Random.value), Mathf.Lerp(-100f, 400f, Random.value));
        this.pos = pos + vel.normalized * 60f * Random.value;
        Rad = Mathf.Lerp(.6f, 1.5f, Random.value) * size;
        Rotation = Random.value * 360f;
        LastRotation = Rotation;
        RotVel = Mathf.Lerp(-6f, 6f, Random.value);
        LifeTime = Mathf.Lerp(170f, 400f, Random.value);
    }

    public override void Update(bool eu)
    {
        vel *= .9f;
        vel += Custom.DirVec(pos, GetToPos) * Random.value * .04f;
        LastRotation = Rotation;
        Rotation += RotVel * vel.magnitude;
        LastLife = Life;
        Life -= 1f / LifeTime;
        if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
        {
            var intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
            if (intVector.HasValue)
            {
                var floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
                pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
                var cornerB = floatRect.GetCorner(FloatRect.CornerLabel.B);
                if (cornerB.x < 0f)
                    vel.x = Math.Abs(vel.x);
                else if (cornerB.x > 0f)
                    vel.x = -Math.Abs(vel.x);
                else if (cornerB.y < 0f)
                    vel.y = Math.Abs(vel.y);
                else if (cornerB.y > 0f)
                    vel.y = -Math.Abs(vel.y);
            }
        }
        if (LastLife <= 0f)
            Destroy();
        if (CheckInsectsDelay >= 0)
        {
            --CheckInsectsDelay;
            if (CheckInsectsDelay < 1)
            {
                CheckInsectsDelay = 20;
                var crits = room.abstractRoom.creatures;
                for (var i = 0; i < crits.Count; i++)
                {
                    if (crits[i] is not AbstractCreature acr || acr.realizedCreature is not Creature crit || crit.Submersion > .3f || (acr.rippleLayer != RippleLayer && !acr.rippleBothSides))
                        continue;
                    var chs = crit.bodyChunks;
                    var smallies = crit.Template.type == CreatureTemplate.Type.Fly || crit.Template.type == CreatureTemplate.Type.Spider;
                    for (var j = 0; j < chs.Length; j++)
                    {
                        var ch = chs[j];
                        if (Custom.DistLess(pos, ch.pos, Rad + ch.rad + 20f))
                        {
                            if (crit is Player player)
                            {
                                player.exhausted = true;
                                player.lungsExhausted = true;
                                player.airInLungs = Math.Max(.1f, player.airInLungs);
                            }
                            if (!smallies)
                            {
                                if (crit is InsectoidCreature insect && insect is not Sporantula)
                                {
                                    insect.poison += .02f * Mathf.Pow(Life, 4f) / Mathf.Lerp(crit.TotalMass, 1f, .15f);
                                    if (insect.poison >= 1f)
                                    {
                                        if (insect.State is HealthState hs)
                                            hs.health -= .01f * Mathf.Pow(Life, 4f) / Mathf.Lerp(crit.TotalMass, 1f, .15f);
                                        else
                                            crit.Die();
                                    }
                                    crit.SetKillTag(KillTag);
                                    if (Random.value < .35f)
                                        crit.Stun(Mathf.RoundToInt(25f * Random.value * Life / Mathf.Lerp(crit.TotalMass, 1f, .15f)));
                                }
                                else if (Random.value < (crit is Player ? .15f : .2f))
                                    crit.Stun(Mathf.RoundToInt(25f * Random.value * Life / Mathf.Lerp(crit.TotalMass, 1f, .15f)));
                            }
                        }
                    }
                    if (smallies)
                    {
                        var mbc = crit.mainBodyChunk;
                        if (Custom.DistLess(pos, mbc.pos, Rad + mbc.rad + 20f))
                        {
                            if (Random.value < Life)
                                crit.Die();
                            else
                                crit.Stun(Random.Range(10, 120));
                        }
                    }
                }
                var uads = room.updateList;
                for (var i = 0; i < uads.Count; i++)
                {
                    var uad = uads[i];
                    if (uad is LocustSystem sys)
                    {
                        sys.KillInRadius(pos, Rad + 70f);
                        sys.AddAvoidancePoint(pos, Rad + 200f, 20);
                    }
                    else if (uad is SporePlant sp && (sp.abstractPhysicalObject.rippleLayer == RippleLayer || sp.abstractPhysicalObject.rippleBothSides))
                        sp.PuffBallSpores(pos, Rad);
                    else if (uad is SporePlant.AttachedBee bee && (bee.abstractPhysicalObject.rippleLayer == RippleLayer || bee.abstractPhysicalObject.rippleBothSides) && Custom.DistLess(pos, bee.firstChunk.pos, Rad + 20f))
                        bee.life -= .5f;
                }
                if (SmallInsects?.allInsects is List<CosmeticInsect> insects)
                {
                    for (var n = 0; n < insects.Count; n++)
                    {
                        var insect = insects[n];
                        if (Custom.DistLess(insect.pos, pos, Rad + 70f) && !room.PointSubmerged(insect.pos))
                            insect.alive = false;
                    }
                }
            }
        }
        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("Futile_White")
        {
            shader = Custom.rainWorld.Shaders["FumeFruitHaze"]
        }];
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var ps = Vector2.Lerp(lastPos, pos, timeStacker);
        var s0 = sLeaser.sprites[0];
        s0.SetPosition(ps - camPos);
        s0.rotation = Mathf.Lerp(LastRotation, Rotation, timeStacker);
        var num = Mathf.Lerp(LastLife, Life, timeStacker);
        s0.scale = 7f * Rad * (num > .5f ? Custom.LerpMap(num, 1f, .5f, .5f, 1f) : Mathf.Sin(num * Mathf.PI));
        s0.alpha = Mathf.Pow(Math.Max(0f, Mathf.Lerp(LastLife, Life, timeStacker)), 1.2f);
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
}