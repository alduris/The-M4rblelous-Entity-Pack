/*using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class XyloGraphics : GraphicsModule
{
    public const int ROOT = 0, BODY = 1, HOLES = 2;
    public float Rotation, RootRotation, LightUp;
    public bool InvertX, InvertY;

    public virtual Xylo Creature => (owner as Xylo)!;

    public XyloGraphics(Xylo ow) : base(ow, false)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        Rotation = Random.value * 360f;
        RootRotation = Random.value * 360f;
        InvertX = Random.value > .5f;
        InvertY = Random.value > .5f;
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
        sLeaser.sprites =
        [
            new("corruption")
            {
                scale = Xylo.BASE_RAD / 50f,
                shader = Custom.rainWorld.Shaders["BlackGoo"],
                rotation = RootRotation
            },
            new("XyloBody")
            {
                rotation = Rotation,
                scaleX = InvertX ? -1f : 1f,
                scaleY = InvertY ? -1f : 1f
            },
            new("XyloHoles")
            {
                rotation = Rotation,
                scaleX = InvertX ? -1f : 1f,
                scaleY = InvertY ? -1f : 1f
            }
        ];
        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Shortcuts");
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
        {
            var spr = sprs[i];
            spr.RemoveFromContainer();
            newContainer.AddChild(spr);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var sprs = sLeaser.sprites;
        FSprite body = sprs[BODY],
            root = sprs[ROOT],
            holes = sprs[HOLES];
        if (Creature is Xylo ow)
        {
            var fc = ow.firstChunk;
            var pos = Vector2.Lerp(fc.lastPos, fc.pos, timeStacker);
            var tweakedPos = pos - camPos;
            root.SetPosition(tweakedPos);
            holes.SetPosition(tweakedPos);
            body.SetPosition(tweakedPos);
            holes.color = Color.Lerp(rCam.currentPalette.blackColor, ow.EffectColor, .25f + LightUp);
            body.scale = holes.scale = fc.rad / 110f + .025f;
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) => sLeaser.sprites[BODY].color = palette.blackColor;
}*/