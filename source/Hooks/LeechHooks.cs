global using static LBMergedMods.Hooks.LeechHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace LBMergedMods.Hooks;

public static class LeechHooks
{
    internal static void IL_Leech_Swim(ILContext il)
    {
        MethodReference? ref1 = null;
        var label = il.DefineLabel();
        var c = new ILCursor(il);
        var loc = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Leech>("school"),
            x => x.MatchLdfld<Leech.LeechSchool>("prey"),
            x => x.MatchLdloc(out loc),
            x => x.MatchCallOrCallvirt(out ref1),
            x => x.MatchLdfld<Leech.LeechSchool.LeechPrey>("creature"),
            x => x.MatchCallOrCallvirt<Creature>("get_Template"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Snail"),
            x => x.MatchCall(out _),
            x => x.MatchBrfalse(out _))
        && ref1 is not null)
        {
            label.Target = c.Next;
            c.Index -= 10;
            c.Emit<Leech>(OpCodes.Ldfld, "school")
             .Emit<Leech.LeechSchool>(OpCodes.Ldfld, "prey")
             .Emit(OpCodes.Ldloc, il.Body.Variables[loc])
             .Emit(OpCodes.Callvirt, ref1)
             .Emit<Leech.LeechSchool.LeechPrey>(OpCodes.Ldfld, "creature")
             .Emit(OpCodes.Isinst, il.Import(typeof(BouncingBall)))
             .Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Leech.Swim! (part 1)");
        loc = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out loc),
            x => x.MatchCallOrCallvirt<Creature>("get_abstractCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Leech"),
            x => x.MatchCall(out _)))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc])
             .EmitDelegate((bool flag, Creature realizedCreature) => flag && realizedCreature is not MiniLeech);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Leech.Swim! (part 2)");
    }
}