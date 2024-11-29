global using static LBMergedMods.Hooks.GarbageWormHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class GarbageWormHooks
{
    internal static Tracker.CreatureRepresentation? On_GarbageWormAI_CreateTrackerRepresentationForCreature(On.GarbageWormAI.orig_CreateTrackerRepresentationForCreature orig, GarbageWormAI self, AbstractCreature otherCreature)
    {
        if (otherCreature.creatureTemplate.type == CreatureTemplateType.MiniBlackLeech)
            return null;
        return orig(self, otherCreature);
    }

    internal static void IL_GarbageWormAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var vars = il.Body.Variables;
        if (c.TryGotoNext(
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_GarbageWormAI_CreatureInterest_crit,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchCallOrCallvirt_CreatureTemplate_get_IsVulture,
            s_MatchBrfalse_Any,
            s_MatchLdcR4_1000,
            s_MatchStloc_OutLoc2))
        {
            var l2 = vars[s_loc2];
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldloc, l2)
             .EmitDelegate((GarbageWormAI.CreatureInterest interest, float num) =>
             {
                 if (interest.crit.representedCreature.creatureTemplate.type == CreatureTemplateType.FlyingBigEel)
                     return 1000f;
                 return num;
             });
            c.Emit(OpCodes.Stloc, l2);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook GarbageWormAI.Update!");
    }
}