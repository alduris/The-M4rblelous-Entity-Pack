using RWCustom;
using UnityEngine;

namespace LBMergedMods.Items;

public class DarkGrubVision(Vector2 pos, float rad) : UpdatableAndDeletable, IDrawable
{
    public Vector2 LastPos = pos, Pos = pos;
    public float LastRad = rad, Rad = rad;
    public Vector2? SetPos;
    public float? SetRad;

    public override void Update(bool eu)
    {
        base.Update(eu);
        LastRad = Rad;
        LastPos = Pos;
        if (slatedForDeletetion)
            LastRad = Rad = 0f;
        else
        {
            if (SetRad.HasValue)
                Rad = SetRad.Value;
            if (SetPos.HasValue)
                Pos = SetPos.Value;
        }
    }

    public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = [new("Futile_White") { color = Color.green, shader = Custom.rainWorld.Shaders["DarkGrubVision"] }];
        AddToContainer(sLeaser, rCam, null);
    }

    public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Water");
        var s0 = sLeaser.sprites[0];
        s0.RemoveFromContainer();
        newContainer.AddChild(s0);
    }

    public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var s0 = sLeaser.sprites[0];
        s0.SetPosition(Vector2.Lerp(LastPos, Pos, timeStacker) - camPos);
        s0.scale = Mathf.Lerp(LastRad, Rad, timeStacker) / 200f;
        s0.alpha = rCam.currentPalette.darkness;
        if (!sLeaser.deleteMeNextFrame && (slatedForDeletetion || room != rCam.room))
            sLeaser.CleanSpritesAndRemove();
    }

    public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }
}