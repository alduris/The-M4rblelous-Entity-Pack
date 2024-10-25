using UnityEngine;

namespace LBMergedMods.Creatures;

public class ScutigeraFlash(Vector2 pos, float size, Centipede ow) : ElectricDeath.SparkFlash(pos, size)
{
    public Centipede Owner = ow;

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprs = sLeaser.sprites;
        var clr = ShockColorIfScut(.7f, .7f, 1f, Owner);
        for (var i = 0; i < sprs.Length; i++)
            sprs[i].color = clr;
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
    }
}