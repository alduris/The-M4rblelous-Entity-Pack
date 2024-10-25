using RWCustom;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class FatFireFlyGraphics : VultureGraphics
{
    public Fire[][]? Fires;

    public FatFireFlyGraphics(FatFireFly ow) : base(ow)
    {
        albino = false;
        feathersPerWing /= 2;
        var vgs = wings;
        var w2 = wings = new VultureFeather[2, feathersPerWing];
        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < feathersPerWing; j++)
                w2[i, j] = vgs[i, j * 2];
        }
        if (ow.room is Room rm)
        {
            var altForm = ow.abstractCreature.superSizeMe;
            var fires = Fires = new Fire[w2.GetLength(0)][];
            for (var i = 0; i < fires.Length; i++)
            {
                var fi = fires[i] = new Fire[w2.GetLength(1)];
                for (var j = 0; j < fi.Length; j++)
                    fi[j] = new(rm, w2[i, j], altForm);
            }
        }
        var state = Random.state;
        Random.InitState(ow.abstractCreature.ID.RandomSeed);
        if (ow.abstractCreature.superSizeMe)
        {
            ColorB = new(Mathf.Lerp(.5278f, .5972f, Random.value), Mathf.Lerp(.65f, .7f, 1f - Random.value * Random.value), Mathf.Lerp(.35f, .4f, Random.value * Random.value));
            ColorA = new(ColorB.hue + Mathf.Lerp(-.03f, .03f, Random.value), Mathf.Lerp(.7f, .8f, Random.value), Mathf.Lerp(.4f, .5f, Random.value));
        }
        else
        {
            ColorB = new(Mathf.Lerp(0f, .1f, Random.value), Mathf.Lerp(.95f, 1f, 1f - Random.value * Random.value), Mathf.Lerp(.45f, .5f, Random.value * Random.value));
            ColorA = new(ColorB.hue + Mathf.Lerp(-.03f, .03f, Random.value), Mathf.Lerp(.9f, .95f, Random.value), Mathf.Lerp(.5f, .55f, Random.value));
        }
        Random.state = state;
    }

    public override void Update()
    {
        base.Update();
        if (owner is Vulture v && v.room is Room rm)
        {
            if (!rm.BeingViewed && Fires is Fire[][] fs)
            {
                for (var i = 0; i < fs.Length; i++)
                {
                    var fi = fs[i];
                    for (var j = 0; j < fi.Length; j++)
                        fi[j].Destroy();
                }
            }
            var ws = wings;
            if (Fires?.Length is null or 0)
            {
                var altForm = v.abstractCreature.superSizeMe;
                var fires = Fires = new Fire[ws.GetLength(0)][];
                for (var i = 0; i < fires.Length; i++)
                {
                    var fi = fires[i] = new Fire[ws.GetLength(1)];
                    for (var j = 0; j < fi.Length; j++)
                    {
                        var wi = ws[i, j];
                        wi.pos = Vector2.Lerp(wi.pos, wi.ConnectedPos, .75f);
                        fi[j] = new(rm, wi, altForm);
                    }
                }
            }
            else
            {
                for (var i = 0; i < ws.GetLength(0); i++)
                {
                    for (var j = 0; j < ws.GetLength(1); j++)
                    {
                        var w = ws[i, j];
                        var f = Fires[i][j];
                        w.pos = Vector2.Lerp(w.pos, w.ConnectedPos, .75f);
                        f.Feather = w;
                        f.Update(v.evenUpdate);
                    }
                }
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprites = sLeaser.sprites;
        FSprite fs0 = sprites[FrontShieldSprite(0)], bs0 = sprites[BackShieldSprite(0)], bs1 = sprites[BackShieldSprite(1)], body = sprites[BodySprite];
        bs1.anchorX = bs0.anchorX = fs0.anchorX = body.anchorX;
        bs1.anchorY = bs0.anchorY = fs0.anchorY = body.anchorY;
        body.element = Futile.atlasManager.GetElementWithName("FFFBody");
        fs0.element = Futile.atlasManager.GetElementWithName("FFFBodyColor");
        bs0.element = Futile.atlasManager.GetElementWithName("Futile_White");
        bs0.shader = Custom.rainWorld.Shaders["HeatDistortion"];
        bs1.element = Futile.atlasManager.GetElementWithName("Futile_White");
        bs1.shader = Custom.rainWorld.Shaders["HeatDistortion"];
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        var sprites = sLeaser.sprites;
        for (var j = 0; j < 2; j++)
        {
            for (var m = 0; m < feathersPerWing; m++)
                sprites[FeatherColorSprite(j, m)].isVisible = false;
        }
        FSprite fs0 = sprites[FrontShieldSprite(0)], bs0 = sprites[BackShieldSprite(0)], bs1 = sprites[BackShieldSprite(1)], body = sprites[BodySprite], eye = sprites[EyesSprite];
        var altForm = vulture?.abstractCreature.superSizeMe is true;
        if (!spritesInShadowMode)
            eye.color = Color.Lerp(altForm ? new(.92f, .92f, .95f) : new(.75f, .15f, 0f), eye.color, altForm ? .25f : .5f);
        eye.element = Futile.atlasManager.GetElementWithName($"FFFEyes{headGraphic}");
        bs1.x = bs0.x = fs0.x = body.x;
        bs1.y = bs0.y = fs0.y = body.y;
        bs1.rotation = bs0.rotation = fs0.rotation = body.rotation;
        fs0.scale = body.scale;
        if (!spritesInShadowMode)
            fs0.color = Color.Lerp(ColorB.rgb, Color.Lerp(altForm ? Color.blue : Color.red, Color.white, .5f), .25f);
        bs1.scale = bs0.scale = 25f;
        bs1.color = bs0.color = Color.white;
        if (vulture is Vulture v && v.dead)
        {
            bs0.alpha = Mathf.Max(bs0.alpha - .1f, 0f);
            bs1.alpha = Mathf.Max(bs1.alpha - .1f, 0f);
        }
        else if (spritesInShadowMode)
            bs1.alpha = bs0.alpha = 0f;
        else
        {
            bs0.alpha = Mathf.Min(bs0.alpha + .1f, 1f);
            bs1.alpha = Mathf.Min(bs1.alpha + .1f, 1f);
        }
        sprites[FrontShieldSprite(1)].isVisible = false;
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        base.AddToContainer(sLeaser, rCam, newContatiner);
        var bs0 = sLeaser.sprites[BackShieldSprite(0)];
        bs0.RemoveFromContainer();
        rCam.ReturnFContainer("GrabShaders").AddChild(bs0);
    }

    public class Fire : UpdatableAndDeletable
    {
        public class FireSprite : CosmeticSprite
        {
            public float LifeTime;
            public float Life, LastLife;
            public bool AltForm;

            public FireSprite(Vector2 pos, bool altForm)
            {
                base.pos = pos;
                AltForm = altForm;
                lastPos = pos;
                vel = Custom.RNV() * 1.5f * Random.value;
                Life = 1f;
                LifeTime = Mathf.Lerp(10f, 40f, Random.value);
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                vel *= .8f;
                vel.y += .4f;
                vel += Custom.RNV() * Random.value * .5f;
                LastLife = Life;
                Life -= 1f / LifeTime;
                if (Life < 0f)
                    Destroy();
            }

            public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
            {
                var fSprite = sLeaser.sprites[0];
                fSprite.RemoveFromContainer();
                rCam.ReturnFContainer("Midground").AddChild(fSprite);
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = [new("deerEyeB")];
                AddToContainer(sLeaser, rCam, null);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                var s0 = sLeaser.sprites[0];
                s0.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
                var num = Mathf.Lerp(LastLife, Life, timeStacker);
                s0.scale = num;
                s0.color = Custom.HSL2RGB(AltForm ? Mathf.Lerp(194f / 360f, 250f / 360f, num) : Mathf.Lerp(.01f, .08f, num), 1f, Mathf.Lerp(.5f, 1f, Mathf.Pow(num, 3f)));
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpecialLightSource
        {
            public LightSource? LightSource;
            public Vector2 GetToPos;
            public float GetToRad;
        }

        public SpecialLightSource[] LightSources = new SpecialLightSource[3];
        public LightSource? FlatLightSource;
        public VultureFeather Feather;
        public bool AltForm;

        public Fire(Room room, VultureFeather feather, bool altForm)
        {
            this.room = room;
            Feather = feather;
            AltForm = altForm;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (Feather is not VultureFeather f || f.wing is not VultureTentacle t || t.room is not Room rm)
                return;
            var fl = t.mode == VultureTentacle.Mode.Fly && IsPositionInsideBoundries(f.pos, rm) && t.vulture is Vulture v && !v.dead;
            var lhs = LightSources;
            var altForm = AltForm;
            for (var i = 0; i < lhs.Length; i++)
            {
                ref var slh = ref lhs[i];
                if (Random.value < .2f)
                    slh.GetToPos = Custom.RNV() * 50f * Random.value;
                if (Random.value < .2f)
                    slh.GetToRad = Mathf.Lerp(50f, Mathf.Lerp(400f, 200f, i / 2f), Mathf.Pow(Random.value, .5f));
                if (slh.LightSource is LightSource li)
                {
                    li.stayAlive = true;
                    li.setPos = Vector2.Lerp(li.Pos, f.pos + slh.GetToPos, .2f);
                    li.setRad = Mathf.Lerp(li.Rad, slh.GetToRad, .2f);
                    li.setAlpha = fl ? Mathf.Min(li.Alpha + .05f, .2f) : Mathf.Max(li.Alpha - .05f, 0f);
                    if (li.slatedForDeletetion)
                        slh.LightSource = null;
                }
                else
                {
                    rm.AddObject(slh.LightSource = new(f.pos, false, Custom.HSL2RGB(altForm ? Mathf.Lerp(194f / 360f, 250f / 360f, i / 2f) : Mathf.Lerp(.01f, .07f, i / 2f), 1f, .5f), this)
                    {
                        requireUpKeep = true,
                        setAlpha = fl ? .2f : 0f
                    });
                }
            }
            if (FlatLightSource is LightSource l)
            {
                l.stayAlive = true;
                l.setAlpha = fl ? Mathf.Min(l.Alpha + .05f, Mathf.Lerp(.1f, .2f, Random.value)) : Mathf.Max(l.Alpha - .05f, 0f);
                l.setRad = Mathf.Lerp(24f, 33f, Random.value);
                l.setPos = f.pos;
                if (l.slatedForDeletetion)
                    FlatLightSource = null;
            }
            else
                rm.AddObject(FlatLightSource = new(f.pos, false, altForm ? new(.41803923f, .3f, 1f) : new(1f, .5909804f, .3f), this)
                {
                    flat = true,
                    requireUpKeep = true,
                    setAlpha = fl ? Mathf.Lerp(.1f, .2f, Random.value) : 0f
                });
            if (fl)
            {
                rm.AddObject(new FireSprite(f.pos, altForm));
                rm.PlaySound(SoundID.Firecracker_Burn, f.pos, .14f, 1.5f);
            }
        }

        public static bool IsPositionInsideBoundries(Vector2 pos, Room rm) => pos.x >= -50 && pos.x <= rm.PixelWidth + 50 && pos.y >= -50 && pos.y <= rm.PixelHeight + 50;
    }
}