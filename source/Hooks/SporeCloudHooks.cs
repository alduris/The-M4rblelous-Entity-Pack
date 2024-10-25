global using static LBMergedMods.Hooks.SporeCloudHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class SporeCloudHooks
{
    internal static void IL_SporeCloud_Update(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        var local = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
            x => x.MatchCallOrCallvirt<Room>("get_abstractRoom"),
            x => x.MatchLdfld<AbstractRoom>("creatures"),
            x => x.MatchLdloc(out local),
            x => x.MatchCallOrCallvirt(out _),
            x => x.MatchCallOrCallvirt<AbstractCreature>("get_realizedCreature"),
            x => x.MatchIsinst<InsectoidCreature>(),
            x => x.MatchBrfalse(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, il.Body.Variables[local])
             .EmitDelegate((SporeCloud self, int index) => self.room?.abstractRoom?.creatures?[index]?.creatureTemplate.type == CreatureTemplateType.Sporantula);
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SporeCloud.Update!");
    }
}