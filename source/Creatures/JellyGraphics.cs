using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
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
            var eye = eyes[0];
            var spr0 = sLeaser.sprites[eye.firstSprite];
            spr0.element = Futile.atlasManager.GetElementWithName("JellyLLGraf");
            spr0.scale = (d.firstChunk.rad * 1.1f + 2f) / 64f;
            spr0.alpha = 1f;
            var spr1 = sLeaser.sprites[eye.firstSprite + 1];
            spr1.isVisible = false;
            spr1.RemoveFromContainer();
            spr0.container.AddChild(sLeaser.sprites[eye.firstSprite + 1] = new("JellyLLGrad")
            {
                scale = (d.firstChunk.rad * 1.1f + 2f) / 64f,
                alpha = 1f,
                shader = spr0.shader = RainWorld.TryGetRippleMaskedShaderVariant(d.abstractPhysicalObject, "RippleBasic")
            });
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && daddy is DaddyLongLegs d)
        {
            var eye = eyes[0];
            var spr0 = sLeaser.sprites[eye.firstSprite];
            var spr1 = sLeaser.sprites[eye.firstSprite + 1];
            spr1.rotation = spr0.rotation = Mathf.Lerp(LastRot, Rot, timeStacker);
            spr1.SetPosition(spr0.GetPosition());
            spr1.color = d.eyeColor;
            spr1.alpha = Mathf.Lerp(LastAlpha, Alpha, timeStacker);
            spr1.isVisible = true;
            spr1.MoveInFrontOfOtherNode(spr0);
            spr1.scaleX = spr0.scaleX;
            spr1.scaleY = spr0.scaleY;
            sLeaser.sprites[eye.firstSprite + 2].isVisible = false;
        }
    }

    public virtual Color GetColor() => daddy.effectColor;

    public virtual Vector2 GetPosition() => daddy.firstChunk.pos;

    public virtual Color GetEyeColor() => EffectColor;
}