using UnityEngine;
using RWCustom;

namespace LBMergedMods.Creatures;
//CHK
public class FlyingBigEelGraphics : BigEelGraphics
{
    public FlyingBigEelGraphics(FlyingBigEel ow) : base(ow)
    {
        var eyes = eyesData;
        for (var n = 0; n < eyes.Length; n++)
            eyes[n] *= .75f;
    }

    public override void Reset()
    {
        base.Reset();
        if (finSound is StaticSoundLoop snd)
            snd.volume = 0f;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        var sprites = sLeaser.sprites;
        sprites[MeshSprite].shader = Custom.rainWorld.Shaders["GRJEelBody"];
        for (var k = 0; k < 2; k++)
        {
            for (var l = 0; l < fins.Length; l++)
                sprites[FinSprite(l, k)].shader = Custom.rainWorld.Shaders["TentaclePlant"];
            for (var num = 0; num < 2; num++)
            {
                for (var n = 0; n < 4; n++)
                {
                    if (n % 2 == 0)
                        sprites[BeakArmSprite(n, num, k)].scaleX *= .75f;
                    else
                        sprites[BeakArmSprite(n, num, k)].scale *= .75f;
                }
                sprites[BeakSprite(k, num)].element = Futile.atlasManager.GetElementWithName("FEelJaw" + (2 - k) + (num is 0 ? "A" : "B"));
            }
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (eel is BigEel be)
        {
            var sprites = sLeaser.sprites;
            var chs = be.bodyChunks;
            var vector4 = Vector2.Lerp(chs[0].lastPos, chs[0].pos, timeStacker);
            var vec = Vector2.Lerp(chs[1].lastPos, chs[1].pos, timeStacker);
            var vec2 = Vector2.Lerp(vector4, vec, .5f) - camPos;
            for (var k = 0; k < 2; k++)
            {
                for (var num12 = 0; num12 < 2; num12++)
                {
                    for (var num14 = 0; num14 < 4; num14++)
                    {
                        var s = sprites[BeakArmSprite(num14, num12, k)];
                        s.x = Mathf.Lerp(s.x, vec2.x, .2f);
                        s.y = Mathf.Lerp(s.y, vec2.y, .2f);
                    }
                }
            }
        }
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        base.ApplyPalette(sLeaser, rCam, palette);
        if (eel is FlyingBigEel be)
        {
            var state = Random.state;
            Random.InitState(be.abstractPhysicalObject.ID.RandomSeed);
            var sprites = sLeaser.sprites;
            var flg = fins.Length;
            for (var k = 0; k < 2; k++)
            {
                var beak = sprites[BeakSprite(k, 1)];
                beak.color = Color.Lerp(beak.color, RainWorld.GoldRGB, .65f);
                for (var l = 0; l < numberOfScales; l++)
                {
                    var scl = sprites[ScaleSprite(l, k)];
                    scl.color = Color.Lerp(scl.color, HSLColor.Lerp(be.iVars.patternColorA, be.iVars.patternColorB, Random.value).rgb, .8f);
                }
                for (var m = 0; m < flg; m++)
                    sprites[FinSprite(m, k)].color = HSLColor.Lerp(be.iVars.patternColorA, be.iVars.patternColorB, Random.value).rgb;
            }
            Random.state = state;
        }
    }
}