global using static LBMergedMods.Hooks.SpearHooks;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;
//CHK
public static class SpearHooks
{
    internal static void IL_Spear_HitSomething(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            s_MatchLdloc_OutLoc1,
            s_MatchIsinst_Cicada))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((Spear self, Creature creature) =>
             {
                 if (creature is Caterpillar c && c.Glowing && self.thrownBy is Player p)
                     p.glowing = true;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Spear.HitSomething!");
    }

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