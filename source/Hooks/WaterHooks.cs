global using static LBMergedMods.Hooks.WaterHooks;
using System.Collections.Generic;
using UnityEngine;

namespace LBMergedMods.Hooks;
//CHK
public static class WaterHooks
{
    internal static void On_Water_DrawSprites(On.Water.orig_DrawSprites orig, Water self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.room?.game?.Players is List<AbstractCreature> clist)
        {
            for (var i = 0; i < clist.Count; i++)
            {
                if (clist[i] is AbstractCreature cr && PlayerData.TryGetValue(cr, out var props) && props.BlueFaceDuration > 10 && cr.realizedCreature is Player p && !p.isNPC && p.Submersion >= 1f && p.Consious)
                {
                    sLeaser.sprites[1].isVisible = false;
                    break;
                }
            }
        }
    }
}