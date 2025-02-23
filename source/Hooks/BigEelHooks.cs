global using static LBMergedMods.Hooks.BigEelHooks;
using MonoMod.Cil;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using RWCustom;
using System;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class BigEelHooks
{
    public static int _MiniLeviColorA, _MiniLeviColorB, _MiniLeviColorHead, _AMiniLeviColorA, _AMiniLeviColorB, _AMiniLeviColorHead, _GRJLeviathanColorA, _GRJLeviathanColorB, _GRJLeviathanColorHead, _GRJMiniLeviathanColorA, _GRJMiniLeviathanColorB, _GRJMiniLeviathanColorHead;

    internal static void IL_BigEel_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdcI4_20,
            s_MatchNewarr_BodyChunk))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((BodyChunk[] ar, BigEel self) => self is FlyingBigEel ? new BodyChunk[10] : ar);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEel.ctor!");
    }

    internal static void IL_BigEel_AccessSwimSpace(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdsfld_AbstractRoomNode_Type_SeaExit))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEel self) => self is FlyingBigEel or MiniFlyingBigEel ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEel.AccessSwimSpace!");
    }

    internal static void On_BigEel_Act(On.BigEel.orig_Act orig, BigEel self, bool eu)
    {
        if (self is MiniLeviathan or MiniFlyingBigEel && self.jawCharge > 0f && self.jawChargeFatigue <= 0f && (self.jawCharge > .3f || (self.jawChargeFatigue < 1f && self.AI?.WantToChargeJaw() is true)))
            self.jawCharge += 1f / 140f;
        orig(self, eu);
    }

    internal static void On_BigEel_Crush(On.BigEel.orig_Crush orig, BigEel self, PhysicalObject obj)
    {
        orig(self, obj);
        if (self is MiniLeviathan)
        {
            if (self.room is Room rm)
            {
                var mbcPos = self.mainBodyChunk.pos;
                for (var i = 0; i < 10; i++)
                {
                    rm.AddObject(new WaterDrip(mbcPos, new Vector2(Random.value, Random.value).normalized, false));
                    rm.AddObject(new Bubble(mbcPos, new Vector2(Random.value, Random.value).normalized, false, false));
                }
            }
            obj.Destroy();
        }
        else if (self is MiniFlyingBigEel)
        {
            var mbcPos = self.mainBodyChunk.pos;
            if (self.room is Room rm)
            {
                for (var i = 0; i < 10; i++)
                    rm.AddObject(new WaterDrip(mbcPos, new Vector2(Random.value, Random.value).normalized, false));
            }
            obj.Destroy();
        }
    }

    internal static bool On_BigEel_InBiteArea(On.BigEel.orig_InBiteArea orig, BigEel self, Vector2 pos, float margin)
    {
        if (self is MiniLeviathan or MiniFlyingBigEel)
        {
            var mbcPos = self.mainBodyChunk.pos;
            var vector = Custom.DirVec(self.bodyChunks[1].pos, mbcPos);
            if (!Custom.DistLess(mbcPos + vector * 45f, pos, 14f + margin))
                return false;
            Vector2 pos2 = mbcPos,
                vector2 = Custom.PerpendicularVector(vector);
            if (Math.Abs(Custom.DistanceToLine(pos, pos2 + 60f * vector - vector2, pos2 + 60f * vector + vector2)) > 30f + margin)
                return false;
            if (Math.Abs(Custom.DistanceToLine(pos, pos2 - vector, pos2 + vector)) > 20f + margin)
                return false;
        }
        return orig(self, pos, margin);
    }

    internal static void IL_BigEel_JawsSnap(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_UpdatableAndDeletable_room,
            s_MatchLdsfld_SoundID_Leviathan_Bite))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((SoundID ID, BigEel self) => self is FlyingBigEel or MiniFlyingBigEel ? NewSoundID.Flying_Leviathan_Bite : ID);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEel.JawsSnap!");
    }

    internal static void IL_BigEel_Swim(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            var ins = instrs[i];
            if (ins.MatchCallOrCallvirt<BodyChunk>("get_submersion"))
            {
                c.Goto(ins, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((float submersion, BigEel self) => self is FlyingBigEel or MiniFlyingBigEel ? 1f : submersion);
            }
        }
    }

    internal static void IL_BigEelAbstractAI_AbstractBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_world,
            s_MatchLdfld_World_seaAccessNodes,
            s_MatchLdlen))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int length, BigEelAbstractAI self) => self.parent?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self.RealAI is MiniFlyingBigEelAI ? self.world.skyAccessNodes.Length : length);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AbstractBehavior!");
    }

    internal static void IL_BigEelAbstractAI_AddRandomCheckRoom(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_world,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_world,
            s_MatchCallOrCallvirt_World_get_firstRoomIndex,
            s_MatchCallOrCallvirt_World_GetAbstractRoom_int,
            s_MatchLdfld_AbstractRoom_nodes,
            s_MatchLdloc_Any,
            s_MatchLdelema_AbstractRoomNode,
            s_MatchLdfld_AbstractRoomNode_type,
            s_MatchLdsfld_AbstractRoomNode_Type_SeaExit))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEelAbstractAI self) => self.parent?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self.RealAI is MiniFlyingBigEelAI ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRandomCheckRoom (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_world,
            s_MatchLdfld_World_seaAccessNodes,
            s_MatchLdcI4_0,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_world,
            s_MatchLdfld_World_seaAccessNodes,
            s_MatchLdlen,
            s_MatchConvI4,
            s_MatchCall_Random_Range_int_int,
            s_MatchLdelemAny_WorldCoordinate))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((WorldCoordinate coord, BigEelAbstractAI self) => self.parent?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self.RealAI is MiniFlyingBigEelAI ? self.world.skyAccessNodes[Random.Range(0, self.world.skyAccessNodes.Length)] : coord);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRandomCheckRoom (part 2)!");
    }

    internal static void IL_BigEelAbstractAI_AddRoomClusterToCheckList(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_BigEel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CreatureTemplate.Type type, BigEelAbstractAI self) => self.parent?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel ? CreatureTemplateType.FlyingBigEel : type);
            if (c.TryGotoNext(MoveType.After,
                s_MatchCall_op_Equality_Any))
            {
                c.Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, BigEelAbstractAI self) => self.RealAI is not MiniLeviathanAI and not MiniFlyingBigEelAI && flag);
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRoomClusterToCheckList! (part 2)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRoomClusterToCheckList! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_Any,
            s_MatchLdfld_AbstractRoom_nodes,
            s_MatchLdloc_Any,
            s_MatchLdelema_AbstractRoomNode,
            s_MatchLdfld_AbstractRoomNode_type,
            s_MatchLdsfld_AbstractRoomNode_Type_SeaExit))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEelAbstractAI self) => self.parent?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self.RealAI is MiniFlyingBigEelAI ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRoomClusterToCheckList (part 3)!");
    }

    internal static void IL_BigEelAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var inst = ins[i];
            if (inst.MatchLdfld<Room>("defaultWaterLevel"))
            {
                c.Goto(inst, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((int defaultLevel, BigEelAI self) => (self.creature?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self is MiniFlyingBigEelAI) && self.eel?.room is Room r ? r.TileHeight : defaultLevel);
            }
        }
    }

    internal static bool On_BigEelAI_WantToChargeJaw(On.BigEelAI.orig_WantToChargeJaw orig, BigEelAI self)
    {
        if (self is MiniLeviathanAI or MiniFlyingBigEelAI && self.eel is BigEel be && !be.safariControlled)
        {
            if (self.behavior == BigEelAI.Behavior.Hunt && self.focusCreature is Tracker.CreatureRepresentation r && be.room is Room rm)
                return Custom.DistLess(be.mainBodyChunk.pos, rm.MiddleOfTile(r.BestGuessForPosition()), 140f);
            return false;
        }
        return orig(self);
    }

    internal static void IL_BigEelAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            var ins = instrs[i];
            if (ins.MatchLdfld<Room>("defaultWaterLevel"))
            {
                c.Goto(ins, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((int defaultLevel, BigEelAI self) => self.creature is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self is MiniFlyingBigEelAI) && self.eel?.room is Room r ? r.TileHeight : defaultLevel);
            }
        }
        c.Index = il.Body.Instructions.Count - 1;
        c.Emit(OpCodes.Ldarg_0)
         .Emit(OpCodes.Ldarg_1)
         .EmitDelegate((CreatureTemplate.Relationship rel, BigEelAI self, RelationshipTracker.DynamicRelationship dRelation) =>
         {
             if (rel.type == CreatureTemplate.Relationship.Type.Eats && (self.creature?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self is MiniFlyingBigEelAI) && self.eel is BigEel be && be.antiStrandingZones is List<PlacedObject> list && list.Count > 0 && dRelation.trackerRep?.representedCreature?.realizedCreature?.mainBodyChunk is BodyChunk b)
             {
                 for (var j = 0; j < list.Count; j++)
                 {
                     if (Custom.DistLess(b.pos, list[j].pos, 100f))
                     {
                         rel.type = CreatureTemplate.Relationship.Type.Ignores;
                         rel.intensity = 0f;
                         break;
                     }
                 }
             }
             return rel;
         });
    }

    internal static void IL_BigEelGraphics_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdcI4_60,
            s_MatchNewarr_TailSegment))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((TailSegment[] ar, BigEelGraphics self) => self is FlyingBigEelGraphics ? new TailSegment[45] : ar);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ctor!");
    }

    internal static void IL_BigEelGraphics_ApplyPalette(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_RainWorld_ShadPropLeviathanColorA))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int i, BigEelGraphics g) =>
             {
                 if (g is MiniLeviathanGraphics)
                 {
                     if (g.eel?.abstractCreature.superSizeMe is true)
                         return _MiniLeviColorA;
                     return _AMiniLeviColorA;
                 }
                 if (g is FlyingBigEelGraphics)
                     return _GRJLeviathanColorA;
                 if (g is MiniFlyingBigEelGraphics)
                     return _GRJMiniLeviathanColorA;
                 return i;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ApplyPalette (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_RainWorld_ShadPropLeviathanColorB))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int i, BigEelGraphics g) =>
             {
                 if (g is MiniLeviathanGraphics)
                 {
                     if (g.eel?.abstractCreature.superSizeMe is true)
                         return _MiniLeviColorB;
                     return _AMiniLeviColorB;
                 }
                 if (g is FlyingBigEelGraphics)
                     return _GRJLeviathanColorB;
                 if (g is MiniFlyingBigEelGraphics)
                     return _GRJMiniLeviathanColorB;
                 return i;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ApplyPalette (part 2)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_RainWorld_ShadPropLeviathanColorHead))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int i, BigEelGraphics g) =>
             {
                 if (g is MiniLeviathanGraphics)
                 {
                     if (g.eel?.abstractCreature.superSizeMe is true)
                         return _MiniLeviColorHead;
                     return _AMiniLeviColorHead;
                 }
                 if (g is FlyingBigEelGraphics)
                     return _GRJLeviathanColorHead;
                 if (g is MiniFlyingBigEelGraphics)
                     return _GRJMiniLeviathanColorHead;
                 return i;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ApplyPalette (part 3)!");
    }

    internal static void IL_BigEelGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        c.Emit(OpCodes.Ldarg_0)
         .EmitDelegate((BigEelGraphics self) =>
         {
             if (self is FlyingBigEelGraphics or MiniFlyingBigEelGraphics && self.finSound is StaticSoundLoop snd)
                 snd.volume = 0f;
         });
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_BigEelGraphics_eel,
            s_MatchLdfld_BigEel_swimSpeed,
            s_MatchCall_Mathf_Lerp,
            s_MatchDiv,
            s_MatchSub,
            s_MatchStfld_BigEelGraphics_tailSwim))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((BigEelGraphics self) =>
             {
                 if (self is FlyingBigEelGraphics or MiniFlyingBigEelGraphics)
                     self.tailSwim /= 2f;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.Update!");
        var instrs = il.Instrs;
        for (var i = 0; i < instrs.Count; i++)
        {
            var ins = instrs[i];
            if (ins.MatchCallOrCallvirt<Room>("PointSubmerged"))
            {
                c.Goto(ins, MoveType.After);
                if (i != 0)
                {
                    c.Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool sub, BigEelGraphics self) => self is FlyingBigEelGraphics or MiniFlyingBigEelGraphics || sub);
                }
            }
        }
    }

    internal static void IL_BigEelPather_FollowPath(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_RoomBorderExit_type,
            s_MatchLdsfld_AbstractRoomNode_Type_SeaExit))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEelPather self) => self.creature?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self.AI is MiniFlyingBigEelAI ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelPather.FollowPath (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_PathFinder_world,
            s_MatchLdfld_World_seaAccessNodes))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((WorldCoordinate[] accessNodes, BigEelPather self) => self.creature?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || self.AI is MiniFlyingBigEelAI ? self.world.skyAccessNodes : accessNodes);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelPather.FollowPath (part 2)!");
    }
}