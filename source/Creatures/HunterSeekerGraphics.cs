using UnityEngine;
using LizardCosmetics;

namespace LBMergedMods.Creatures;
//CHK
public class HunterSeekerGraphics : LizardGraphics
{
    public HunterSeekerGraphics(HunterSeeker ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        if (Random.value < .5f)
            spriteIndex = AddCosmetic(spriteIndex, new TailGeckoScales(this, spriteIndex));
        var ts = ow.lizardParams.tailSegments;
        for (var k = 0; k < ts; k++)
        {
            var num3 = Mathf.InverseLerp(0f, ts - 1, k);
            var tk = tail[k];
            tk.rad += Mathf.Sin(Mathf.Pow(num3, .7f) * Mathf.PI) * 2.5f;
            tk.rad *= 1f - Mathf.Sin(Mathf.InverseLerp(0f, .4f, num3) * Mathf.PI) * .5f;
        }
        spriteIndex = AddCosmetic(spriteIndex, new WingScales(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new WingScales(this, spriteIndex));
        spriteIndex = (Random.value >= .5f || iVars.tailColor != 0f) ? AddCosmetic(spriteIndex, new TailGeckoScales(this, spriteIndex)) : AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new JumpRings(this, spriteIndex));
        if (Random.value < .4f)
            spriteIndex = AddCosmetic(spriteIndex, new BumpHawk(this, spriteIndex));
        else if (Random.value < .4f)
            spriteIndex = AddCosmetic(spriteIndex, new ShortBodyScales(this, spriteIndex));
        else if (Random.value < .2f)
            spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        else if (Random.value < .2f)
            spriteIndex = AddCosmetic(spriteIndex, new LongHeadScales(this, spriteIndex));
        if (Random.value < .5f)
            AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex));
        Random.state = state;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var flag = !culled && !debugVisualization;
        if (flag && lizard is HunterSeeker liz)
        {
            ColorBody(sLeaser, DynamicBodyColor(0f));
            Color color = rCam.PixelColorAtCoordinate(liz.mainBodyChunk.pos),
                color2 = rCam.PixelColorAtCoordinate(liz.bodyChunks[1].pos),
                color3 = rCam.PixelColorAtCoordinate(liz.bodyChunks[2].pos);
            if (color == color2)
                whitePickUpColor = color;
            else if (color2 == color3)
                whitePickUpColor = color2;
            else if (color3 == color)
                whitePickUpColor = color3;
            else
                whitePickUpColor = (color + color2 + color3) / 3f;
            if (whiteCamoColorAmount == -1f)
            {
                whiteCamoColor = whitePickUpColor;
                whiteCamoColorAmount = 1f;
            }
        }
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (flag)
        {
            var sprites = sLeaser.sprites;
            var num8 = SpriteLimbsColorStart - SpriteLimbsStart;
            var end = SpriteLimbsEnd;
            for (var m = SpriteLimbsStart; m < end; m++)
            {
                var s = sprites[m + num8];
                s.alpha = Mathf.Sin(whiteCamoColorAmount * Mathf.PI) * .3f;
                s.color = palette.blackColor;
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (!debugVisualization)
            ColorBody(sLeaser, Color.white);
    }
}