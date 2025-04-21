using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;
//CHK
public class ScutigeraFlash(Vector2 pos, float size) : ElectricDeath.SparkFlash(pos, size)
{
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprs = sLeaser.sprites;
        sprs[0].shader = Custom.rainWorld.Shaders["LightSource"];
        sprs[1].shader = Custom.rainWorld.Shaders["FlatLight"];
        sprs[2].shader = Custom.rainWorld.Shaders["FlareBomb"];
        sprs[2].color = sprs[1].color = sprs[0].color = new(.7f, 1f, .7f);
    }
}