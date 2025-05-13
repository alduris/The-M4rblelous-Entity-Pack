global using static LBMergedMods.Hooks.SporePlantHooks;

namespace LBMergedMods.Hooks;
//CHK
public static class SporePlantHooks
{
    internal static void On_SporePlant_HitByWeapon(On.SporePlant.orig_HitByWeapon orig, SporePlant self, Weapon weapon)
    {
        if (weapon is not SmallPuffBall or FumeFruit)
            orig(self, weapon);
    }

    internal static bool On_SporePlant_SporePlantInterested(On.SporePlant.orig_SporePlantInterested orig, CreatureTemplate.Type tp) => tp != CreatureTemplateType.Xylo && tp != CreatureTemplateType.Denture && orig(tp);
}