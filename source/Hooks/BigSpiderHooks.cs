﻿global using static LBMergedMods.Hooks.BigSpiderHooks;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;
using RWCustom;
using System;
using Noise;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;
//CHK
public static class BigSpiderHooks
{
    internal static bool On_BigSpider_get_CanIBeRevived(Func<BigSpider, bool> orig, BigSpider self) => self is not Sporantula && orig(self);

    internal static bool On_BigSpider_get_CanJump(Func<BigSpider, bool> orig, BigSpider self) => self is Sporantula ? self.jumpStamina >= .1f && self.grasps[0] is null : orig(self);

    internal static void IL_BigSpider_Act(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_BigSpider_spitter))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool spitter, BigSpider self) => spitter && self is not Sporantula);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpider.Act!");
    }

    internal static void IL_BigSpider_Collide(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchIsinst_BigSpider,
            s_MatchBrfalse_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((BigSpider self, PhysicalObject otherObject) => self is Sporantula && otherObject is BigSpider b && b is not Sporantula);
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpider.Collide!");
        var ctr = 0;
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var instr = ins[i];
            if (instr.MatchLdfld<BigSpider>("spitter"))
            {
                ++ctr;
                if (ctr == 2)
                {
                    c.Goto(instr, MoveType.After)
                     .Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool spitter, BigSpider self) => self is not Sporantula && spitter);
                    break;
                }
            }
        }
        ctr = 0;
        for (var i = ins.Count - 1; i >= 0; i--)
        {
            var instr = ins[i];
            if (instr.MatchLdfld<BigSpider>("spitter"))
            {
                ++ctr;
                if (ctr <= 3)
                {
                    c.Goto(instr, MoveType.After)
                     .Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool spitter, BigSpider self) => self is not Sporantula && spitter);
                }
                else
                    break;
            }
        }
    }

    internal static void IL_BigSpider_FlyingWeapon(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_BigSpider_spitter))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool spitter, BigSpider self) => self is not Sporantula && spitter);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpider.FlyingWeapon! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdcR4_0_3))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((float stam, BigSpider self) => self is Sporantula ? stam * .25f : stam);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpider.FlyingWeapon! (part 2)");
    }

    internal static void On_BigSpider_InitiateJump(On.BigSpider.orig_InitiateJump orig, BigSpider self, Vector2 target)
    {
        orig(self, target);
        if (self is Sporantula)
            self.canBite = Math.Min(self.canBite + 5, 10);
    }

    internal static void On_BigSpider_Revive(On.BigSpider.orig_Revive orig, BigSpider self)
    {
        if (self is not Sporantula)
            orig(self);
    }

    internal static void IL_BigSpider_Spit(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdcR4_2,
            s_MatchCall_Vector2_op_Multiply,
            s_MatchCall_Vector2_op_Addition,
            s_MatchStfld_BodyChunk_vel))
        {
            var label = il.DefineLabel();
            label.Target = il.Instrs[il.Instrs.Count - 1];
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((BigSpider self, Vector2 aimDir) =>
             {
                 if (self is Sporantula)
                 {
                     var puffBall = new AbstractConsumable(self.room.world, AbstractObjectType.SporeProjectile, null, self.abstractCreature.pos, self.room.game.GetNewID(), -1, -1, null);
                     puffBall.RealizeInRoom();
                     var rObj = puffBall.realizedObject as SmallPuffBall;
                     rObj!.Shoot(self.mainBodyChunk.pos, aimDir, self);
                     self.room.PlaySound(SoundID.Big_Spider_Spit, self.mainBodyChunk);
                     self.AI.spitModule.SporantulaHasSpit();
                     self.charging += .1f;
                     return true;
                 }
                 return false;
             });
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpider.Spit!");
    }

    internal static void On_BigSpider_TryInitiateSpit(On.BigSpider.orig_TryInitiateSpit orig, BigSpider self)
    {
        var flag = self is Sporantula;
        if (flag && (self.AI?.spitModule?.spitAtCrit is not Tracker.CreatureRepresentation rep || rep.representedCreature.creatureTemplate.type == CreatureTemplateType.Sporantula))
            return;
        orig(self);
        if (flag)
            self.Spit();
    }

    internal static void IL_BigSpider_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_BigSpider_jumpStamina,
            s_MatchLdcR4_1,
            s_MatchBneUn_Any,
            s_MatchLdarg_0,
            s_MatchLdfld_BigSpider_spitter))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool spitter, BigSpider self) => self is not Sporantula && spitter);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpider.Update!");
    }

    internal static float On_BigSpiderAI_get_ShyFromLight(Func<BigSpiderAI, float> orig, BigSpiderAI self) => self is SporantulaAI ? 0f : orig(self);

    internal static void On_BigSpiderAI_ReactToNoise(On.BigSpiderAI.orig_ReactToNoise orig, BigSpiderAI self, NoiseTracker.TheorizedSource source, InGameNoise noise)
    {
        orig(self, source, noise);
        if (self is SporantulaAI)
            self.noiseRectionDelay = Random.Range(0, 4);
    }

    internal static void IL_BigSpiderAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_BigSpider))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CreatureTemplate.Type type, BigSpiderAI self) => self is SporantulaAI ? CreatureTemplateType.Sporantula : type);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpiderAI.Update! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_InLoc1,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_SpitterSpider))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CreatureTemplate.Type type, BigSpiderAI self) => self is SporantulaAI ? CreatureTemplateType.Sporantula : type);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpiderAI.Update! (part 2)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchStfld_NoiseTracker_hearingSkill,
            s_MatchLdarg_0,
            s_MatchLdfld_BigSpiderAI_bug,
            s_MatchLdfld_BigSpider_spitter))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool spitter, BigSpiderAI self) => self is not SporantulaAI && spitter);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpiderAI.Update! (part 3)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_BigSpiderAI_bug,
            s_MatchLdfld_BigSpider_spitter,
            s_MatchBrfalse_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((BigSpiderAI self) =>
             {
                 if (self is SporantulaAI)
                 {
                     if (self.preyTracker.MostAttractivePrey.representedCreature.realizedCreature is Creature c && self.bug is Sporantula s && s.CanJump && !s.jumping && s.charging == 0f && s.Footing && c.room == s.room)
                     {
                         Vector2 pos = c.mainBodyChunk.pos, myPos = s.mainBodyChunk.pos, b1pos = s.bodyChunks[1].pos;
                         if (Custom.DistLess(myPos, pos, 120f) && (s.room.aimap.TileAccessibleToCreature(Room.StaticGetTilePosition(b1pos - Custom.DirVec(b1pos, pos) * 30f), s.Template) || s.room.GetTile(b1pos - Custom.DirVec(b1pos, pos) * 30f).Solid) && s.room.VisualContact(myPos, pos))
                         {
                             if (Vector2.Dot((myPos - pos).normalized, (b1pos - myPos).normalized) > .2f)
                                 s.InitiateJump(pos);
                             else
                             {
                                 s.mainBodyChunk.vel += Custom.DirVec(myPos, pos);
                                 s.bodyChunks[1].vel -= Custom.DirVec(myPos, pos);
                             }
                         }
                     }
                 }
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpiderAI.Update! (part 4)");
    }

    internal static void IL_BigSpiderAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdarg_1,
            s_MatchLdfld_RelationshipTracker_DynamicRelationship_trackerRep,
            s_MatchLdfld_Tracker_CreatureRepresentation_representedCreature,
            s_MatchCallOrCallvirt_ArtificialIntelligence_StaticRelationship,
            s_MatchStloc_OutLoc1))
        {
            VariableDefinition l;
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .Emit(OpCodes.Ldloc, l = il.Body.Variables[s_loc1])
             .EmitDelegate((BigSpiderAI self, RelationshipTracker.DynamicRelationship dRelation, CreatureTemplate.Relationship result) =>
             {
                 if (self.bug is BigSpider b && dRelation.trackerRep?.representedCreature?.creatureTemplate?.type is CreatureTemplate.Type tp && (tp == CreatureTemplateType.Scutigera || tp == CreatureTemplateType.RedHorrorCenti || tp == CreatureTemplateType.Killerpillar || tp == CreatureTemplateType.Glowpillar) && dRelation.state is BigSpiderAI.SpiderTrackState st && st.consious && st.totalMass > b.TotalMass * 2f)
                     result = new(CreatureTemplate.Relationship.Type.Afraid, Mathf.InverseLerp(b.TotalMass, b.TotalMass * 7f, st.totalMass));
                 return result;
             });
            c.Emit(OpCodes.Stloc, l);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpiderAI.IUseARelationshipTracker.UpdateDynamicRelationship!");
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var instr = ins[i];
            if (instr.MatchLdfld<BigSpider>("spitter"))
            {
                c.Goto(instr, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool spitter, BigSpiderAI self) => self is not SporantulaAI && spitter);
            }
        }
    }

    internal static CreatureTemplate.Relationship On_BigSpiderAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.BigSpiderAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigSpiderAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var res = orig(self, dRelation);
        if (self is SporantulaAI && dRelation.trackerRep?.representedCreature is AbstractCreature cr && SporeMemory.TryGetValue(self.creature, out var mem))
        {
            if (cr.creatureTemplate.type == CreatureTemplate.Type.Deer)
            {
                res.type = CreatureTemplate.Relationship.Type.Afraid;
                res.intensity = 1f;
            }
            else if (!mem.Contains(cr) || cr.creatureTemplate.type == CreatureTemplateType.Sporantula)
            {
                res.type = CreatureTemplate.Relationship.Type.Ignores;
                res.intensity = 0f;
            }
            else
            {
                res.type = CreatureTemplate.Relationship.Type.Eats;
                res.intensity = 1f;
            }
            if (dRelation.state is BigSpiderAI.SpiderTrackState s)
                s.armed = false;
        }
        else if (self.creature.creatureTemplate.type == CreatureTemplate.Type.BigSpider || self.creature.creatureTemplate.type?.value == "MaracaSpider")
        {
            if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c && c.abstractPhysicalObject.SameRippleLayer(self.creature) && self.StaticRelationship(c.abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                var grs = c.grasps;
                if (grs is not null)
                {
                    for (var i = 0; i < grs.Length; i++)
                    {
                        if (grs[i]?.grabbed is StarLemon)
                        {
                            res.type = CreatureTemplate.Relationship.Type.Ignores;
                            res.intensity = 0f;
                            break;
                        }
                    }
                }
            }
        }
        return res;
    }

    internal static bool On_SpiderSpitModule_CanSpit(On.BigSpiderAI.SpiderSpitModule.orig_CanSpit orig, BigSpiderAI.SpiderSpitModule self) => (self.AI is not SporantulaAI || self.spitAtCrit?.representedCreature?.state?.dead is false) && orig(self);

    internal static void IL_SpiderSpitModule_SpitPosScore(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_CreatureTemplate_Type_SpitterSpider))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CreatureTemplate.Type type, BigSpiderAI.SpiderSpitModule self) => self.AI is SporantulaAI ? CreatureTemplateType.Sporantula : type);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook BigSpiderAI.SpiderSpitModule.SpitPosScore!");
    }

    internal static void On_SpiderSpitModule_Update(On.BigSpiderAI.SpiderSpitModule.orig_Update orig, BigSpiderAI.SpiderSpitModule self)
    {
        if (self.AI is SporantulaAI && self.spitAtCrit?.representedCreature.creatureTemplate.type == CreatureTemplateType.Sporantula)
            self.spitAtCrit = null;
        orig(self);
    }

    public static void SporantulaHasSpit(this BigSpiderAI.SpiderSpitModule self)
    {
        if (!self.bug.safariControlled)
            --self.ammo;
        self.ammoRegen = 0f;
        if (self.ammo < 1)
        {
            self.fastAmmoRegen = true;
            self.randomCritSpitDelay = 0;
        }
        else
            self.fastAmmoRegen = false;
    }
}