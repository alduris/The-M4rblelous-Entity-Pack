using System;
using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;

public class CommonEelGraphics : LizardGraphics
{
    public CommonEelGraphics(CommonEel ow) : base(ow)
    {
        iVars.tailColor = .001f;
        iVars.tailFatness = 1f;
        iVars.fatness = Math.Min(iVars.fatness, .65f) * 1.1f;
        overrideHeadGraphic = -1;
    }

    public override void Update()
    {
        base.Update();
        if (lightSource is LightSource l)
            l.setAlpha = 0f;
        if (lizard is CommonEel eel)
        {
            eel.bubble = 0;
            eel.bubbleIntensity = 0f;
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && !debugVisualization && lizard is CommonEel eel)
        {
            var sprites = sLeaser.sprites;
            var end = SpriteLimbsEnd;
            for (int num8 = SpriteLimbsColorStart - SpriteLimbsStart, tl = SpriteLimbsStart; tl < end; tl++)
            {
                sprites[tl].isVisible = false;
                sprites[tl + num8].isVisible = false;
            }
            var ef = effectColor;
            ref readonly var palette = ref rCam.currentPalette;
            var shd = sprites[SpriteHeadStart];
            shd.element = Futile.atlasManager.GetElementWithName("NLG" + shd.element.name);
            shd.scaleX *= .98f;
            shd.scaleY *= 1.02f;
            shd.anchorY = .575f;
            shd.anchorX = .525f;
            sprites[SpriteHeadStart + 1].isVisible = false;
            sprites[SpriteHeadStart + 2].isVisible = false;
            shd = sprites[SpriteHeadStart + 3];
            shd.element = Futile.atlasManager.GetElementWithName("NLG" + shd.element.name);
            shd.anchorY = .575f;
            shd.anchorX = .53f;
            shd = sprites[SpriteHeadStart + 4];
            shd.element = Futile.atlasManager.GetElementWithName("NLG" + shd.element.name);
            shd.scaleY *= 1.02f;
            shd.anchorY = .575f;
            shd.anchorX = .53f;
            shd.color = eel.dead ? palette.blackColor : ef;
            var mesh = (sprites[SpriteBodyMesh] as TriangleMesh)!;
            var a = Vector2.Lerp(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker), .2f);
            var vector = BodyPosition(0, timeStacker);
            Vector2 normalized = (a - vector).normalized, vector2 = Custom.PerpendicularVector(normalized);
            var num7 = Vector2.Distance(vector, a);
            mesh.MoveVertice(0, vector + normalized * 1.725f * num7 - vector2 * 5f - camPos);
            mesh.MoveVertice(1, vector + normalized * 1.725f * num7 + vector2 * 5f - camPos);
            var num6 = (BodyChunkDisplayRad(1) + BodyChunkDisplayRad(0)) / 2f;
            mesh.MoveVertice(2, vector - vector2 * 1.025f * num6 - camPos);
            mesh.MoveVertice(3, vector + vector2 * 1.025f * num6 - camPos);
            end = SpriteBodyCirclesEnd;
            var mbc = eel.mainBodyChunk;
            for (var k = SpriteBodyCirclesStart; k < end; k++)
            {
                var spr1 = sprites[k];
                spr1.element = Futile.atlasManager.GetElementWithName("NLGCircle20");
                spr1.rotation = Custom.VecToDeg(Custom.DirVec(mbc.lastPos, mbc.pos));
                if (k != end - 1)
                    spr1.scale *= 1.2f;
                else
                    spr1.scale = 1.2f;
            }
            var vertCols = (sprites[SpriteTail] as TriangleMesh)!.verticeColors;
            var l = vertCols.Length;
            if (Mathf.Sign(Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker)) == 1)
            {
                for (var j = 0; j < vertCols.Length; j++)
                {
                    if (j % 2 == 1)
                        vertCols[j] = Color.Lerp(ef, palette.blackColor, .5f);
                    else
                        vertCols[j] = palette.blackColor;
                }
                if (1 < l)
                {
                    vertCols[1] = palette.blackColor;
                    if (3 < l)
                    {
                        vertCols[3] = Color.Lerp(ef, palette.blackColor, .875f);
                        if (5 < l)
                        {
                            vertCols[5] = Color.Lerp(ef, palette.blackColor, .75f);
                            if (7 < l)
                                vertCols[7] = Color.Lerp(ef, palette.blackColor, .625f);
                        }
                    }
                }
            }
            else
            {
                for (var j = 0; j < vertCols.Length; j++)
                {
                    if (j % 2 == 0)
                        vertCols[j] = Color.Lerp(ef, palette.blackColor, .5f);
                    else
                        vertCols[j] = palette.blackColor;
                }
                if (0 < l)
                {
                    vertCols[0] = palette.blackColor;
                    if (2 < l)
                    {
                        vertCols[2] = Color.Lerp(ef, palette.blackColor, .875f);
                        if (4 < l)
                        {
                            vertCols[4] = Color.Lerp(ef, palette.blackColor, .75f);
                            if (6 < l)
                                vertCols[6] = Color.Lerp(ef, palette.blackColor, .625f);
                        }
                    }
                }
            }
        }
    }
}