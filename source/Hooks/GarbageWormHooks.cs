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
        int loc1 = 0, loc2 = 0;
        if (c.TryGotoNext(
            x => x.MatchLdloc(out loc1),
            x => x.MatchLdfld<GarbageWormAI.CreatureInterest>("crit"),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchCallOrCallvirt<CreatureTemplate>("get_IsVulture"),
            x => x.MatchBrfalse(out _),
            x => x.MatchLdcR4(1000f),
            x => x.MatchStloc(out loc2)))
        {
            var l2 = il.Body.Variables[loc2];
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc1])
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