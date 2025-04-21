using UnityEngine;

namespace LBMergedMods.Creatures;
//CHK
public class MiniScutigeraGraphics : CentipedeGraphics
{
    public MiniScutigeraGraphics(MiniScutigera ow) : base(ow)
    {
        var state = Random.state;
        Random.InitState(ow.abstractPhysicalObject.ID.RandomSeed);
        hue = Mathf.Lerp(.1527777777777778f, .1861111111111111f, Random.value);
        saturation = Mathf.Lerp(.294f, .339f, Random.value);
        Random.state = state;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (centipede is MiniScutigera c)
        {
            var chs = c.bodyChunks;
            var sprs = sLeaser.sprites;
            for (var i = 0; i < chs.Length; i++)
            {
                var spr = sprs[ShellSprite(i, 0)];
                if (spr.element.name is "CentipedeBackShell")
                    spr.element = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
                else if (spr.element.name is "CentipedeBellyShell")
                    spr.element = Futile.atlasManager.GetElementWithName("ScutigeraBellyShell");
            }
        }
    }
}