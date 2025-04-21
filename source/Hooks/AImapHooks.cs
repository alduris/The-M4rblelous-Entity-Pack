global using static LBMergedMods.Hooks.AImapHooks;
using RWCustom;

namespace LBMergedMods.Hooks;
//CHK
public static class AImapHooks
{
    internal static int On_AImap_ExitDistanceForCreatureAndCheckNeighbours(On.AImap.orig_ExitDistanceForCreatureAndCheckNeighbours orig, AImap self, IntVector2 pos, int creatureSpecificExitIndex, CreatureTemplate crit)
    {
        if (crit.PreBakedPathingIndex < 0 || crit.PreBakedPathingIndex >= self.creatureSpecificAImaps?.Length)
            return -1;
        return orig(self, pos, creatureSpecificExitIndex, crit);
    }
}