using UnityEngine;
using MoreSlugcats;

namespace LBMergedMods.Creatures;
//CHK
public class RedHorrorFlash(Vector2 pos, float size) : SingularityBomb.SparkFlash(pos, size, Color.blue)
{
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprs = sLeaser.sprites;
        for (var i = 0; i < sprs.Length; i++)
            sprs[i].color = new(.7f, .7f, 1f);
    }
}