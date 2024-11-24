using UnityEngine;

namespace LBMergedMods.Creatures;

public class ScutigeraFlash(Vector2 pos, float size) : ElectricDeath.SparkFlash(pos, size)
{
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
            sprs[i].color = new(.7f, 1f, .7f);
    }
}