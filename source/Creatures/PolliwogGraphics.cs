using UnityEngine;
using LizardCosmetics;

namespace LBMergedMods.Creatures;

public class PolliwogGraphics : LizardGraphics
{
    public PolliwogGraphics(PhysicalObject ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        var spriteIndex = startOfExtraSprites + extraSprites;
        spriteIndex = AddCosmetic(spriteIndex, new AxolotlGills(this, spriteIndex));
        AddCosmetic(spriteIndex, new TailFin(this, spriteIndex));
        Random.state = state;
        overrideHeadGraphic = -1;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (!culled && !debugVisualization)
        {
            var sprites = sLeaser.sprites;
            for (int num8 = SpriteLimbsColorStart - SpriteLimbsStart, l = SpriteLimbsStart + 2; l < SpriteLimbsEnd; l++)
            {
                sprites[l].isVisible = false;
                sprites[l + num8].isVisible = false;
            }
        }
    }
}