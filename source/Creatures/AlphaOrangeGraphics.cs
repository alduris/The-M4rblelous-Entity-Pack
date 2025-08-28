using UnityEngine;
using LizardCosmetics;

namespace LBMergedMods.Creatures;

public class AlphaOrangeGraphics : LizardGraphics
{
    public AlphaOrangeGraphics(AlphaOrange ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new WingScales(this, spriteIndex));
        spriteIndex = Random.value < .5f || iVars.tailColor != 0f ? AddCosmetic(spriteIndex, new TailGeckoScales(this, spriteIndex)) : AddCosmetic(spriteIndex, new TailTuft(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new JumpRings(this, spriteIndex));
        AddCosmetic(spriteIndex, new Antennae(this, spriteIndex));
        var segs = ow.lizardParams.tailSegments;
        for (var i = 0; i < segs; i++)
        {
            var num4 = Mathf.InverseLerp(0f, segs - 1, i);
            var tailSeg = tail[i];
            tailSeg.rad += Mathf.Sin(Mathf.Pow(num4, .7f) * Mathf.PI) * 2.5f;
            tailSeg.rad *= 1f - Mathf.Sin(Mathf.InverseLerp(0f, .4f, num4) * Mathf.PI) * .5f;
        }
        Random.state = state;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && !debugVisualization)
            sLeaser.sprites[SpriteHeadStart + 3].color = sLeaser.sprites[SpriteHeadStart].color = HeadColor(timeStacker);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (!debugVisualization)
        {
            sLeaser.sprites[SpriteHeadStart + 3].color = sLeaser.sprites[SpriteHeadStart].color = HeadColor(1f);
            if (lizard.rotModule is not null && lizard.LizardState.rotType == LizardState.RotType.Full)
                sLeaser.sprites[SpriteHeadStart + 4].color = lizard.rotModule.rotColor;
        }
    }
}