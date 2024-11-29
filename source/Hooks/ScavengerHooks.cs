global using static LBMergedMods.Hooks.ScavengerHooks;
using MonoMod.Cil;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class ScavengerHooks
{
    internal static void On_Scavenger_MeleeGetFree(On.Scavenger.orig_MeleeGetFree orig, Scavenger self, Creature target, bool eu)
    {
        if (self.grabbedBy is List<Creature.Grasp> l && l.Count > 0 && l[0].grabber is FatFireFly c && c == target && self.grabbedAttackCounter != 25 && !(self.Elite && self.grabbedAttackCounter % 75 == 0))
            return;
        orig(self, target, eu);
    }

    internal static int On_ScavengerAI_CollectScore_PhysicalObject_bool(On.ScavengerAI.orig_CollectScore_PhysicalObject_bool orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
    {
        var res = orig(self, obj, weaponFiltered);
        if (obj is ThornyStrawberry st)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (weaponFiltered && self.NeedAWeapon)
                res = self.WeaponScore(st, true);
            else
                res = st.SpikesRemoved() ? 1 : 3;
        }
        else if (obj is SmallPuffBall pf)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (weaponFiltered && self.NeedAWeapon)
                res = self.WeaponScore(pf, true);
            else
                res = 2;
        }
        else if (obj is BlobPiece or Physalis or LimeMushroom or MarineEye or StarLemon)
        {
            if (self.scavenger.room is Room rm)
            {
                var ownedItemOnGround = rm.socialEventRecognizer.ItemOwnership(obj);
                if (ownedItemOnGround is not null && ownedItemOnGround.offeredTo is not null && ownedItemOnGround.offeredTo != self.scavenger)
                    return 0;
            }
            if (!(weaponFiltered && self.NeedAWeapon))
                res = obj is Physalis ? 5 : (obj is LimeMushroom or StarLemon ? 3 : 2);
        }
        return res;
    }

    internal static int On_ScavengerAI_WeaponScore(On.ScavengerAI.orig_WeaponScore orig, ScavengerAI self, PhysicalObject obj, bool pickupDropInsteadOfWeaponSelection)
    {
        var res = orig(self, obj, pickupDropInsteadOfWeaponSelection);
        if (obj is ThornyStrawberry st)
        {
            if (st.SpikesRemoved())
                res = 0;
            else
            {
                if (!pickupDropInsteadOfWeaponSelection && (self.currentViolenceType == ScavengerAI.ViolenceType.NonLethal || self.currentViolenceType == ScavengerAI.ViolenceType.ForFun))
                    res = 2;
                else
                    res = 3;
            }
        }
        else if (obj is SmallPuffBall)
            res = 2;
        return res;
    }

    internal static void IL_ScavengerAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_PoleMimic,
            s_MatchCall_Any,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((RelationshipTracker.DynamicRelationship dRelation) => dRelation.trackerRep?.representedCreature?.creatureTemplate.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.Denture || tp == CreatureTemplateType.Scutigera || tp == CreatureTemplateType.RedHorrorCenti || tp == CreatureTemplateType.Sporantula));
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_MirosBird,
            s_MatchCall_Any,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((RelationshipTracker.DynamicRelationship dRelation) => dRelation.trackerRep?.representedCreature?.creatureTemplate.type == CreatureTemplateType.Blizzor);
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerAI.IUseARelationshipTracker.UpdateDynamicRelationship! (part 2)");
    }

    internal static void IL_WorldFloodFiller_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var vars = il.Body.Variables;
        if (c.TryGotoNext(
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_World_GetAbstractRoom_WorldCoordinate,
            s_MatchLdfld_AbstractRoom_connections,
            s_MatchLdloc_InLoc1,
            s_MatchLdfld_WorldCoordinate_abstractNode,
            s_MatchLdelemI4,
            s_MatchLdcI4_M1,
            s_MatchBle_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .EmitDelegate((ScavengersWorldAI.WorldFloodFiller self, WorldCoordinate worldCoordinate) => worldCoordinate.abstractNode >= 0 && worldCoordinate.abstractNode < self.world.GetAbstractRoom(worldCoordinate).connections.Length);
            c.Emit(OpCodes.Brfalse, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerWorldAI.WorldFloodFiller.Update! (part 1)");
        if (c.TryGotoNext(
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix,
            s_MatchLdloc_OutLoc2,
            s_MatchLdfld_WorldCoordinate_room,
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world,
            s_MatchCallOrCallvirt_World_get_firstRoomIndex,
            s_MatchSub,
            s_MatchLdelemRef,
            s_MatchLdloc_InLoc2,
            s_MatchLdfld_WorldCoordinate_abstractNode,
            s_MatchLdelemU1,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((ScavengersWorldAI.WorldFloodFiller self, WorldCoordinate item) =>
             {
                 var index = item.room - self.world.firstRoomIndex;
                 return index >= 0 && index < self.nodesMatrix.Length && index < self.roomsMatrix.Length && item.abstractNode >= 0 && item.abstractNode < self.nodesMatrix[index].Length;
             });
            c.Emit(OpCodes.Brfalse, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerWorldAI.WorldFloodFiller.Update! (part 2)");
        if (c.TryGotoNext(
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_nodesMatrix,
            s_MatchLdloc_InLoc1,
            s_MatchLdfld_WorldCoordinate_room,
            s_MatchLdarg_0,
            s_MatchLdfld_ScavengersWorldAI_WorldFloodFiller_world,
            s_MatchCallOrCallvirt_World_get_firstRoomIndex,
            s_MatchSub,
            s_MatchLdelemRef,
            s_MatchLdloc_OutLoc2,
            s_MatchLdelemU1,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((ScavengersWorldAI.WorldFloodFiller self, WorldCoordinate worldCoordinate, int i) =>
             {
                 var index = worldCoordinate.room - self.world.firstRoomIndex;
                 return index >= 0 && index < self.nodesMatrix.Length && index < self.roomsMatrix.Length && i < self.nodesMatrix[index].Length;
             });
            c.Emit(OpCodes.Brfalse, s_label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook ScavengerWorldAI.WorldFloodFiller.Update! (part 3)");
    }
}