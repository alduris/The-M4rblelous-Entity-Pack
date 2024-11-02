global using static LBMergedMods.Hooks.ExpeditionHooks;
using System.Collections.Generic;

namespace LBMergedMods.Hooks;

public static class ExpeditionHooks
{
    internal static void On_ChallengeTools_CreatureName(On.Expedition.ChallengeTools.orig_CreatureName orig, ref string?[] creatureNames)
    {
        orig(ref creatureNames);
        creatureNames[(int)CreatureTemplateType.MiniBlackLeech] = null;
    }

    internal static void On_ChallengeTools_GenerateCreatureScores(On.Expedition.ChallengeTools.orig_GenerateCreatureScores orig, ref Dictionary<string, int> dict)
    {
        orig(ref dict);
        if (dict.ContainsKey("MiniBlackLeech"))
            dict.Remove("MiniBlackLeech");
    }
}