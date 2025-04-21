using RWCustom;
using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class WaterBlobGraphics : GraphicsModule
{
    public Color Color, DarkColor, BlackColor;
    public Vector2 LookPoint, EyePos;
    public float EyeSize, Shake, Darkness, LastDarkness, LastProp, Prop, PropSpeed;

    public virtual WaterBlob? Blob => owner as WaterBlob;

    public virtual float BodySize => Blob is WaterBlob b ? b.firstChunk.rad / 10f : 0f;

    public WaterBlobGraphics(WaterBlob ow) : base(ow, true)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        EyeSize = Random.Range(.6f, 1.4f);
        Random.state = state;
        internalContainerObjects = [];
    }

    public override void Update()
    {
        base.Update();
        if (Blob is WaterBlob b)
        {
            if (b.EatObject is not null)
                Shake = Mathf.Lerp(Shake, .5f, .1f);
            else
                Shake = Mathf.Lerp(Shake, 0f, .2f);
            LookPoint = b.firstChunk.pos;
            if (b.EatObject is PhysicalObject obj)
                LookPoint = obj.firstChunk.pos;
            else if (b.Consious && b.AI is WaterBlobAI AI)
            {
                if (AI.threatTracker.mostThreateningCreature?.representedCreature?.realizedCreature is Creature c)
                    LookPoint = c.firstChunk.pos;
                else if (AI.preyTracker.currentPrey is not null && AI.Prey?.realizedCreature is Creature cr)
                    LookPoint = cr.firstChunk.pos;
            }
            EyePos = Vector2.Lerp(EyePos, LookPoint != b.firstChunk.pos ? Custom.DirVec(b.firstChunk.pos, LookPoint) * 5f : default, .3f);
            LastProp = Prop;
            Prop += PropSpeed;
            PropSpeed *= .85f;
            PropSpeed -= Prop / 10f;
            Prop = Mathf.Clamp(Prop, -15f, 15f);
            if (b.grabbedBy?.Count == 0)
            {
                Prop += (b.firstChunk.lastPos.x - b.firstChunk.pos.x) / 15f;
                Prop -= (b.firstChunk.lastPos.y - b.firstChunk.pos.y) / 15f;
            }
        }
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites =
        [
            new("Futile_White")
            {
                shader = Custom.rainWorld.Shaders["WaterNut"],
                anchorX = .5f,
                anchorY = .5f,
                scale = .75f * EyeSize * BodySize
            },
            new("Futile_White")
            {
                shader = Custom.rainWorld.Shaders["WaterNut"],
                anchorX = .5f,
                anchorY = .5f,
                scale = .5f * BodySize
            }
        ];
        sLeaser.containers = [new()];
        AddToContainer(sLeaser, rCam, null);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");
        if (sLeaser.containers is FContainer[] c)
        {
            for (var i = 0; i < c.Length; i++)
            {
                var container = c[i];
                container.RemoveFromContainer();
                newContainer.AddChild(container);
            }
        }
        var spr = sLeaser.sprites[0];
        spr.RemoveFromContainer();
        newContainer.AddChild(spr);
        spr = sLeaser.sprites[1];
        spr.RemoveFromContainer();
        rCam.ReturnFContainer("GrabShaders").AddChild(spr);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (Blob is WaterBlob b && b.room is Room rm)
        {
            float sat = b.Saturated, bs = BodySize;
            var vector = Vector2.Lerp(b.firstChunk.lastPos, b.firstChunk.pos, timeStacker);
            FSprite s0 = sLeaser.sprites[0], s1 = sLeaser.sprites[1];
            LastDarkness = Darkness;
            Darkness = rm.Darkness(vector) * (1f - rm.LightSourceExposure(vector));
            if (Darkness != LastDarkness)
                ApplyPalette(sLeaser, rCam, rCam.currentPalette);
            s0.SetPosition(vector + EyePos * (1f - sat) * (bs * .5f) * Custom.LerpMap(EyeSize, .6f, 1.4f, 1.2f, .5f) - camPos);
            s1.SetPosition(vector - camPos + Custom.RNV() * Shake);
            s1.alpha = Mathf.Lerp((1f - Darkness * 0.25f) * (1f - b.firstChunk.submersion * .25f), 1f, sat);
            s1.scaleX = (1.5f + Mathf.Lerp(LastProp, Prop, timeStacker) / 20f) * bs;
            s1.scaleY = (1.5f - Mathf.Lerp(LastProp, Prop, timeStacker) / 20f) * bs;
            s1.color = Color.Lerp(Color, BlackColor, sat * .8f);
            s0.color = Color.Lerp(DarkColor, BlackColor, sat * .8f);
            s0.scale = Mathf.Lerp(.75f * EyeSize * bs, 1.25f * bs, sat);
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (Blob is WaterBlob b)
        {
            Color = Color.Lerp(palette.waterColor1, palette.waterColor2, b.WaterRatio);
            DarkColor = Color.Lerp(palette.blackColor, Color, .6f);
            BlackColor = palette.blackColor;
            sLeaser.sprites[0].color = DarkColor;
            sLeaser.sprites[1].color = Color;
        }
    }

    public virtual void Impact(IntVector2 direction, float speed)
    {
        if (direction.y != 0)
        {
            Prop += speed;
            PropSpeed += speed / 10f;
        }
        else
        {
            Prop -= speed;
            PropSpeed -= speed / 10f;
        }
    }
}