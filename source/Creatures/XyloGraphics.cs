using UnityEngine;
using System;
using Random = UnityEngine.Random;
using RWCustom;

namespace LBMergedMods.Creatures;

public class XyloGraphics : GraphicsModule
{
    public const int MAIN_ROOT_SPRITE = 0, MAIN_ROOT_DANGLER_SPRITE = 1, BODY_SPRITE = 2, GRADIENT_SPRITE = 3, LIGHT_SPRITE = 4, HOLES_SPRITE = 5;
    public int DanglerVar;
    public float Rotation, MainRootRotation, LightUp;
    public bool InvertX, InvertY, InvertDangler;

    public virtual Xylo Creature => (owner as Xylo)!;

    public XyloGraphics(Xylo ow) : base(ow, false)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        Rotation = Random.value * 360f;
        MainRootRotation = Random.value * 360f;
        InvertX = Random.value > .5f;
        InvertY = Random.value > .5f;
        InvertDangler = Random.value > .5f;
        DanglerVar = Random.Range(1, 4);
        Random.state = state;
    }

    public override void Update()
    {
        base.Update();
        if (LightUp > 0f)
            LightUp = Math.Max(0f, LightUp - .05f);
    }

    public override void Reset() => LightUp = 0f;

    public override void PushOutOf(Vector2 pos, float rad) { }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprites = sLeaser.sprites = new FSprite[HOLES_SPRITE + 1];
        sprites[MAIN_ROOT_SPRITE] = new("XyloDarkness")
        {
            scale = Xylo.BASE_RAD / 94f,
            rotation = MainRootRotation,
            shader = Custom.rainWorld.Shaders["BlackGoo"]
        };
        sprites[MAIN_ROOT_DANGLER_SPRITE] = new("XyloRootDangler" + DanglerVar)
        {
            anchorY = .85f,
            anchorX = .55f
        };
        sprites[BODY_SPRITE] = new("XyloBody")
        {
            rotation = Rotation,
            scaleX = InvertX ? -1f : 1f,
            scaleY = InvertY ? -1f : 1f
        };
        sprites[GRADIENT_SPRITE] = new("XyloGrad")
        {
            rotation = Rotation,
            scaleX = InvertX ? -1f : 1f,
            scaleY = InvertY ? -1f : 1f,
            alpha = .15f
        };
        sprites[LIGHT_SPRITE] = new("XyloLight")
        {
            rotation = Rotation,
            scaleX = InvertX ? -1f : 1f,
            scaleY = InvertY ? -1f : 1f
        };
        sprites[HOLES_SPRITE] = new("XyloHoles")
        {
            rotation = Rotation,
            scaleX = InvertX ? -1f : 1f,
            scaleY = InvertY ? -1f : 1f
        };
        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        var sprs = sLeaser.sprites;
        var spr = sprs[MAIN_ROOT_SPRITE];
        spr.RemoveFromContainer();
        rCam.ReturnFContainer("Shortcuts").AddChild(spr);
        for (var i = 1; i < sprs.Length; i++)
        {
            spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprs = sLeaser.sprites;
        FSprite body = sprs[BODY_SPRITE],
            root = sprs[MAIN_ROOT_SPRITE],
            holes = sprs[HOLES_SPRITE],
            grad = sprs[GRADIENT_SPRITE],
            dangler = sprs[MAIN_ROOT_DANGLER_SPRITE],
            light = sprs[LIGHT_SPRITE];
        if (Creature is Xylo ow)
        {
            var fc = ow.firstChunk;
            var pos = Vector2.Lerp(fc.lastPos, fc.pos, timeStacker);
            var tweakedPos = pos - camPos;
            dangler.SetPosition(tweakedPos);
            var rd = fc.rad / 110f + .025f;
            dangler.scaleX = rd * (InvertDangler ? -1f : 1f);
            root.SetPosition(tweakedPos);
            holes.SetPosition(tweakedPos);
            body.SetPosition(tweakedPos);
            grad.SetPosition(tweakedPos);
            light.SetPosition(tweakedPos);
            holes.color = Color.Lerp(rCam.currentPalette.blackColor, ow.EffectColor, .25f + LightUp);
            dangler.scaleY = light.scale = grad.scale = body.scale = holes.scale = rd;
            light.alpha = LightUp * .585f;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (Creature?.NoHolesMode is true)
        {
            root.isVisible = false;
            holes.isVisible = false;
            grad.isVisible = false;
            light.isVisible = false;
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var sprs = sLeaser.sprites;
        sprs[BODY_SPRITE].color = palette.blackColor;
        if (Creature is Xylo ow)
        {
            if (ow.NoHolesMode)
                sprs[MAIN_ROOT_DANGLER_SPRITE].color = palette.blackColor;
            else
            {
                sprs[LIGHT_SPRITE].color = sprs[GRADIENT_SPRITE].color = ow.EffectColor;
                sprs[MAIN_ROOT_DANGLER_SPRITE].color = Color.Lerp(palette.blackColor, ow.EffectColor, .025f);
            }
        }
    }
}