using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class RedHorrorShell(Vector2 pos, Vector2 vel, float hue, float saturation, float scaleX, float scaleY) : CentipedeShell(pos, vel, hue, saturation, scaleX, scaleY)
{
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites[1].element = sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("RedHorrorBackShell");
    }
}