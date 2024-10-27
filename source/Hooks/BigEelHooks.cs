global using static LBMergedMods.Hooks.BigEelHooks;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using RWCustom;
using Mono.Cecil;
using Random = UnityEngine.Random;
using System.Security.Cryptography;
using System.Linq;

namespace LBMergedMods.Hooks;

public static class BigEelHooks
{
    public static int _MiniLeviColorA, _MiniLeviColorB, _MiniLeviColorHead, _AMiniLeviColorA, _AMiniLeviColorB, _AMiniLeviColorHead, _GRJLeviathanColorA, _GRJLeviathanColorB, _GRJLeviathanColorHead, _GRJMiniLeviathanColorA, _GRJMiniLeviathanColorB, _GRJMiniLeviathanColorHead;

    internal static void IL_BigEel_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(20),
            x => x.MatchNewarr<BodyChunk>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((BodyChunk[] ar, BigEel self) => self.Template.type == CreatureTemplateType.FlyingBigEel ? new BodyChunk[10] : ar);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEel.ctor!");
        c.Index = il.Body.Instructions.Count - 1;
        c.Emit(OpCodes.Ldarg_0)
         .EmitDelegate((BigEel self) =>
         {
             if (self.Template.type == CreatureTemplateType.FlyingBigEel)
             {
                 var state = Random.state;
                 Random.InitState(self.abstractCreature.ID.RandomSeed);
                 self.iVars.patternColorB = HSLColor.Lerp(RainWorld.GoldHSL, new(RainWorld.GoldHSL.hue, RainWorld.GoldHSL.saturation, RainWorld.GoldHSL.lightness + Random.value / 12f), .5f);
                 self.iVars.patternColorA = RainWorld.GoldHSL;
                 self.iVars.patternColorA.hue = .5f;
                 self.iVars.patternColorA = HSLColor.Lerp(self.iVars.patternColorA, new(RainWorld.GoldHSL.hue + Random.value / 50f, RainWorld.GoldHSL.saturation + Random.value / 50f, RainWorld.GoldHSL.lightness + Random.value / 4f), .9f);
                 self.airFriction = .98f;
                 self.waterFriction = .999f;
                 self.gravity = 0f;
                 self.buoyancy = 1f;
                 self.bounce = 0f;
                 self.albino = false;
                 var chs = self.bodyChunks;
                 for (var i = 0; i < chs.Length; i++)
                     chs[i].rad *= .75f;
                 Random.state = state;
             }
         });
    }

    internal static void On_BigEel_ctor(On.BigEel.orig_ctor orig, BigEel self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Template.type == CreatureTemplateType.MiniLeviathan)
        {
            var flag = !abstractCreature.superSizeMe;
            self.albino = flag;
            if (flag)
            {
                self.iVars.patternColorB = new(0f, .6f, .75f);
                self.iVars.patternColorA.hue = .5f;
                self.iVars.patternColorA = HSLColor.Lerp(self.iVars.patternColorA, new(.97f, .8f, .75f), .9f);
            }
            self.abstractCreature.lavaImmune = true;
            self.collisionRange = 200f;
            var bs = self.bodyChunks = new BodyChunk[8];
            var bscon = self.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[7];
            for (var i = 0; i < bs.Length; i++)
            {
                var num = i / 7f;
                num = (1f - num) * .5f + Mathf.Sin(Mathf.Pow(num, .5f) * Mathf.PI) * .5f;
                bs[i] = new(self, i, default, Mathf.Lerp(4f, 12f, num), Mathf.Lerp(.1f, .8f, num))
                {
                    restrictInRoomRange = 2000f,
                    defaultRestrictInRoomRange = 2000f
                };
            }
            for (var j = 0; j < bscon.Length; j++)
                bscon[j] = new(bs[j], bs[j + 1], Mathf.Max(bs[j].rad, bs[j + 1].rad), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, -1f);
        }
        else if (self.Template.type == CreatureTemplateType.MiniFlyingBigEel)
        {
            var state = Random.state;
            Random.InitState(self.abstractCreature.ID.RandomSeed);
            self.iVars.patternColorB = HSLColor.Lerp(RainWorld.GoldHSL, new(RainWorld.GoldHSL.hue, RainWorld.GoldHSL.saturation, RainWorld.GoldHSL.lightness + Random.value / 12f), .5f);
            self.iVars.patternColorA = RainWorld.GoldHSL;
            self.iVars.patternColorA.hue = .5f;
            self.iVars.patternColorA = HSLColor.Lerp(self.iVars.patternColorA, new(RainWorld.GoldHSL.hue + Random.value / 50f, RainWorld.GoldHSL.saturation + Random.value / 50f, RainWorld.GoldHSL.lightness + Random.value / 4f), .9f);
            self.airFriction = .98f;
            self.waterFriction = .999f;
            self.gravity = 0f;
            self.buoyancy = 1f;
            self.bounce = 0f;
            self.albino = false;
            self.collisionRange = 200f;
            var bs = self.bodyChunks = new BodyChunk[8];
            var bscon = self.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[7];
            for (var i = 0; i < bs.Length; i++)
            {
                var num = i / 7f;
                num = (1f - num) * .5f + Mathf.Sin(Mathf.Pow(num, .5f) * Mathf.PI) * .5f;
                bs[i] = new(self, i, default, Mathf.Lerp(4f, 12f, num) * .95f, Mathf.Lerp(.1f, .8f, num))
                {
                    restrictInRoomRange = 2000f,
                    defaultRestrictInRoomRange = 2000f
                };
            }
            for (var j = 0; j < bscon.Length; j++)
                bscon[j] = new(bs[j], bs[j + 1], Mathf.Max(bs[j].rad, bs[j + 1].rad), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, -1f);
            Random.state = state;
        }
    }

    internal static void IL_BigEel_AccessSwimSpace(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdsfld<AbstractRoomNode.Type>("SeaExit")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEel self) => self.Template.type == CreatureTemplateType.FlyingBigEel || self.Template.type == CreatureTemplateType.MiniFlyingBigEel ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEel.AccessSwimSpace!");
    }

    internal static void On_BigEel_Crush(On.BigEel.orig_Crush orig, BigEel self, PhysicalObject obj)
    {
        orig(self, obj);
        if (self.Template.type == CreatureTemplateType.MiniLeviathan)
        {
            if (self.room is Room rm)
            {
                for (var i = 0; i < 10; i++)
                {
                    rm.AddObject(new WaterDrip(self.mainBodyChunk.pos, new Vector2(Random.value, Random.value).normalized, false));
                    rm.AddObject(new Bubble(self.mainBodyChunk.pos, new Vector2(Random.value, Random.value).normalized, false, false));
                }
            }
            obj.Destroy();
        }
        else if (self.Template.type == CreatureTemplateType.MiniFlyingBigEel)
        {
            if (self.room is Room rm)
            {
                for (var i = 0; i < 10; i++)
                    rm.AddObject(new WaterDrip(self.mainBodyChunk.pos, new Vector2(Random.value, Random.value).normalized, false));
            }
            obj.Destroy();
        }
    }

    internal static bool On_BigEel_InBiteArea(On.BigEel.orig_InBiteArea orig, BigEel self, Vector2 pos, float margin)
    {
        if (self.Template.type == CreatureTemplateType.MiniLeviathan || self.Template.type == CreatureTemplateType.MiniFlyingBigEel)
        {
            var vector = Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos);
            if (!Custom.DistLess(self.mainBodyChunk.pos + vector * 60f, pos, 14f + margin))
                return false;
            Vector2 pos2 = self.mainBodyChunk.pos, vector2 = Custom.PerpendicularVector(vector);
            if (Mathf.Abs(Custom.DistanceToLine(pos, pos2 + 60f * vector - vector2, pos2 + 60f * vector + vector2)) > 30f + margin)
                return false;
            if (Mathf.Abs(Custom.DistanceToLine(pos, pos2 - vector, pos2 + vector)) > 20f + margin)
                return false;
        }
        return orig(self, pos, margin);
    }

    internal static void IL_BigEel_JawsSnap(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<UpdatableAndDeletable>("room"),
            x => x.MatchLdsfld<SoundID>("Leviathan_Bite")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((SoundID ID, BigEel self) => self.Template.type == CreatureTemplateType.FlyingBigEel || self.Template.type == CreatureTemplateType.MiniFlyingBigEel ? NewSoundID.Flying_Leviathan_Bite : ID);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEel.JawsSnap!");
    }

    internal static Color On_BigEel_ShortCutColor(On.BigEel.orig_ShortCutColor orig, BigEel self) => self.Template.type == CreatureTemplateType.MiniLeviathan ? MiniLeviathanCritob.s_col : (self.Template.type == CreatureTemplateType.MiniFlyingBigEel ? self.Template.shortcutColor : orig(self));

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
                 .EmitDelegate((float submersion, BigEel self) => self.Template.type == CreatureTemplateType.FlyingBigEel || self.Template.type == CreatureTemplateType.MiniFlyingBigEel ? 1f : submersion);
            }
        }
    }

    internal static void IL_BigEelAbstractAI_AbstractBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("world"),
            x => x.MatchLdfld<World>("seaAccessNodes"),
            x => x.MatchLdlen()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int length, BigEelAbstractAI self) => self.parent is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) ? self.world.skyAccessNodes.Length : length);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AbstractBehavior!");
    }

    internal static void IL_BigEelAbstractAI_AddRandomCheckRoom(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("world"),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("world"),
            x => x.MatchCallOrCallvirt<World>("get_firstRoomIndex"),
            x => x.MatchCallOrCallvirt(typeof(World).GetMethod("GetAbstractRoom", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(int)], null)),
            x => x.MatchLdfld<AbstractRoom>("nodes"),
            x => x.MatchLdloc(out _),
            x => x.MatchLdelema<AbstractRoomNode>(),
            x => x.MatchLdfld<AbstractRoomNode>("type"),
            x => x.MatchLdsfld<AbstractRoomNode.Type>("SeaExit")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEelAbstractAI self) => self.parent is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRandomCheckRoom (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("world"),
            x => x.MatchLdfld<World>("seaAccessNodes"),
            x => x.MatchLdcI4(0),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("world"),
            x => x.MatchLdfld<World>("seaAccessNodes"),
            x => x.MatchLdlen(),
            x => x.MatchConvI4(),
            x => x.MatchCall(typeof(Random).GetMethod("Range", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(int), typeof(int)], null)),
            x => x.MatchLdelemAny<WorldCoordinate>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((WorldCoordinate coord, BigEelAbstractAI self) => self.parent is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) ? self.world.skyAccessNodes[Random.Range(0, self.world.skyAccessNodes.Length)] : coord);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRandomCheckRoom (part 2)!");
    }

    internal static void IL_BigEelAbstractAI_AddRoomClusterToCheckList(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("BigEel")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CreatureTemplate.Type type, BigEelAbstractAI self) => self.parent?.creatureTemplate.type == CreatureTemplateType.FlyingBigEel ? CreatureTemplateType.FlyingBigEel : type);
            if (c.TryGotoNext(MoveType.After,
                x => x.OpCode == OpCodes.Call && x.Operand is MethodReference r && r.Name.Contains("op_Equality")))
            {
                c.Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, BigEelAbstractAI self) => (self.parent is not AbstractCreature cr || (cr.creatureTemplate.type != CreatureTemplateType.MiniFlyingBigEel && cr.creatureTemplate.type != CreatureTemplateType.MiniLeviathan)) && flag);
            }
            else
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRoomClusterToCheckList! (part 2)");
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRoomClusterToCheckList! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out _),
            x => x.MatchLdfld<AbstractRoom>("nodes"),
            x => x.MatchLdloc(out _),
            x => x.MatchLdelema<AbstractRoomNode>(),
            x => x.MatchLdfld<AbstractRoomNode>("type"),
            x => x.MatchLdsfld<AbstractRoomNode.Type>("SeaExit")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEelAbstractAI self) => self.parent is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelAbstractAI.AddRoomClusterToCheckList (part 3)!");
    }

    internal static void On_BigEelAI_ctor(On.BigEelAI.orig_ctor orig, BigEelAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if (self.creature.creatureTemplate.type == CreatureTemplateType.MiniLeviathan || self.creature.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel)
        {
            var ut = self.utilityComparer.uTrackers;
            for (var i = 0; i < ut.Count; i++)
            {
                var u = ut[i];
                if (u.module is PreyTracker)
                    u.weight = 1f;
            }
        }
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
                 .EmitDelegate((int defaultLevel, BigEelAI self) => self.creature is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) && self.eel?.room is Room r ? r.TileHeight : defaultLevel);
            }
        }
    }

    internal static void On_BigEelAI_Update(On.BigEelAI.orig_Update orig, BigEelAI self)
    {
        orig(self);
        var flag = self.creature.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel;
        if (self.creature.creatureTemplate.type == CreatureTemplateType.MiniLeviathan || flag)
        {
            if (self.hungerDelay > 2)
                self.hungerDelay -= 2;
            if (Random.value > .00001f && self.behavior == BigEelAI.Behavior.Idle && self.eel?.room is Room rm && flag)
            {
                var newDest = new WorldCoordinate(rm.abstractRoom.index, Random.Range(0, rm.TileWidth), Random.Range(0, rm.TileHeight), -1);
                if (self.pathFinder.CoordinateReachableAndGetbackable(newDest))
                    self.creature.abstractAI.SetDestination(newDest);
            }
        }
    }

    internal static bool On_BigEelAI_WantToChargeJaw(On.BigEelAI.orig_WantToChargeJaw orig, BigEelAI self)
    {
        if ((self.creature.creatureTemplate.type == CreatureTemplateType.MiniLeviathan || self.creature.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) && self.eel is BigEel be && !be.safariControlled)
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
                 .EmitDelegate((int defaultLevel, BigEelAI self) => self.creature is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) && self.eel?.room is Room r ? r.TileHeight : defaultLevel);
            }
        }
        c.Index = il.Body.Instructions.Count - 1;
        c.Emit(OpCodes.Ldarg_0)
         .Emit(OpCodes.Ldarg_1)
         .EmitDelegate((CreatureTemplate.Relationship rel, BigEelAI self, RelationshipTracker.DynamicRelationship dRelation) =>
         {
             if (rel.type == CreatureTemplate.Relationship.Type.Eats && self.creature is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) && self.eel is BigEel be && be.antiStrandingZones is List<PlacedObject> list && list.Count > 0 && dRelation.trackerRep?.representedCreature?.realizedCreature?.mainBodyChunk is BodyChunk b)
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
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(60),
            x => x.MatchNewarr<TailSegment>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((TailSegment[] ar, BigEelGraphics self) => self.eel?.Template.type == CreatureTemplateType.FlyingBigEel ? new TailSegment[45] : ar);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ctor!");
        c.Index = il.Body.Instructions.Count - 1;
        c.Emit(OpCodes.Ldarg_0)
         .EmitDelegate((BigEelGraphics self) =>
         {
             if (self.eel?.Template.type == CreatureTemplateType.FlyingBigEel)
             {
                 var eyes = self.eyesData;
                 for (var n = 0; n < eyes.Length; n++)
                     eyes[n] *= .75f;
             }
         });
    }

    internal static void On_BigEelGraphics_ctor(On.BigEelGraphics.orig_ctor orig, BigEelGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        var flag = self.eel.Template.type == CreatureTemplateType.MiniFlyingBigEel;
        if (self.eel.Template.type == CreatureTemplateType.MiniLeviathan || flag)
        {
            var state = Random.state;
            Random.InitState(self.eel.abstractCreature.ID.RandomSeed);
            var tl = self.tail = new TailSegment[flag ? 11 : 12];
            var chlm1 = self.eel.bodyChunks.Length - 1;
            for (var i = 0; i < tl.Length; i++)
            {
                var t = i / chlm1;
                tl[i] = new(self, Mathf.Lerp(self.eel.bodyChunks[chlm1].rad, 1f, t), 15f, (i > 0) ? tl[i - 1] : null, .5f, 1f, .1f, true);
            }
            var fins = self.fins = new TailSegment[Random.Range(2, 3)][,];
            self.finsData = new float[self.fins.Length, 2];
            var num = Mathf.Lerp(6f, 8f, Random.value);
            if (fins.Length > 2)
                num *= .8f;
            for (var j = 0; j < fins.Length; j++)
            {
                self.finsData[j, 0] = (5f + 10f * Mathf.Sin(Mathf.Pow((float)j / (fins.Length - 1), .5f) * Mathf.PI)) * (flag ? .9f : 1f);
                var num3 = num + num * Mathf.Sin(Mathf.Pow(j / 5f, .8f) * Mathf.PI);
                var fj = fins[j] = new TailSegment[2, Mathf.FloorToInt(self.finsData[j, 0] / 3f) + 1];
                for (var k = 0; k < 2; k++)
                {
                    self.finsData[j, 1] = Random.value;
                    var l1 = fj.GetLength(1);
                    for (var l = 0; l < l1; l++)
                        fins[j][k, l] = new(self, 1f + self.FinContour((float)l / (l1 - 1)) * num3, (self.finsData[j, 0] / Mathf.FloorToInt(self.finsData[j, 0] / 16f) + 1f) * .05f, (l > 0) ? fins[j][k, l - 1] : null, .5f, 1f, .2f, true);
                }
            }
            self.numberOfScales = Random.Range(16, 20);
            self.scaleSize = Mathf.Lerp(.2f, .7f, Mathf.Pow(Random.value, .5f));
            self.numberOfEyes = 20;
            var eyesData = self.eyesData = new Vector2[self.numberOfEyes];
            self.eyeScales = new float[self.numberOfEyes, 3];
            for (var n = 0; n < eyesData.Length; n++)
            {
                var eyeDn = eyesData[n] = Custom.RNV() * Mathf.Pow(Random.value, .6f) * .15f;
                if (eyeDn.y > .7f)
                    eyeDn.y = Mathf.Lerp(eyeDn.y, .7f, .3f);
                self.eyeScales[n, 0] = Mathf.Lerp(.2f, 1f, Mathf.Pow(Random.value, Custom.LerpMap(eyeDn.y, -1f, .7f, 1.5f, .2f)));
                self.eyeScales[n, 1] = Mathf.Lerp(.1f, self.eyeScales[n, 0] * .9f, Mathf.Pow(Random.value, Custom.LerpMap(eyeDn.y, -1f, .7f, 2f, .1f)));
                self.eyeScales[n, 2] = Mathf.Pow(Random.value, Custom.LerpMap(eyeDn.y, -1f, .7f, 2f, .1f));
            }
            Random.state = state;
            var num4 = 0;
            for (var num5 = 0; num5 < fins.Length; num5++)
                num4 += 2 * fins[num5].GetLength(1);
            var bp = self.bodyParts = new BodyPart[tl.Length + num4];
            for (var num6 = 0; num6 < tl.Length; num6++)
                bp[num6] = tl[num6];
            num4 = 0;
            for (var num7 = 0; num7 < fins.Length; num7++)
            {
                for (var num8 = 0; num8 < 2; num8++)
                {
                    var l = fins[num7].GetLength(1);
                    for (var num9 = 0; num9 < l; num9++)
                    {
                        bp[tl.Length + num4] = fins[num7][num8, num9];
                        num4++;
                    }
                }
            }
        }
    }

    internal static void IL_BigEelGraphics_ApplyPalette(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<RainWorld>("ShadPropLeviathanColorA")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int i, BigEelGraphics g) =>
             {
                 if (g.eel is BigEel b && b.Template.type is CreatureTemplate.Type tp)
                 {
                     if (tp == CreatureTemplateType.MiniLeviathan)
                     {
                         if (b.abstractCreature.superSizeMe)
                            return _MiniLeviColorA;
                         return _AMiniLeviColorA;
                     }
                     if (tp == CreatureTemplateType.FlyingBigEel)
                         return _GRJLeviathanColorA;
                     if (tp == CreatureTemplateType.MiniFlyingBigEel)
                         return _GRJMiniLeviathanColorA;
                 }
                 return i;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ApplyPalette (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<RainWorld>("ShadPropLeviathanColorB")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int i, BigEelGraphics g) =>
             {
                 if (g.eel is BigEel b && b.Template.type is CreatureTemplate.Type tp)
                 {
                     if (tp == CreatureTemplateType.MiniLeviathan)
                     {
                         if (b.abstractCreature.superSizeMe)
                             return _MiniLeviColorB;
                         return _AMiniLeviColorB;
                     }
                     if (tp == CreatureTemplateType.FlyingBigEel)
                         return _GRJLeviathanColorB;
                     if (tp == CreatureTemplateType.MiniFlyingBigEel)
                         return _GRJMiniLeviathanColorB;
                 }
                 return i;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ApplyPalette (part 2)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<RainWorld>("ShadPropLeviathanColorHead")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((int i, BigEelGraphics g) =>
             {
                 if (g.eel is BigEel b && b.Template.type is CreatureTemplate.Type tp)
                 {
                     if (tp == CreatureTemplateType.MiniLeviathan)
                     {
                         if (b.abstractCreature.superSizeMe)
                             return _MiniLeviColorHead;
                         return _AMiniLeviColorHead;
                     }
                     if (tp == CreatureTemplateType.FlyingBigEel)
                         return _GRJLeviathanColorHead;
                     if (tp == CreatureTemplateType.MiniFlyingBigEel)
                         return _GRJMiniLeviathanColorHead;
                 }
                 return i;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelGraphics.ApplyPalette (part 3)!");
    }

    internal static void On_BigEelGraphics_ApplyPalette(On.BigEelGraphics.orig_ApplyPalette orig, BigEelGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.eel is BigEel be && (be.Template.type == CreatureTemplateType.FlyingBigEel || be.Template.type == CreatureTemplateType.MiniFlyingBigEel))
        {
            var state = Random.state;
            Random.InitState(be.abstractCreature.ID.RandomSeed);
            var sprites = sLeaser.sprites;
            for (var k = 0; k < 2; k++)
            {
                var beak = sprites[self.BeakSprite(k, 1)];
                beak.color = Color.Lerp(beak.color, RainWorld.GoldRGB, .65f);
                for (var l = 0; l < self.numberOfScales; l++)
                {
                    var scl = sprites[self.ScaleSprite(l, k)];
                    scl.color = Color.Lerp(scl.color, HSLColor.Lerp(be.iVars.patternColorA, be.iVars.patternColorB, Random.value).rgb, .8f);
                }
                for (var m = 0; m < self.fins.Length; m++)
                    sprites[self.FinSprite(m, k)].color = HSLColor.Lerp(be.iVars.patternColorA, be.iVars.patternColorB, Random.value).rgb;
            }
            Random.state = state;
        }
    }

    internal static void On_BigEelGraphics_DrawSprites(On.BigEelGraphics.orig_DrawSprites orig, BigEelGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.eel is BigEel be)
        {
            var tpl = be.Template.type;
            if (tpl == CreatureTemplateType.MiniLeviathan || tpl == CreatureTemplateType.MiniFlyingBigEel)
            {
                Vector2 vector4 = Vector2.Lerp(be.firstChunk.lastPos, be.firstChunk.pos, timeStacker),
                    vector5 = Custom.DirVec(Vector2.Lerp(be.bodyChunks[1].lastPos, be.bodyChunks[1].pos, timeStacker), vector4),
                    vector6 = Custom.PerpendicularVector(vector5);
                float num4 = Mathf.Lerp(self.lastJawCharge, self.jawCharge, timeStacker),
                    num5 = Mathf.Min(Mathf.InverseLerp(0f, .3f, num4), Mathf.InverseLerp(1f, .7f, num4)) * .6f,
                    t = num4 > .35f ? Mathf.InverseLerp(1f, .65f, num4) : 0f,
                    num6 = num5 * Mathf.InverseLerp(.7f, .4f, num4) * .6f,
                    num7 = Mathf.Sin(be.jawChargeFatigue * Mathf.PI) * num6;
                var sprites = sLeaser.sprites;
                for (var k = 0; k < 2; k++)
                {
                    var num8 = k == 0 ? -.3f : .3f;
                    var vector11 = vector4 + vector5 * 65f * num6 + Custom.RNV() * num7 * 2f;
                    vector11 += vector6 * num8 * (Mathf.Lerp(30f, 6f + be.beakGap / 20f, t) + 10f * Mathf.Sin(Mathf.Pow(num5, 2f) * Mathf.PI));
                    var num11 = Custom.VecToDeg(vector5) + Mathf.Sin(num5 * Mathf.PI) * (num4 < .35f ? -20f : -10f) * num8 + Mathf.Lerp(-2f, 2f, Random.value) * num7;
                    for (var num12 = 0; num12 < 2; num12++)
                    {
                        var s = sprites[self.BeakSprite(k, num12)];
                        s.x = vector11.x - camPos.x;
                        s.y = vector11.y - camPos.y;
                        s.rotation = num11;
                        var num13 = num4 >= .35f ? (num12 == 0 ? (43f * Mathf.Abs(Mathf.Cos(Mathf.InverseLerp(1f, .4f, num4) * Mathf.PI))) : (30f * Mathf.InverseLerp(1f, .4f, num4))) : (num12 == 0 ? Mathf.Lerp(15f, 43f, Mathf.InverseLerp(.35f, .15f, num4)) : (30f * Mathf.Pow(Mathf.InverseLerp(0f, .5f, num5), .2f)));
                        Vector2 vector12 = vector4 + vector6 * num8 * num13 - vector5 * (num12 == 0 ? 22f : 30f),
                            vector13 = vector11 + Custom.DegToVec(num11) * (num12 == 0 ? -12f : 10f) + Custom.PerpendicularVector(Custom.DegToVec(num11)) * 2.5f * num8,
                            vector14 = Custom.InverseKinematic(vector12, vector13, 35f, 25f, 0f - num8);
                        int num14;
                        for (num14 = 0; num14 < 2; num14++)
                        {
                            s = sprites[self.BeakArmSprite(num14, num12, k)];
                            s.x = vector14.x - camPos.x;
                            s.y = vector14.y - camPos.y;
                        }
                        sprites[self.BeakArmSprite(0, num12, k)].rotation = Custom.AimFromOneVectorToAnother(vector14, vector12);
                        for (num14 = 2; num14 < 4; num14++)
                        {
                            s = sprites[self.BeakArmSprite(num14, num12, k)];
                            s.x = vector13.x - camPos.x;
                            s.y = vector13.y - camPos.y;
                        }
                        sprites[self.BeakArmSprite(2, num12, k)].rotation = Custom.AimFromOneVectorToAnother(vector13, vector14);
                    }
                }
            }
            else if (tpl == CreatureTemplateType.FlyingBigEel)
            {
                var vector4 = Vector2.Lerp(be.bodyChunks[0].lastPos, be.bodyChunks[0].pos, timeStacker);
                var vec = Vector2.Lerp(be.bodyChunks[1].lastPos, be.bodyChunks[1].pos, timeStacker);
                var vec2 = Vector2.Lerp(vector4, vec, .5f) - camPos;
                for (var k = 0; k < 2; k++)
                {
                    for (var num12 = 0; num12 < 2; num12++)
                    {
                        for (var num14 = 0; num14 < 4; num14++)
                        {
                            var s = sLeaser.sprites[self.BeakArmSprite(num14, num12, k)];
                            s.x = Mathf.Lerp(s.x, vec2.x, .2f);
                            s.y = Mathf.Lerp(s.y, vec2.y, .2f);
                        }
                    }
                }
            }
        }
    }

    internal static void On_BigEelGraphics_InitiateSprites(On.BigEelGraphics.orig_InitiateSprites orig, BigEelGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.eel is BigEel be && be.Template.type is CreatureTemplate.Type tp)
        {
            var sprites = sLeaser.sprites;
            if (tp == CreatureTemplateType.MiniLeviathan)
            {
                var alt = be.abstractCreature.superSizeMe;
                sprites[self.MeshSprite].shader = Custom.rainWorld.Shaders[alt ? "MiniLeviEelBody" : "AMiniLeviEelBody"];
                for (var k = 0; k < 2; k++)
                {
                    for (var l = 0; l < self.fins.Length; l++)
                    {
                        var fin = sprites[self.FinSprite(l, k)];
                        fin.shader = Custom.rainWorld.Shaders[alt ? "MiniLeviEelFin" : "AMiniLeviEelFin"];
                        fin.MoveToBack();
                    }
                    var s = sprites[self.BeakSprite(k, 0)];
                    s.scaleX *= .35f;
                    s.scaleY *= .4f;
                    s = sprites[self.BeakSprite(k, 1)];
                    s.scaleX *= .35f;
                    s.scaleY *= .4f;
                    for (var n = 0; n < 4; n++)
                    {
                        for (var num = 0; num < 2; num++)
                        {
                            s = sprites[self.BeakArmSprite(n, num, k)];
                            if (n % 2 == 0)
                            {
                                s.scaleX = 2f;
                                s.scaleY = 15f;
                            }
                            else
                                s.scale = .25f;
                        }
                    }
                }
            }
            else if (tp == CreatureTemplateType.FlyingBigEel)
            {
                sprites[self.MeshSprite].shader = Custom.rainWorld.Shaders["GRJEelBody"];
                for (var k = 0; k < 2; k++)
                {
                    for (var l = 0; l < self.fins.Length; l++)
                        sprites[self.FinSprite(l, k)].shader = Custom.rainWorld.Shaders["TentaclePlant"];
                    for (var num = 0; num < 2; num++)
                    {
                        for (var n = 0; n < 4; n++)
                        {
                            if (n % 2 == 0)
                                sprites[self.BeakArmSprite(n, num, k)].scaleX *= .75f;
                            else
                                sprites[self.BeakArmSprite(n, num, k)].scale *= .75f;
                        }
                        sprites[self.BeakSprite(k, num)].element = Futile.atlasManager.GetElementWithName("FEelJaw" + (2 - k) + (num is 0 ? "A" : "B"));
                    }
                }
            }
            else if (tp == CreatureTemplateType.MiniFlyingBigEel)
            {
                sprites[self.MeshSprite].shader = Custom.rainWorld.Shaders["GRJMiniEelBody"];
                for (var k = 0; k < 2; k++)
                {
                    for (var l = 0; l < self.fins.Length; l++)
                    {
                        var fin = sprites[self.FinSprite(l, k)];
                        fin.shader = Custom.rainWorld.Shaders["TentaclePlant"];
                        fin.MoveToBack();
                    }
                    var s = sprites[self.BeakSprite(k, 0)];
                    s.scaleX *= .35f;
                    s.scaleY *= .4f;
                    s = sprites[self.BeakSprite(k, 1)];
                    s.scaleX *= .35f;
                    s.scaleY *= .4f;
                    for (var n = 0; n < 4; n++)
                    {
                        for (var num = 0; num < 2; num++)
                        {
                            s = sprites[self.BeakArmSprite(n, num, k)];
                            if (n % 2 == 0)
                            {
                                s.scaleX = 2f;
                                s.scaleY = 15f;
                            }
                            else
                                s.scale = .25f;
                        }
                    }
                }
            }
        }
    }

    internal static void On_BigEelGraphics_Reset(On.BigEelGraphics.orig_Reset orig, BigEelGraphics self)
    {
        orig(self);
        if (self.eel?.Template.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.FlyingBigEel || tp == CreatureTemplateType.MiniFlyingBigEel) && self.finSound is StaticSoundLoop snd)
            snd.volume = 0f;
    }

    internal static void IL_BigEelGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        c.Emit(OpCodes.Ldarg_0)
         .EmitDelegate((BigEelGraphics self) =>
         {
             if (self.eel?.Template.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.FlyingBigEel || tp == CreatureTemplateType.MiniFlyingBigEel) && self.finSound is StaticSoundLoop snd)
                 snd.volume = 0f;
         });
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<BigEelGraphics>("eel"),
            x => x.MatchLdfld<BigEel>("swimSpeed"),
            x => x.MatchCall<Mathf>("Lerp"),
            x => x.MatchDiv(),
            x => x.MatchSub(),
            x => x.MatchStfld<BigEelGraphics>("tailSwim")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((BigEelGraphics self) =>
             {
                 if (self.eel?.Template.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.FlyingBigEel || tp == CreatureTemplateType.MiniFlyingBigEel))
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
                     .EmitDelegate((bool sub, BigEelGraphics self) => self.eel?.Template.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.FlyingBigEel || tp == CreatureTemplateType.MiniFlyingBigEel) || sub);
                }
            }
        }
    }

    internal static void On_BigEelGraphics_Update(On.BigEelGraphics.orig_Update orig, BigEelGraphics self)
    {
        orig(self);
        if (self.eel?.Template.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.MiniLeviathan || tp == CreatureTemplateType.MiniFlyingBigEel))
        {
            if (self.chargedJawsSound is StaticSoundLoop s)
            {
                s.pitch *= 1.2f;
                s.volume *= .25f;
            }
            if (self.hydraulicsSound is StaticSoundLoop s2)
            {
                s2.pitch *= 1.2f;
                s2.volume *= .25f;
            }
        }
    }

    internal static void IL_BigEelPather_FollowPath(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdfld<RoomBorderExit>("type"),
            x => x.MatchLdsfld<AbstractRoomNode.Type>("SeaExit")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((AbstractRoomNode.Type nodeType, BigEelPather self) => self.creature is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) ? AbstractRoomNode.Type.SkyExit : nodeType);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelPather.FollowPath (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<PathFinder>("world"),
            x => x.MatchLdfld<World>("seaAccessNodes")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((WorldCoordinate[] accessNodes, BigEelPather self) => self.creature is AbstractCreature cr && (cr.creatureTemplate.type == CreatureTemplateType.FlyingBigEel || cr.creatureTemplate.type == CreatureTemplateType.MiniFlyingBigEel) ? self.world.skyAccessNodes : accessNodes);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigEelPather.FollowPath (part 2)!");
    }
}