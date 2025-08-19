global using static LBMergedMods.Hooks.TubeWormHooks;
using UnityEngine;
using RWCustom;

namespace LBMergedMods.Hooks;

public static class TubeWormHooks
{
    internal static void On_Tongue_Shoot(On.TubeWorm.Tongue.orig_Shoot orig, TubeWorm.Tongue self, Vector2 dir)
    {
        var mode = self.mode;
        orig(self, dir);
        if (self.worm.IsBig() && !self.Attached && mode == TubeWorm.Tongue.Mode.Retracted)
            self.requestedRopeLength = 280f;
    }

    internal static void On_TubeWorm_NewRoom(On.TubeWorm.orig_NewRoom orig, TubeWorm self, Room newRoom)
    {
        orig(self, newRoom);
        if (Big.TryGetValue(self.abstractCreature, out var prop) && !prop.Born)
        {
            prop.Born = true;
            var flag = false;
            if (newRoom.game?.GetStorySession?.saveStateNumber?.value == "LBHardhatCat")
            {
                var state = Random.state;
                Random.InitState(self.abstractPhysicalObject.ID.RandomSeed);
                flag = Random.value <= .45f;
                Random.state = state;
            }
            if (newRoom.roomSettings?.GetEffectAmount(RoomEffectType.Bigrubs) > 0 || flag)
                prop.IsBig = true;
        }
    }

    internal static void On_TubeWorm_Update(On.TubeWorm.orig_Update orig, TubeWorm self, bool eu)
    {
        if (self.IsBig())
        {
            var ts = self.tongues;
            for (var i = 0; i < ts.Length; i++)
                ts[i].idealRopeLength = 300f;
            var bs = self.bodyChunks;
            for (var i = 0; i < bs.Length; i++)
            {
                var b = bs[i];
                b.rad = 9f;
                b.mass = .15f;
            }
            self.maxTotalRope = 400f;
        }
        orig(self, eu);
    }

    internal static void On_TubeWormGraphics_ApplyPalette(On.TubeWormGraphics.orig_ApplyPalette orig, TubeWormGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self.worm is TubeWorm w && w.IsBig(out var props) && !props.NormalLook)
            props.BlackCol = palette.blackColor;
        else
            orig(self, sLeaser, rCam, palette);
    }

    internal static void On_TubeWormGraphics_DrawSprites(On.TubeWormGraphics.orig_DrawSprites orig, TubeWormGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.culled && !self.dispose && self.worm is TubeWorm w && w.IsBig(out var props))
        {
            var sprs = sLeaser.sprites;
            var tm = (sprs[2] as TriangleMesh)!;
            if (!props.NormalLook)
            {
                ref readonly RoomPalette palette = ref rCam.currentPalette;
                var glowIntensity = (1f - props.RoomLight) * Mathf.Lerp(props.LastLightLife, props.LightLife, timeStacker);
                var centerPos = (w.bodyChunks[0].pos + w.bodyChunks[1].pos) * .5f;
                Color colBase = ChangeCol(self.color, timeStacker, props),
                    colRedu = ChangeCol(props.ReducedAsRGB, timeStacker, props);
                sprs[0].color = colBase;
                sprs[1].color = colRedu;
                for (var i = 3; i < 5; i++)
                {
                    var spr = sprs[i];
                    spr.color = GlowCol(Color.Lerp(palette.fogColor, Custom.HSL2RGB(.95f, 1f, .865f), .5f), Vector2.Distance(spr.GetPosition() + camPos, centerPos), colBase, glowIntensity);
                }
                var colors = tm.verticeColors;
                for (var i = 0; i < colors.Length; i++)
                {
                    var num = Mathf.Clamp01(Mathf.Sin(i / (colors.Length - 1f) * Mathf.PI));
                    colors[i] = GlowCol(Color.Lerp(palette.fogColor, Custom.HSL2RGB(Mathf.Lerp(.95f, 1f, num), 1f, Mathf.Lerp(.75f, .9f, Mathf.Pow(num, .15f))), .5f), Vector2.Distance(tm.vertices[i] + camPos, centerPos), colBase, glowIntensity);
                }
            }
            var bp = self.bodyParts;
            Vector2 vector = Vector2.Lerp(bp[0].lastPos, bp[0].pos, timeStacker);
            vector += Custom.DirVec(Vector2.Lerp(bp[1].lastPos, bp[1].pos, timeStacker), vector) * 10f;
            for (var i = 0; i < bp.Length; i++)
            {
                Vector2 vector2 = Vector2.Lerp(bp[i].lastPos, bp[i].pos, timeStacker),
                    b = i < 3 ? Vector2.Lerp(bp[i + 1].lastPos, bp[i + 1].pos, timeStacker) : (vector2 + Custom.DirVec(vector, vector2) * 10f),
                    normalized = (vector - vector2).normalized,
                    vector3 = Custom.PerpendicularVector(normalized);
                float num2 = Vector2.Distance(vector2, vector) / 4f,
                    num3 = Vector2.Distance(vector2, b) / 4f,
                    num4 = self.SegmentStretchFac(i, timeStacker) * 2f,
                    num5 = (i == 0 ? 3.5f : 5f) * num4,
                    num6 = (i == 3 ? 3.5f : 5f) * num4;
                for (var j = 0; j < 2; j++)
                {
                    var s = (sprs[j] as TriangleMesh)!;
                    s.MoveVertice(i * 4, vector - vector3 * num5 - normalized * num2 - camPos);
                    s.MoveVertice(i * 4 + 1, vector + vector3 * num5 - normalized * num2 - camPos);
                    s.MoveVertice(i * 4 + 2, vector2 - vector3 * num6 + normalized * num3 - camPos);
                    s.MoveVertice(i * 4 + 3, vector2 + vector3 * num6 + normalized * num3 - camPos);
                }
                vector = vector2;
            }
            var rs = self.ropeSegments;
            var b2 = Mathf.Lerp(self.lastStretch, self.stretch, timeStacker);
            vector = Vector2.Lerp(rs[1].lastPos, rs[1].pos, timeStacker);
            vector += Custom.DirVec(Vector2.Lerp(rs[2].lastPos, rs[2].pos, timeStacker), vector) * 1f;
            for (var k = 1; k < rs.Length; k++)
            {
                Vector2 vector4 = Vector2.Lerp(rs[k].lastPos, rs[k].pos, timeStacker),
                    vector5 = Custom.PerpendicularVector((vector - vector4).normalized);
                var num8 = .4f + 3.2f * Mathf.Lerp(1f, b2, Mathf.Pow(Mathf.Sin((float)k / (rs.Length - 1) * Mathf.PI), .7f));
                var km1f4 = (k - 1) * 4;
                tm.MoveVertice(km1f4, vector - vector5 * num8 - camPos);
                tm.MoveVertice(km1f4 + 1, vector + vector5 * num8 - camPos);
                tm.MoveVertice(km1f4 + 2, vector4 - vector5 * num8 - camPos);
                tm.MoveVertice(km1f4 + 3, vector4 + vector5 * num8 - camPos);
                vector = vector4;
            }
            for (var i = 3; i < 5; i++)
            {
                var spr = sprs[i];
                spr.scaleX = .4f;
                spr.scaleY *= 2f;
            }
        }
    }

    internal static void On_TubeWormGraphics_Reset(On.TubeWormGraphics.orig_Reset orig, TubeWormGraphics self)
    {
        orig(self);
        if (self.worm.IsBig(out var props) && !props.NormalLook)
            props.LastLightLife = props.LightLife;
    }

    internal static void On_TubeWormGraphics_Update(On.TubeWormGraphics.orig_Update orig, TubeWormGraphics self)
    {
        orig(self);
        if (self.worm is TubeWorm w && w.IsBig(out var props))
        {
            var baseNewCol = props.NewCol;
            if (!props.InitGraphics)
            {
                if (!props.NormalLook)
                {
                    props.LightLife = w.dead ? 0f : 1f;
                    props.NewCol = new(w.abstractCreature.superSizeMe ? Custom.WrappedRandomVariation(.11f, .025f, .6f) : Custom.WrappedRandomVariation(.32f, .1f, .6f), 1f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
                    var rdHSL = baseNewCol;
                    rdHSL.lightness *= .21f;
                    rdHSL.saturation = Mathf.Clamp01(rdHSL.saturation * .21f);
                    props.ReducedAsRGB = rdHSL.rgb;
                    props.LastLightLife = props.LightLife;
                }
                props.InitGraphics = true;
            }
            else if (!props.NormalLook)
            {
                self.color = baseNewCol.rgb;
                props.LastLightLife = props.LightLife;
                props.LightLife = Mathf.Clamp01(props.LightLife + (w.dead ? -.00125f : .00125f));
                var pos = w.bodyChunks[0].pos * .5f + w.bodyChunks[1].pos * .5f;
                var darkness = w.room?.Darkness(pos) ?? 0f;
                props.RoomLight = Mathf.Pow(1f - Mathf.InverseLerp(0f, .5f, darkness), 3f);
                if (props.LightSource is LightSource lh)
                {
                    lh.stayAlive = true;
                    lh.setPos = pos;
                    lh.setRad = 280f * props.LightLife;
                    lh.setAlpha = .8f * Mathf.Pow(props.LightLife, .5f);
                    lh.color = self.color;
                    if (lh.slatedForDeletetion || darkness == 0f)
                        props.LightSource = null;
                }
                else if (darkness > 0f)
                    w.room?.AddObject(props.LightSource = new(pos, false, Color.white, w) { requireUpKeep = true });
            }
        }
    }

    internal static Color ChangeCol(Color col, float timeStacker, BigrubProperties props) => MaxBlack(Color.Lerp(col, props.BlackCol, (1f - (1f - props.RoomLight) * Mathf.Lerp(props.LastLightLife, props.LightLife, timeStacker)) * .6f));

    internal static Color GlowCol(Color col, float dist, Color mBase, float glowAlpha) => col + ((mBase * Mathf.InverseLerp(100f, 0f, dist) * .5f * glowAlpha) with { a = 0f });

    public static bool IsBig(this TubeWorm self) => Big.TryGetValue(self.abstractCreature, out var prop) && prop.IsBig;

    public static bool IsBig(this TubeWorm self, out BigrubProperties prop) => Big.TryGetValue(self.abstractCreature, out prop) && prop.IsBig;

    public static bool IsBig(this AbstractCreature self) => Big.TryGetValue(self, out var prop) && prop.IsBig;

    public static bool IsBig(this AbstractCreature self, out BigrubProperties prop) => Big.TryGetValue(self, out prop) && prop.IsBig;

    internal static Color MaxBlack(Color a) => new(a.r >= .02f ? a.r : .02f, a.g >= .02f ? a.g : .02f, a.b >= .02f ? a.b : .02f);
}