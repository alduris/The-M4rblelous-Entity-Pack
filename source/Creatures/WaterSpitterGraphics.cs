using UnityEngine;
using LizardCosmetics;
using RWCustom;

namespace LBMergedMods.Creatures;
//CHK
public class WaterSpitterGraphics : LizardGraphics
{
    public WaterSpitterGraphics(WaterSpitter ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new Whiskers(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));
        if (Random.value < .4f)
            spriteIndex = AddCosmetic(spriteIndex, new LongShoulderScales(this, spriteIndex));
        if (Random.value < .4f)
            AddCosmetic(spriteIndex, new ShortBodyScales(this, spriteIndex));
        Random.state = state;
        overrideHeadGraphic = -1;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && !debugVisualization)
        {
            var sprites = sLeaser.sprites;
            for (var i = 7; i < 11; i++)
                sprites[i + 9].color = Color.Lerp(palette.waterSurfaceColor1, Color.white, .1f);
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