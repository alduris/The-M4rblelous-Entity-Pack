global using static LBMergedMods.Hooks.SporeCloudHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class SporeCloudHooks
{
    internal static void IL_SporeCloud_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_UpdatableAndDeletable_room,
            s_MatchCallOrCallvirt_Room_get_abstractRoom,
            s_MatchLdfld_AbstractRoom_creatures,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Any,
            s_MatchCallOrCallvirt_AbstractCreature_get_realizedCreature,
            s_MatchIsinst_InsectoidCreature,
            s_MatchBrfalse_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((SporeCloud self, int index) => self.room.abstractRoom.creatures[index].creatureTemplate.type == CreatureTemplateType.Sporantula);
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SporeCloud.Update!");
    }
}