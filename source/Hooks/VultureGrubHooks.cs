﻿global using static LBMergedMods.Hooks.VultureGrubHooks;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using UnityEngine;

namespace LBMergedMods.Hooks;
//CHK
public static class VultureGrubHooks
{
    internal static void IL_VultureGrub_AttemptCallVulture(ILContext il)
    {
        var vars = il.Body.Variables;
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdsfld_CreatureTemplate_Type_Vulture,
            s_MatchCallOrCallvirt_AbstractRoom_AttractionForCreature,
            s_MatchLdsfld_AbstractRoom_CreatureRoomAttraction_Forbidden,
            s_MatchCallOrCallvirt_Any))
        {
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .EmitDelegate((bool flag, AbstractRoom rm) => flag && rm.AttractionForCreature(CreatureTemplateType.FlyingBigEel) == AbstractRoom.CreatureRoomAttraction.Forbidden && rm.AttractionForCreature(CreatureTemplateType.MiniFlyingBigEel) == AbstractRoom.CreatureRoomAttraction.Forbidden && rm.AttractionForCreature(CreatureTemplateType.FatFireFly) == AbstractRoom.CreatureRoomAttraction.Forbidden);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook VultureGrub.AttemptCallVulture (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchStloc_OutLoc1,
            s_MatchBr_Any)
        && c.TryGotoNext(MoveType.After,
            s_MatchLdcI4_0,
            s_MatchStloc_Any))
        {
            VariableDefinition l;
            c.Emit(OpCodes.Ldloc, l = vars[s_loc1])
             .EmitDelegate((CreatureTemplate.Type[] array) =>
             {
                 var l = array.Length;
                 Array.Resize(ref array, l + 3);
                 array[l] = CreatureTemplateType.FlyingBigEel;
                 array[l + 1] = CreatureTemplateType.MiniFlyingBigEel;
                 array[l + 2] = CreatureTemplateType.FatFireFly;
                 return array;
             });
            c.Emit(OpCodes.Stloc, l);
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