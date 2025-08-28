using UnityEngine;
using LizardCosmetics;
using RWCustom;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Creatures;

public class MoleSalamanderGraphics : LizardGraphics
{
    public MoleSalamanderGraphics(MoleSalamander ow) : base(ow)
    {
        blackSalamander = ow.Black;
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        AddCosmetic(spriteIndex, new Whiskers(this, spriteIndex));
        overrideHeadGraphic = -1;
    }

    public override void Update()
    {
        base.Update();
        if (lightSource is LightSource l)
        {
            if (HeadLightsUpFromNoise)
                l.color = Color.white;
            else
                l.setAlpha = 0f;
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && !debugVisualization && lizard is MoleSalamander liz)
        {
            var sprites = sLeaser.sprites;
            var num2 = Mathf.Lerp(liz.lastJawOpen, liz.JawOpen, timeStacker);
            if (liz.JawReadyForBite && liz.Consious)
                num2 += Random.value * .2f;
            num2 = Mathf.Lerp(num2, Mathf.Lerp(lastVoiceVisualization, voiceVisualization, timeStacker) + .2f, Mathf.Lerp(lastVoiceVisualizationIntensity, voiceVisualizationIntensity, timeStacker) * .8f);
            num2 = Mathf.Clamp(num2, 0f, 1f);
            var c = !blackSalamander ? effectColor : palette.blackColor;
            for (var m = 7; m < 11; m++)
            {
                sprites[m + 9].color = c;
                sprites[m + 9].alpha = !blackSalamander ? (m % 2 != 1 ? .3f : Mathf.Lerp(.3f, .1f, Math.Abs(Mathf.Lerp(lastDepthRotation, depthRotation, timeStacker)))) : Mathf.Sin(whiteCamoColorAmount * Mathf.PI) * .3f;
            }
            if (blackSalamander && HeadLightsUpFromNoise)
                sprites[13].color = Color.Lerp(palette.blackColor, new(.5f, .5f, .5f), Mathf.Pow(blackLizardLightUpHead, 1f - .95f * num2));
            sprites[15].isVisible = false;
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (!debugVisualization && lizard is MoleSalamander liz)
        {
            if (!blackSalamander)
                ColorBody(sLeaser, SalamanderColor);
            var sprites = sLeaser.sprites;
            var jawOpen = liz.JawOpen;
            if (liz.JawReadyForBite && liz.Consious)
                jawOpen += Random.value * .2f;
            jawOpen = Mathf.Clamp(Mathf.Lerp(jawOpen, voiceVisualization + .2f, voiceVisualizationIntensity * .8f), 0f, 1f);
            var c = !blackSalamander ? effectColor : palette.blackColor;
            for (var m = 7; m < 11; m++)
                sprites[m + 9].color = c;
            if (blackSalamander && HeadLightsUpFromNoise)
                sprites[13].color = Color.Lerp(palette.blackColor, new(.5f, .5f, .5f), Mathf.Pow(blackLizardLightUpHead, 1f - .95f * jawOpen));
        }
    }

    public virtual float HeadRotation(Lizard lizard, float timeStacker)
    {
        var num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker));
        var num2 = Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker);
        var num3 = Mathf.Clamp(Mathf.Lerp(lizard.lastJawOpen, lizard.JawOpen, timeStacker), 0f, 1f);
        return num + lizard.lizardParams.jawOpenAngle * (1.5f - (lizard.lizardParams.jawOpenLowerJawFac / 3f)) * num3 * num2;
    }
}