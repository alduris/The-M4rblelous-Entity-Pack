global using static LBMergedMods.Hooks.FirecrackerPlantHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class FirecrackerPlantHooks
{
    internal static void IL_ScareObject_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((bool flag, FirecrackerPlant.ScareObject self, int i) => flag && self.room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook FirecrackerPlant.ScareObject.Update!");
    }
}