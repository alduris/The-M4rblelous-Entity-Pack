global using static LBMergedMods.Hooks.ShortcutHooks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class ShortcutHooks
{
    internal static bool On_ShortcutHelper_PopsOutOfDeadShortcuts(On.ShortcutHelper.orig_PopsOutOfDeadShortcuts orig, ShortcutHelper self, PhysicalObject physicalObject) => (physicalObject.grabbedBy.Count == 0 && physicalObject is HazerMom) || orig(self, physicalObject);

    internal static Color On_ShortcutGraphics_ShortCutColor(On.ShortcutGraphics.orig_ShortCutColor orig, ShortcutGraphics self, Creature crit, IntVector2 pos)
    {
        var color = orig(self, crit, pos);
        if (crit.Template.type == CreatureTemplateType.WaterSpitter && self.room is Room rm)
        {
            if (((!ModManager.MMF || !MMF.cfgShowUnderwaterShortcuts.Value) && rm.water && pos.y < rm.DefaultWaterLevel(pos) && rm.GetTile(pos).Terrain != Room.Tile.TerrainType.ShortcutEntrance && (self.DisplayLayer(pos.x, pos.y) > 0 || rm.waterInFrontOfTerrain)) || (ModManager.MMF && color.grayscale < .15f))
                return color;
            color = Color.Lerp(self.palette.waterSurfaceColor1, Color.white, .1f);
        }
        return color;
    }

    internal static void IL_ShortcutHandler_FlyingCreatureArrivedInRealizedRoom(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<ShortcutHandler.Vessel>("creature"),
            x => x.MatchCallOrCallvirt<Creature>("get_abstractCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Vulture"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((ShortcutHandler.BorderVessel self) => self.creature is FatFireFly);
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ShortcutHandler.FlyingCreatureArrivedInRealizedRoom!");
    }
}