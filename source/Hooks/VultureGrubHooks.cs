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
                 if (Array.IndexOf(array, CreatureTemplateType.FlyingBigEel) == -1)
                 {
                     Array.Resize(ref array, array.Length + 1);
                     array[array.Length - 1] = CreatureTemplateType.FlyingBigEel;
                 }
                 if (Array.IndexOf(array, CreatureTemplateType.MiniFlyingBigEel) == -1)
                 {
                     Array.Resize(ref array, array.Length + 1);
                     array[array.Length - 1] = CreatureTemplateType.MiniFlyingBigEel;
                 }
                 if (Array.IndexOf(array, CreatureTemplateType.FatFireFly) == -1)
                 {
                     Array.Resize(ref array, array.Length + 1);
                     array[array.Length - 1] = CreatureTemplateType.FatFireFly;
                 }
                 return array;
             });
            c.Emit(OpCodes.Stloc, loc2);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook VultureGrub.AttemptCallVulture (part 2)!");
    }

    internal static bool On_VultureGrub_RayTraceSky(On.VultureGrub.orig_RayTraceSky orig, VultureGrub self, Vector2 testDir) => orig(self, testDir) && self.room.abstractRoom.AttractionForCreature(CreatureTemplateType.FlyingBigEel) != AbstractRoom.CreatureRoomAttraction.Forbidden && self.room.abstractRoom.AttractionForCreature(CreatureTemplateType.MiniFlyingBigEel) != AbstractRoom.CreatureRoomAttraction.Forbidden && self.room.abstractRoom.AttractionForCreature(CreatureTemplateType.FatFireFly) != AbstractRoom.CreatureRoomAttraction.Forbidden;
}