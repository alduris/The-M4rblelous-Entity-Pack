global using static LBMergedMods.Hooks.SporeCloudHooks;
using MonoMod.Cil;

namespace LBMergedMods.Hooks;

public static class SporeCloudHooks
{
    internal static void IL_SporeCloud_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchIsinst_InsectoidCreature))
            c.EmitDelegate((InsectoidCreature? cr) => cr is Sporantula ? null : cr);
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SporeCloud.Update!");
    }
}