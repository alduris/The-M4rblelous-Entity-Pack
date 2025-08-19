global using static LBMergedMods.Hooks.ExpeditionHooks;
using System.Collections.Generic;

namespace LBMergedMods.Hooks;

public static class ExpeditionHooks
{
    internal static void On_ChallengeTools_CreatureName(On.Expedition.ChallengeTools.orig_CreatureName orig, ref string?[] creatureNames)
    {
        orig(ref creatureNames);
        creatureNames[(int)CreatureTemplateType.MiniBlackLeech] = null;
        creatureNames[(int)CreatureTemplateType.Denture] = null;
    }

    internal static void On_ChallengeTools_GenerateCreatureScores(On.Expedition.ChallengeTools.orig_GenerateCreatureScores orig, ref Dictionary<string, int> dict)
    {
        orig(ref dict);
        if (dict.ContainsKey(nameof(CreatureTemplateType.MiniBlackLeech)))
            dict.Remove(nameof(CreatureTemplateType.MiniBlackLeech));
        if (dict.ContainsKey(nameof(CreatureTemplateType.Denture)))
            dict.Remove(nameof(CreatureTemplateType.Denture));
    }
}