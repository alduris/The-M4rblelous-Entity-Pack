using UnityEngine;

namespace LBMergedMods.Creatures;

public class RedHorrorGraphics : CentipedeGraphics
{
    public RedHorrorGraphics(RedHorror ow) : base(ow)
    {
        hue = Mathf.Lerp(-.02f, .01f, Random.value);
        saturation = .9f + .1f * Random.value;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (centipede is RedHorror c)
        {
            var sprs = sLeaser.sprites;
            var lg = c.bodyChunks.Length;
            for (var i = 0; i < lg; i++)
            {
                var spr = sprs[ShellSprite(i, 0)];
                if (spr.element.name is "CentipedeBackShell")
                    spr.element = Futile.atlasManager.GetElementWithName("RedHorrorBackShell");
                else if (spr.element.name is "CentipedeBellyShell")
                    spr.element = Futile.atlasManager.GetElementWithName("RedHorrorBellyShell");
            }
            var voided = c.abstractCreature.IsVoided();
            for (var k = 0; k < 2; k++)
            {
                for (var num15 = 0; num15 < wingPairs; num15++)
                {
                    if (sprs[WingSprite(k, num15)] is CustomFSprite cSpr)
                    {
                        cSpr.verticeColors[1] = cSpr.verticeColors[0] = PinkToBlue(cSpr.verticeColors[0], voided);
                        cSpr.verticeColors[2] = cSpr.verticeColors[3] = PinkToBlue2(cSpr.verticeColors[3], voided);
                    }
                }
            }
        }
    }

    public static Color PinkToBlue(Color clr, bool isVoided)
    {
        var col = Color.Lerp(new(clr.b, clr.g, clr.r), Color.white, .35f);
        return isVoided ? Color.Lerp(col, RainWorld.SaturatedGold, .5f) : col;
    }

    public static Color PinkToBlue2(Color clr, bool isVoided)
    {
        var col = Color.Lerp(Color.Lerp(new(clr.b, clr.g, clr.r), Color.white, .35f), Color.blue, .2f);
        return isVoided ? Color.Lerp(col, RainWorld.SaturatedGold, .5f) : col;
    }
}