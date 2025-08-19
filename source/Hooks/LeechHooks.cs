global using static LBMergedMods.Hooks.LeechHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class LeechHooks
{
    internal static void IL_Leech_Swim(ILContext il)
    {
        var vars = il.Body.Variables;
        var label = il.DefineLabel();
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_Leech_school,
            s_MatchLdfld_Leech_LeechSchool_prey,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_OutRef,
            s_MatchLdfld_Leech_LeechSchool_LeechPrey_creature,
            s_MatchCallOrCallvirt_Creature_get_Template,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Snail,
            s_MatchCall_Any,
            s_MatchBrfalse_Any))
        {
            label.Target = c.Next;
            c.Index -= 10;
            c.Emit<Leech>(OpCodes.Ldfld, "school")
             .Emit<Leech.LeechSchool>(OpCodes.Ldfld, "prey")
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Callvirt, s_ref)
             .Emit<Leech.LeechSchool.LeechPrey>(OpCodes.Ldfld, "creature")
             .Emit(OpCodes.Isinst, il.Import(typeof(BouncingBall)))
             .Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Leech.Swim! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Creature_get_abstractCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Leech,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .EmitDelegate((bool flag, Creature realizedCreature) => flag && realizedCreature is not MiniLeech);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Leech.Swim! (part 2)");
    }
}