global using static LBMergedMods.Hooks.ShortcutHooks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Watcher;

namespace LBMergedMods.Hooks;

public static class ShortcutHooks
{
    internal static bool On_ShortcutHelper_PopsOutOfDeadShortcuts(On.ShortcutHelper.orig_PopsOutOfDeadShortcuts orig, ShortcutHelper self, PhysicalObject physicalObject) => (physicalObject.grabbedBy.Count == 0 && physicalObject is HazerMom) || orig(self, physicalObject);

    internal static Color On_ShortcutGraphics_ShortCutColor(On.ShortcutGraphics.orig_ShortCutColor orig, ShortcutGraphics self, Creature crit, IntVector2 pos)
    {
        var color = orig(self, crit, pos);
        if (crit is WaterSpitter ws)
        {
            if (((!ModManager.MMF || !MMF.cfgShowUnderwaterShortcuts.Value) && self.room is Room rm && rm.water && pos.y < rm.DefaultWaterLevel(pos) && rm.GetTile(pos).Terrain != Room.Tile.TerrainType.ShortcutEntrance && (self.DisplayLayer(pos.x, pos.y) > 0 || rm.waterInFrontOfTerrain)) || (ModManager.MMF && color.grayscale < .15f))
                return color;
            color = Color.Lerp(self.palette.waterSurfaceColor1, Color.white, .1f);
            if (ws.rotModule is LizardRotModule mod && ws.LizardState is LizardState st && st.rotType != LizardState.RotType.Slight)
                color = Color.Lerp(color, mod.RotEyeColor, st.rotType == LizardState.RotType.Opossum ? .2f : .8f);
        }
        return color;
    }

    internal static void IL_ShortcutHandler_FlyingCreatureArrivedInRealizedRoom(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_ShortcutHandler_Vessel_creature,
            s_MatchCallOrCallvirt_Creature_get_abstractCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Vulture,
            s_MatchCall_Any,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((ShortcutHandler.BorderVessel self) => self.creature is FatFireFly);
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ShortcutHandler.FlyingCreatureArrivedInRealizedRoom!");
    }
}