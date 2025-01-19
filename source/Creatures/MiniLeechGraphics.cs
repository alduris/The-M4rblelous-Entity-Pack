using System;
using UnityEngine;

namespace LBMergedMods.Creatures;

public class MiniLeechGraphics : LeechGraphics
{
    public MiniLeechGraphics(MiniLeech ow) : base(ow)
    {
        var bdy = body;
        for (var i = 0; i < bdy.Length; i++)
            bdy[i].rad *= .5f;
        var rads = radiuses;
        for (var i = 0; i < rads.Length; i++)
            rads[i] *= .5f;
        Array.Resize(ref radiuses, 3);
        Array.Resize(ref body, 3);
        Array.Resize(ref bodyParts, 3);
    }

    public override void Update()
    {
        base.Update();
        if (culled)
            return;
        var rads = radiuses;
        for (var i = 1; i < rads.Length - 1; i++)
            rads[i] *= .5f;
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        if (culled || leech is not MiniLeech l)
            return;
        sLeaser.sprites[0].color = Color.Lerp(blackColor, Color.Lerp(Color.white, rCam.currentPalette.fogColor, .25f), l.airDrown);
    }
}