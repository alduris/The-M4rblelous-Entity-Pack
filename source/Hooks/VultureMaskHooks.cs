global using static LBMergedMods.Hooks.VultureMaskHooks;
using MoreSlugcats;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using System;

namespace LBMergedMods.Hooks;

public static class VultureMaskHooks
{
    internal static void On_VultureMask_ctor(On.VultureMask.orig_ctor orig, VultureMask self, AbstractPhysicalObject abstractPhysicalObject, World world)
    {
        orig(self, abstractPhysicalObject, world);
        var abs = self.AbstrMsk;
        if (abs.spriteOverride is string s && s.StartsWith("M4RScavMask"))
        {
            abs.king = false;
            abs.scavKing = false;
            self.maskGfx = new M4RScavMaskGraphics(self, abs, 0);
            self.maskGfx.GenerateColor(abs.colorSeed);
        }
    }

    internal static void On_VultureMask_PlaceInRoom(On.VultureMask.orig_PlaceInRoom orig, VultureMask self, Room placeRoom)
    {
        orig(self, placeRoom);
        if (self.maskGfx is M4RScavMaskGraphics g)
            g.Reset();
    }

    internal static void On_VultureMask_Update(On.VultureMask.orig_Update orig, VultureMask self, bool eu)
    {
        orig(self, eu);
        if (self.maskGfx is M4RScavMaskGraphics g)
            g.Room = self.room;
    }

    internal static bool On_VultureMaskGraphics_get_King(Func<VultureMaskGraphics, bool> orig, VultureMaskGraphics self) => self is not M4RScavMaskGraphics && orig(self);

    internal static int On_VultureMaskGraphics_get_BaseTotalSprites(Func<VultureMaskGraphics, int> orig, VultureMaskGraphics self)
    {
        if (self is M4RScavMaskGraphics g)
        {
            var num = 4;
            if (g.Rag1 is not null)
                ++num;
            if (g.Rag2 is not null)
                ++num;
            return num;
        }
        return orig(self);
    }

    internal static void On_VultureMaskGraphics_AddToContainer(On.MoreSlugcats.VultureMaskGraphics.orig_AddToContainer orig, VultureMaskGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        if (self is M4RScavMaskGraphics g)
        {
            var sprites = sLeaser.sprites;
            newContainer ??= rCam.ReturnFContainer("Items");
            var tot = g.firstSprite + g.BaseTotalSprites;
            for (var i = g.firstSprite; i < tot; i++)
                sprites[i].RemoveFromContainer();
            newContainer.AddChild(sprites[g.firstSprite + 2]);
            newContainer.AddChild(sprites[g.firstSprite + 1]);
            newContainer.AddChild(sprites[g.firstSprite]);
            for (var i = g.firstSprite + 3; i < tot; i++)
                newContainer.AddChild(sprites[i]);
        }
        else
            orig(self, sLeaser, rCam, newContainer);
    }

    internal static void On_VultureMaskGraphics_ApplyPalette(On.MoreSlugcats.VultureMaskGraphics.orig_ApplyPalette orig, VultureMaskGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self is M4RScavMaskGraphics g)
        {
            g.blackColor = palette.blackColor;
            g.RagColor = Color.Lerp(new(1f, .05f, .04f), palette.blackColor, .1f + .8f * palette.darkness);
        }
        else
            orig(self, sLeaser, rCam, palette);
    }

    internal static void On_VultureMaskGraphics_DrawSprites(On.MoreSlugcats.VultureMaskGraphics.orig_DrawSprites orig, VultureMaskGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (self is M4RScavMaskGraphics g)
        {
            var sprites = sLeaser.sprites;
            Vector2 pos = default,
                rot = Vector3.Slerp(g.lastRotationA, g.rotationA, timeStacker),
                zRot = Vector3.Slerp(g.lastRotationB, g.rotationB, timeStacker);
            if (g.overrideRotationVector.HasValue)
                rot = g.overrideRotationVector.Value;
            if (g.overrideAnchorVector.HasValue)
                zRot = g.overrideAnchorVector.Value;
            if (g.overrideDrawVector.HasValue)
                pos = g.overrideDrawVector.Value;
            else if (g.attachedTo is PhysicalObject obj)
                pos = Vector2.Lerp(obj.firstChunk.lastPos, obj.firstChunk.pos, timeStacker);
            var dark = g.ignoreDarkness ? 0f : rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos)) * .8f * (1f - g.fallOffVultureMode);
            var zRotDeg = Custom.VecToDeg(zRot);
            var sprite = Custom.IntClamp(Mathf.RoundToInt(Math.Abs(zRotDeg / 180f) * 8f), 0, 8);
            var degRot = Custom.VecToDeg(rot);
            var sgn = Math.Sign(zRotDeg);
            FSprite s;
            var tot = g.firstSprite + 3;
            var rt = Custom.LerpMap(Math.Abs(zRotDeg), 0f, 100f, .5f, .675f, 2.1f);
            for (var i = g.firstSprite; i < tot; i++)
            {
                s = sprites[i];
                s.element = Futile.atlasManager.GetElementWithName((i == g.firstSprite ? g.TrueSpriteOverride : "KrakenMask") + sprite);
                s.scaleX = sgn != 0f && sprite is not 0 and not 8 ? sgn : 1f;
                s.anchorY = rt;
                s.anchorX = .5f - zRot.x * .1f * sgn;
                s.rotation = degRot;
                s.SetPosition(pos - camPos);
            }
            s = sprites[g.firstSprite + 3];
            s.element = Futile.atlasManager.GetElementWithName("M4RScavMaskRag" + sprite + (sgn >= 0 ? "A" : "B") + g.Mode);
            s.anchorY = rt;
            s.anchorX = .5f - zRot.x * .1f;
            s.rotation = degRot;
            s.color = g.RagColor;
            s.SetPosition(pos - camPos);
            var s1 = sprites[g.firstSprite + 1];
            s1.scaleX *= .85f;
            s1.scaleY = .9f;
            var s2 = sprites[g.firstSprite + 2];
            s2.scaleY = 1.1f;
            s2.anchorY += .015f;
            if (g.attachedTo is PlayerCarryableItem it && it.blink > 0 && Random.value < .5f)
            {
                for (var j = g.firstSprite; j < tot; j++)
                    sprites[g.firstSprite + j].color = Color.white;
                return;
            }
            g.color = Color.Lerp(Color.Lerp(g.ColorA.rgb, Color.white, .35f * g.fallOffVultureMode), g.blackColor, g.ignoreDarkness ? 0f : Mathf.Lerp(.2f, 1f, Mathf.Pow(dark, 2f)));
            if (g.glimmer)
            {
                var rotTwk = zRot * 5f;
                g.color = Color.Lerp(g.color, g.blackColor, Mathf.PerlinNoise(rotTwk.x, rotTwk.y) * .5f);
            }
            if (g.Rag1 is M4RScavMaskGraphics.Rag rag1)
            {
                rag1.Layer = sprite == 8 || (sgn <= 0 && sprite != 0) ? 0 : 1;
                rag1.DrawSprites(sLeaser, rCam, timeStacker, camPos, pos, s.GetAnchor());
            }
            if (g.Rag2 is M4RScavMaskGraphics.Rag rag2)
            {
                rag2.Layer = sprite == 8 || (sgn >= 0 && sprite != 0) ? 0 : 1;
                rag2.DrawSprites(sLeaser, rCam, timeStacker, camPos, pos, s.GetAnchor());
            }
            sprites[g.firstSprite].color = g.color;
            s2.color = s1.color = Color.Lerp(g.color, g.blackColor, Mathf.Lerp(.75f, 1f, dark));
        }
        else
            orig(self, sLeaser, rCam, timeStacker, camPos);
    }

    internal static void On_VultureMaskGraphics_InitiateSprites(On.MoreSlugcats.VultureMaskGraphics.orig_InitiateSprites orig, VultureMaskGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (self is M4RScavMaskGraphics g)
        {
            var sprites = sLeaser.sprites;
            sprites[g.firstSprite] = new("pixel");
            sprites[g.firstSprite + 1] = new("pixel");
            sprites[g.firstSprite + 2] = new("pixel");
            sprites[g.firstSprite + 3] = new("pixel");
            g.Rag1?.InitiateSprites(sLeaser, rCam);
            g.Rag2?.InitiateSprites(sLeaser, rCam);
            g.AddToContainer(sLeaser, rCam, null);
        }
        else
            orig(self, sLeaser, rCam);
    }

    internal static void On_VultureMaskGraphics_Update(On.MoreSlugcats.VultureMaskGraphics.orig_Update orig, VultureMaskGraphics self)
    {
        orig(self);
        if (self is M4RScavMaskGraphics g)
        {
            g.Rag1?.Update();
            g.Rag2?.Update();
        }
    }
}