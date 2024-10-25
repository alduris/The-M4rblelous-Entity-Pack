using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class JellyGraphics(PhysicalObject ow) : DaddyGraphics(ow), DaddyGraphics.DaddyBubbleOwner
{
    public float Consious, Lerper, LastRot, Rot, LastAlpha, Alpha;
    public bool LerpUp = true;

    public override void Update()
    {
        base.Update();
        if (daddy is DaddyLongLegs d)
        {
            Consious = Mathf.Clamp01(Consious + (!d.Consious ? .02f : -.0075f));
            Lerper = Mathf.Clamp(Lerper + (LerpUp ? .0075f : -.0075f), -.75f, 1f);
            if (Lerper == 1f)
                LerpUp = false;
            else if (Lerper == -.75f)
                LerpUp = true;
            LastAlpha = Alpha;
            Alpha = Mathf.Lerp(1f, 0f, Mathf.Max(Consious, Lerper, digesting / 2f));
            LastRot = Rot;
            Rot += d.Consious is true ? Mathf.Max(Lerper / 3.8f + .35f, digesting * 32f) * 1.0001f : 0f;
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        if (daddy is DaddyLongLegs d)
        {
            var sprites = sLeaser.sprites;
            sprites[BodySprite(0)].isVisible = false;
            sprites[BodySprite(0)].RemoveFromContainer();
            sprites[EyeSprite(0, 0)].isVisible = false;
            sprites[EyeSprite(0, 0)].RemoveFromContainer();
            sprites[EyeSprite(0, 1)].isVisible = false;
            sprites[EyeSprite(0, 1)].RemoveFromContainer();
            sprites[BodySprite(0)] = new("JellyLLGraf")
            {
                scale = (d.bodyChunks[0].rad * 1.1f + 2f) / 64f,
                shader = Custom.rainWorld.Shaders["Basic"],
                alpha = 1f
            };
            sprites[BodySprite(0) + 1] = new("JellyLLGrad")
            {
                scale = (d.bodyChunks[0].rad * 1.1f + 2f) / 64f,
                shader = Custom.rainWorld.Shaders["Basic"],
                alpha = 1f
            };
        }
        AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && daddy is DaddyLongLegs d)
        {
            var vector2 = Vector2.Lerp(d.bodyChunks[0].lastPos, d.bodyChunks[0].pos, timeStacker) + Custom.RNV() * digesting * 4f * Random.value;
            var sRot = Mathf.Lerp(LastRot, Rot, timeStacker);
            var sAlpha = Mathf.Lerp(LastAlpha, Alpha, timeStacker);
            sLeaser.sprites[BodySprite(0)].rotation = sRot;
            var s = sLeaser.sprites[BodySprite(0) + 1];
            s.x = vector2.x - camPos.x;
            s.y = vector2.y - camPos.y;
            s.rotation = sRot;
            s.color = d.eyeColor;
            s.alpha = sAlpha;
        }
    }

    public virtual Color GetColor() => daddy.effectColor;

    public virtual Vector2 GetPosition() => daddy.firstChunk.pos;
}