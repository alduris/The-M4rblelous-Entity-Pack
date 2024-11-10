global using static LBMergedMods.Hooks.VultureGrubHooks;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class VultureGrubHooks
{
    internal static void IL_VultureGrub_AttemptCallVulture(ILContext il)
    {
        var c = new ILCursor(il);
        var loc1 = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out loc1),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Vulture"),
            x => x.MatchCallOrCallvirt<AbstractRoom>("AttractionForCreature"),
            x => x.MatchLdsfld<AbstractRoom.CreatureRoomAttraction>("Forbidden"),
            x => x.MatchCallOrCallvirt(out _)))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc1])
             .EmitDelegate((bool flag, AbstractRoom rm) => flag && rm.AttractionForCreature(CreatureTemplateType.FlyingBigEel) == AbstractRoom.CreatureRoomAttraction.Forbidden && rm.AttractionForCreature(CreatureTemplateType.MiniFlyingBigEel) == AbstractRoom.CreatureRoomAttraction.Forbidden && rm.AttractionForCreature(CreatureTemplateType.FatFireFly) == AbstractRoom.CreatureRoomAttraction.Forbidden);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook VultureGrub.AttemptCallVulture (part 1)!");
        var loc2 = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchStloc(out loc2),
            x => x.MatchBr(out _))
        && c.TryGotoNext(MoveType.After,
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(out _)))
        {
            c.Emit(OpCodes.Ldloc, loc2)
             .EmitDelegate((CreatureTemplate.Type[] array) =>
             {
                 var l = array.Length;
                 Array.Resize(ref array, l + 3);
                 array[l] = CreatureTemplateType.FlyingBigEel;
                 array[l + 1] = CreatureTemplateType.MiniFlyingBigEel;
                 array[l + 2] = CreatureTemplateType.FatFireFly;
                 return array;
             });
            c.Emit(OpCodes.Stloc, loc2);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook VultureGrub.AttemptCallVulture (part 2)!");
    }

    internal static bool On_VultureGrub_RayTraceSky(On.VultureGrub.orig_RayTraceSky orig, VultureGrub self, Vector2 testDir)
    {
        var abRm = self.room.abstractRoom;
        return abRm.AttractionForCreature(CreatureTemplateType.FlyingBigEel) != AbstractRoom.CreatureRoomAttraction.Forbidden && abRm.AttractionForCreature(CreatureTemplateType.MiniFlyingBigEel) != AbstractRoom.CreatureRoomAttraction.Forbidden && abRm.AttractionForCreature(CreatureTemplateType.FatFireFly) != AbstractRoom.CreatureRoomAttraction.Forbidden && orig(self, testDir);
    }
}