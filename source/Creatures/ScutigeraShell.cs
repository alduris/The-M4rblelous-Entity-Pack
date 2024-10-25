using UnityEngine;

namespace LBMergedMods.Creatures;

public class ScutigeraShell(Vector2 pos, Vector2 vel, float hue, float saturation, float scaleX, float scaleY) : CentipedeShell(pos, vel, hue, saturation, scaleX, scaleY)
{
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var elem = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
        sLeaser.sprites[0].element = elem;
        sLeaser.sprites[1].element = elem;
    }
}
