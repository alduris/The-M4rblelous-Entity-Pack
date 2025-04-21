global using static LBMergedMods.Hooks.SpearHooks;
using UnityEngine;

namespace LBMergedMods.Hooks;
//CHK
public static class SpearHooks
{
    internal static void On_Spear_Update(On.Spear.orig_Update orig, Spear self, bool eu)
    {
        orig(self, eu);
        if (self.mode == Weapon.Mode.StuckInCreature && !self.stuckInWall.HasValue && self.stuckInObject is ChipChop ch && !ch.slatedForDeletetion && ch.graphicsModule is ChipChopGraphics gr)
        {
            var ang = self.stuckRotation * (Mathf.PI / 180f);
            var newAng = (ang + Mathf.Atan2(-gr.BodyDir.x, gr.BodyDir.y)) % (2f * Mathf.PI);
            self.setRotation = new(Mathf.Cos(newAng), Mathf.Sin(newAng));
        }
    }
}