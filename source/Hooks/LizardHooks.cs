global using static LBMergedMods.Hooks.LizardHooks;
using UnityEngine;
using LizardCosmetics;
using System;
using Random = UnityEngine.Random;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RWCustom;
using System.Collections.Generic;
using Noise;
using MoreSlugcats;

namespace LBMergedMods.Hooks;

public static class LizardHooks
{
    internal static void On_AxolotlGills_DrawSprites(On.LizardCosmetics.AxolotlGills.orig_DrawSprites orig, AxolotlGills self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is LizardGraphics lg && lg.lizard is Lizard l && l.AI?.yellowAI is PolliwogCommunication c)
        {
            var flicker = Mathf.Lerp(c.LastFlicker, c.CurrentFlicker, timeStacker);
            if (!l.Consious)
                flicker = 0f;
            for (var num = self.startSprite + self.scalesPositions.Length - 1; num >= self.startSprite; num--)
            {
                sLeaser.sprites[num].color = Color.Lerp(lg.HeadColor(timeStacker), Color.Lerp(lg.HeadColor(timeStacker), lg.effectColor, .6f), flicker);
                if (self.colored)
                    sLeaser.sprites[num + self.scalesPositions.Length].color = c.PackLeader ? lg.HeadColor(timeStacker) : Color.Lerp(lg.HeadColor(timeStacker), new(1f, .007843137254902f, .3529411764705882f), flicker);
            }
        }
    }

    internal static void On_BumpHawk_DrawSprites(On.LizardCosmetics.BumpHawk.orig_DrawSprites orig, BumpHawk self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is LizardGraphics g && g.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            var sprites = sLeaser.sprites;
            int start = self.startSprite, numMax = start + self.numberOfSprites - 1;
            for (var num = numMax; num >= start; num--)
            {
                var spr = sprites[num];
                var num2 = Mathf.InverseLerp(start, numMax, num);
                if (self.coloredHawk)
                    spr.color = Color.Lerp(g.HeadColor(timeStacker), g.BodyColor(1f), num2);
                else
                    spr.color = g.DynamicBodyColor(1f);
            }
        }
    }

    internal static void On_JumpRings_DrawSprites(On.LizardCosmetics.JumpRings.orig_DrawSprites orig, JumpRings self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is LizardGraphics g && g.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            var sprs = sLeaser.sprites;
            var c = g.BodyColor(1f);
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                    sprs[self.RingSprite(i, j, 1)].color = c;
            }
        }
    }

    internal static float On_Lizard_get_VisibilityBonus(Func<Lizard, float> orig, Lizard self) => self.Template.type == CreatureTemplateType.HunterSeeker && self.graphicsModule is LizardGraphics g ? 0f - g.Camouflaged : orig(self);

    internal static void On_Lizard_ctor(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        var tpl = self.Template.type;
        Random.State state;
        if (tpl == CreatureTemplateType.SilverLizard)
        {
            state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.58f, .08f, .6f), .3f, Custom.ClampedRandomVariation(.8f, .15f, .1f));
            Random.state = state;
        }
        else if (tpl == CreatureTemplateType.NoodleEater)
        {
            state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.effectColor = abstractCreature.superSizeMe ? Custom.HSL2RGB(Custom.WrappedRandomVariation(86f / 360f, .05f, .6f), Custom.WrappedRandomVariation(.95f, .05f, .1f), Custom.ClampedRandomVariation(.5f, .05f, .1f)) : Custom.HSL2RGB(Custom.WrappedRandomVariation(.8333f, .05f, .6f), Custom.WrappedRandomVariation(.9f, .05f, .1f), Custom.ClampedRandomVariation(.7f, .05f, .1f));
            Random.state = state;
            self.tongue ??= new(self);
        }
        else if (tpl == CreatureTemplateType.Polliwog)
        {
            state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.tongue ??= new(self);
            self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.708f, .1f, .6f), .482f, Custom.ClampedRandomVariation(.5f, .15f, .1f));
            self.buoyancy = .92f;
            Random.state = state;
        }
        else if (tpl == CreatureTemplateType.WaterSpitter)
        {
            self.buoyancy = .915f;
            self.effectColor = Color.white;
        }
        else if (tpl == CreatureTemplateType.HunterSeeker)
        {
            self.effectColor = self.lizardParams.standardColor;
            self.jumpModule = new(self);
        }
        else if (tpl == CreatureTemplateType.MoleSalamander)
        {
            self.buoyancy = .92f;
            state = Random.state;
            Random.InitState(abstractCreature.ID.RandomSeed);
            self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(.9f, .15f, .6f), 1f, Custom.ClampedRandomVariation(.4f, .15f, .2f));
            Random.state = state;
            self.abstractCreature.HypothermiaImmune = true;
            self.firstChunk.rad *= 1.15f;
        }
    }

    internal static void IL_Lizard_Act(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Creature>("get_abstractCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("CyanLizard"),
            x => x.MatchCall(out _),
            x => x.MatchBrfalse(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Lizard self) => self.Template.type != CreatureTemplateType.HunterSeeker);
            c.Emit(OpCodes.Brfalse, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.Act!");
    }

    internal static void IL_Lizard_ActAnimation(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Lizard>("animation"),
            x => x.MatchLdsfld<Lizard.Animation>("Spit"),
            x => x.MatchCall(typeof(ExtEnum<Lizard.Animation>).GetMethod("op_Equality")),
            x => x.MatchBrfalse(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Lizard self) =>
             {
                 if (self.IsWaterSpitter())
                 {
                     self.SpitWater();
                     return true;
                 }
                 return false;
             });
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.ActAnimation!");
    }

    internal static void On_Lizard_DamageAttackClosestChunk(On.Lizard.orig_DamageAttackClosestChunk orig, Lizard self, Creature target)
    {
        if (self.grabbedBy is List<Creature.Grasp> l && l.Count > 0 && l[0].grabber is FatFireFly c && c == target)
            return;
        orig(self, target);
    }

    internal static void IL_Lizard_EnterAnimation(ILContext il)
    {
        var c = new ILCursor(il);
        var label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Creature>("get_abstractCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("YellowLizard"),
            x => x.MatchCall(out _),
            x => x.MatchBrfalse(out _)))
        {
            label.Target = c.Next;
            c.Index -= 7;
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Lizard self) => self.IsPolliwog());
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.EnterAnimation!");
    }

    internal static bool On_Lizard_HitHeadShield(On.Lizard.orig_HitHeadShield orig, Lizard self, Vector2 direction) => self.Template.type != CreatureTemplateType.NoodleEater && orig(self, direction);

    internal static Color On_Lizard_ShortCutColor(On.Lizard.orig_ShortCutColor orig, Lizard self) => self.Template.type == CreatureTemplateType.MoleSalamander && self.graphicsModule is LizardGraphics { blackSalamander: true } ? Color.Lerp(Color.black, Color.gray, .5f) : orig(self);

    internal static bool On_Lizard_SpearStick(On.Lizard.orig_SpearStick orig, Lizard self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos onAppendagePos, Vector2 direction)
    {
        var res = orig(self, source, dmg, chunk, onAppendagePos, direction);
        var flag = chunk.index == 0 && self.HitInMouth(direction);
        if (source is Spear s && self.Template.type == CreatureTemplateType.HunterSeeker && !self.dead && !flag && self.jumpModule.gasLeakPower > 0f && self.jumpModule.gasLeakSpear == null && chunk.index < 2 && (self.animation == Lizard.Animation.Jumping || self.animation == Lizard.Animation.PrepareToJump || Random.value < (chunk.index == 1 ? .5f : .25f)))
            self.jumpModule.gasLeakSpear = s;
        return res;
    }

    internal static void IL_Lizard_SwimBehavior(ILContext il)
    {
        var c = new ILCursor(il);
        var loc = 0;
        if (c.TryGotoNext(
            x => x.MatchLdcI4(1),
            x => x.MatchStloc(out loc)))
        {
            var l = il.Body.Variables[loc];
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldloc, l)
             .EmitDelegate((Lizard self, bool flag) => !self.IsWaterSpitter() && !self.IsPolliwog() && !self.IsMoleSala() && flag);
            c.Emit(OpCodes.Stloc, l);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.SwimBehavior! (part 1)");
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Creature>("get_Template"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Salamander"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Lizard self) => self.IsPolliwog() || self.IsWaterSpitter() || self.IsMoleSala());
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.SwimBehavior! (part 2)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<ModManager>("MMF"))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<ModManager>("MMF"))
         && c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt(typeof(Room).GetMethod("MiddleOfTile", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(Vector2)], null))))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Vector2 removedVal, Lizard self) => self.room.MiddleOfTile(self.followingConnection.destinationCoord));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.SwimBehavior! (part 3)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchRet()))
        {
            c.Prev.OpCode = OpCodes.Ldarg_0;
            c.EmitDelegate((Lizard self) =>
            {
                if (self.IsWaterSpitter())
                    self.salamanderLurk = false;
            });
            c.Emit(OpCodes.Ret);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Lizard.SwimBehavior! (part 4)");
    }

    internal static void On_Lizard_Update(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        orig(self, eu);
        if (self.IsPolliwog())
        {
            self.lungs = 1f;
            if (self.LizardState?.limbHealth is float[] ar)
            {
                ar[2] = 0f;
                ar[3] = 0f;
            }
        }
        else if (self.IsWaterSpitter() || self.IsMoleSala())
            self.lungs = 1f;
    }

    internal static void On_LizardAI_ctor(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        var tpl = creature.creatureTemplate.type;
        if (tpl == CreatureTemplateType.Polliwog)
        {
            self.AddModule(self.yellowAI = new PolliwogCommunication(self));
            self.AddModule(self.lurkTracker = new(self, self.lizard));
            self.utilityComparer.AddComparedModule(self.lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
        }
        else if (tpl == CreatureTemplateType.WaterSpitter)
            self.AddModule(self.redSpitAI = new(self));
        else if (tpl == CreatureTemplateType.HunterSeeker)
        {
            self.AddModule(self.lurkTracker = new(self, self.lizard));
            self.utilityComparer.AddComparedModule(self.lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
        }
        else if (tpl == CreatureTemplateType.MoleSalamander)
        {
            self.AddModule(new SuperHearing(self, self.tracker, 350f));
            self.lurkTracker = new(self, self.lizard);
            self.AddModule(self.lurkTracker);
            self.utilityComparer.AddComparedModule(self.lurkTracker, null, Mathf.Lerp(.4f, .3f, creature.personality.energy), 1f);
        }
    }

    internal static bool On_LizardAI_ComfortableIdlePosition(On.LizardAI.orig_ComfortableIdlePosition orig, LizardAI self) => orig(self) || (self.lizard is Lizard l && (l.IsPolliwog() || l.IsWaterSpitter() || l.IsMoleSala()) && l.room.GetTile(l.bodyChunks[0].pos).AnyWater);

    internal static float On_LizardAI_IdleSpotScore(On.LizardAI.orig_IdleSpotScore orig, LizardAI self, WorldCoordinate coord)
    {
        var res = orig(self, coord);
        if (coord.room != self.creature.pos.room || !coord.TileDefined)
            return res;
        if (self.lizard?.room.aimap.WorldCoordinateAccessibleToCreature(coord, self.creature.creatureTemplate) is null or false || !self.pathFinder.CoordinateReachableAndGetbackable(coord) || coord.CompareDisregardingNode(self.forbiddenIdleSpot))
            return res;
        if (self.lizard is Lizard l && (l.IsPolliwog() || l.IsWaterSpitter() || l.IsMoleSala()))
        {
            if (!l.room.GetTile(coord).AnyWater)
                res += 20f;
            res += Mathf.Max(0f, coord.Tile.FloatDist(self.creature.pos.Tile) - 30f) * 1.5f;
            res += Mathf.Abs(coord.y - l.room.DefaultWaterLevel(coord.Tile)) * 10f;
            res += l.room.aimap.getTerrainProximity(coord) * 10f;
            if (l.IsMoleSala())
            {
                if (l.room.aimap.getAItile(coord).narrowSpace)
                    res -= 10f;
                res += l.room.aimap.getAItile(coord.Tile).visibility * .1f;
            }
        }
        return res;
    }

    internal static void On_LizardAI_NewRoom(On.LizardAI.orig_NewRoom orig, LizardAI self, Room room)
    {
        if (self.yellowAI is PolliwogCommunication c)
            c.communicating = 0;
        orig(self, room);
    }

    internal static void On_LizardAI_ReactToNoise(On.LizardAI.orig_ReactToNoise orig, LizardAI self, NoiseTracker.TheorizedSource source, InGameNoise noise)
    {
        if (source.creatureRep is not null && self.creature?.creatureTemplate.type == CreatureTemplateType.MoleSalamander)
        {
            self.lizard.bubble = Math.Max(self.lizard.bubble, 4);
            return;
        }
        orig(self, source, noise);
    }

    internal static PathCost On_LizardAI_TravelPreference(On.LizardAI.orig_TravelPreference orig, LizardAI self, MovementConnection connection, PathCost cost)
    {
        var res = orig(self, connection, cost);
        if (self.lizard is Lizard l)
        {
            if (self.yellowAI is PolliwogCommunication c)
            {
                res = c.TravelPreference(connection, res);
                if (!l.room.GetTile(connection.destinationCoord).AnyWater)
                    res.resistance += 5f;
            }
            else if (l.IsWaterSpitter() || l.IsMoleSala())
            {
                if (!l.room.GetTile(connection.destinationCoord).AnyWater)
                    res.resistance += 5f;
            }
        }
        return res;
    }

    internal static void On_LizardAI_Update(On.LizardAI.orig_Update orig, LizardAI self)
    {
        if (self.lizard is Lizard l && l.IsWaterSpitter() && l.Submersion >= 1f && self.redSpitAI is LizardAI.LizardSpitTracker t)
        {
            t.wantToSpit = false;
            t.spitting = false;
        }
        orig(self);
        if (self.lizard is Lizard li && li.IsWaterSpitter())
        {
            self.noiseTracker.hearingSkill = 1.6f;
            if (self.redSpitAI is LizardAI.LizardSpitTracker tr && tr.spitting && li.animation != Lizard.Animation.Spit)
            {
                tr.delay = 0;
                li.voice.MakeSound(LizardVoice.Emotion.BloodLust);
                li.EnterAnimation(Lizard.Animation.Spit, false);
                li.bubble = 10;
                li.bubbleIntensity = 1f;
            }
        }
        else if (self.creature?.creatureTemplate.type == CreatureTemplateType.MoleSalamander && self.noiseTracker is NoiseTracker n)
            n.hearingSkill = 2f;
    }

    internal static void IL_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<RelationshipTracker.DynamicRelationship>("state"),
            x => x.MatchIsinst<LizardAI.LizardTrackState>(),
            x => x.MatchLdfld<LizardAI.LizardTrackState>("vultureMask"),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out label))
        && label is not null)
        {
            c.Next.OpCode = OpCodes.Ldarg_0;
            c.Index++;
            c.Emit<ArtificialIntelligence>(OpCodes.Ldfld, "creature")
             .Emit<AbstractCreature>(OpCodes.Ldfld, "creatureTemplate")
             .Emit<CreatureTemplate>(OpCodes.Ldfld, "type")
             .Emit(OpCodes.Ldsfld, il.Import(typeof(CreatureTemplateType).GetField("MoleSalamander")))
             .Emit(OpCodes.Beq, label)
             .Emit(OpCodes.Ldarg_1);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.IUseARelationshipTracker.UpdateDynamicRelationship!");
    }

    internal static CreatureTemplate.Relationship On_LizardAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.LizardAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, LizardAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var res = orig(self, dRelation);
        if (self.creature.creatureTemplate.type == CreatureTemplateType.NoodleEater)
        {
            var rel = dRelation.trackerRep.representedCreature.creatureTemplate.type;
            if ((rel == CreatureTemplate.Type.Slugcat && res.type == CreatureTemplate.Relationship.Type.Eats) || res.type == CreatureTemplate.Relationship.Type.Attacks)
                res.type = CreatureTemplate.Relationship.Type.Afraid;
            if (rel?.value == "DrainMite")
                res = new(CreatureTemplate.Relationship.Type.Eats, 1f);
            else if (rel?.value == "SnootShootNoot")
                res = new(CreatureTemplate.Relationship.Type.Afraid, .15f);
        }
        return res;
    }

    internal static bool On_LizardBreedParams_get_WallClimber(Func<LizardBreedParams, bool> orig, LizardBreedParams self) => self.template == CreatureTemplateType.HunterSeeker || self.template == CreatureTemplateType.NoodleEater || orig(self);

    internal static CreatureTemplate On_LizardBreeds_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
    {
        CreatureTemplate temp;
        LizardBreedParams breedParams;
        if (type == CreatureTemplateType.SilverLizard)
        {
            temp = orig(CreatureTemplate.Type.GreenLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "SilverLizard";
            breedParams.template = type;
            breedParams.baseSpeed = 3.1f;
            breedParams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[4] = new(.1f, 1f, 1f, 1f);
            breedParams.terrainSpeeds[5] = new(.1f, 1f, 1f, 1f);
            breedParams.standardColor = new(.654f, .811f, .858f);
            breedParams.biteDelay = 3;
            breedParams.biteInFront = 20f;
            breedParams.biteRadBonus = 20f;
            breedParams.biteHomingSpeed = 4.5f;
            breedParams.biteChance = .9f;
            breedParams.attemptBiteRadius = 120f;
            breedParams.getFreeBiteChance = 1f;
            breedParams.biteDamage = 2f;
            breedParams.biteDamageChance = .4f;
            breedParams.toughness = 5f;
            breedParams.stunToughness = 4f;
            breedParams.regainFootingCounter = 1;
            breedParams.bodyMass = 4f;
            breedParams.bodySizeFac = 1.6f;
            breedParams.floorLeverage = 8f;
            breedParams.maxMusclePower = 12f;
            breedParams.wiggleSpeed = .4f;
            breedParams.wiggleDelay = 20;
            breedParams.bodyStiffnes = .4f;
            breedParams.swimSpeed = 1.1f;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 10;
            breedParams.danger = .6f;
            breedParams.aggressionCurveExponent = .7f;
            breedParams.headShieldAngle = 160f;
            breedParams.canExitLounge = false;
            breedParams.canExitLoungeWarmUp = false;
            breedParams.findLoungeDirection = .5f;
            breedParams.loungeDistance = 200f;
            breedParams.preLoungeCrouch = 35;
            breedParams.preLoungeCrouchMovement = -.4f;
            breedParams.loungeSpeed = 2.7f;
            breedParams.loungeMaximumFrames = 30;
            breedParams.loungePropulsionFrames = 30;
            breedParams.loungeJumpyness = .6f;
            breedParams.loungeDelay = 60;
            breedParams.riskOfDoubleLoungeDelay = .1f;
            breedParams.postLoungeStun = 15;
            breedParams.loungeTendensy = 1f;
            temp.visualRadius = 2300f;
            temp.waterVision = .7f;
            temp.throughSurfaceVision = .95f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 4f / 9f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 7f / 9f);
            breedParams.biteDominance = 1f;
            breedParams.limbSize = 2.9f;
            breedParams.stepLength = 1f;
            breedParams.liftFeet = .3f;
            breedParams.feetDown = .5f;
            breedParams.noGripSpeed = .25f;
            breedParams.limbSpeed = 6f;
            breedParams.limbQuickness = .6f;
            breedParams.limbGripDelay = 1;
            breedParams.smoothenLegMovement = true;
            breedParams.legPairDisplacement = .3f;
            breedParams.walkBob = 2.7f;
            breedParams.tailSegments = 16;
            breedParams.tailStiffness = 300f;
            breedParams.tailStiffnessDecline = .25f;
            breedParams.tailLengthFactor = .8f;
            breedParams.tailColorationStart = .9f;
            breedParams.tailColorationExponent = 8f;
            breedParams.headSize = 1.3f;
            breedParams.neckStiffness = .37f;
            breedParams.jawOpenAngle = 150f;
            breedParams.jawOpenLowerJawFac = .7666667f;
            breedParams.jawOpenMoveJawsApart = 25f;
            breedParams.headGraphics = new int[5];
            breedParams.framesBetweenLookFocusChange = 20;
            breedParams.tamingDifficulty = 6f;
            temp.movementBasedVision = .3f;
            temp.waterPathingResistance = 3f;
            temp.dangerousToPlayer = breedParams.danger;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GreenLizard);
            return temp;
        }
        else if (type == CreatureTemplateType.NoodleEater)
        {
            temp = orig(CreatureTemplate.Type.BlueLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (temp.breedParameters as LizardBreedParams)!;
            temp.type = type;
            temp.name = "NoodleEater";
            breedParams.template = type;
            breedParams.baseSpeed = 5.2f;
            breedParams.biteDamage = .5f;
            breedParams.biteDominance = 0f;
            breedParams.loungeTendensy = 0f;
            breedParams.toughness = .1f;
            breedParams.headShieldAngle = 0f;
            breedParams.tongueChance = .6f;
            breedParams.tamingDifficulty = 1f;
            breedParams.tailColorationStart = .4f;
            breedParams.headSize = .8f;
            breedParams.headGraphics = [2, 2, 2, 2, 2];
            breedParams.tongueAttackRange = 350f;
            breedParams.tongueWarmUp = 2;
            breedParams.tongueSegments = 7;
            breedParams.jawOpenAngle = 90f;
            breedParams.jawOpenLowerJawFac = .6666f;
            breedParams.jawOpenMoveJawsApart = 15f;
            breedParams.tailColorationExponent = 1.4f;
            breedParams.tailLengthFactor = 1.1f;
            breedParams.standardColor = NoodleEaterCritob.NEatColor;
            breedParams.limbSpeed = 9f;
            breedParams.limbSize *= .7f;
            breedParams.limbQuickness = .9f;
            breedParams.noGripSpeed = .6f;
            breedParams.toughness = 0.25f;
            breedParams.stunToughness = .4f;
            breedParams.tailSegments = 3;
            breedParams.walkBob = .2f;
            breedParams.stepLength = .2f;
            breedParams.limbThickness = 1.1f;
            breedParams.danger = 0f;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
            temp.abstractedLaziness = 10;
            temp.dangerousToPlayer = 0f;
            return temp;
        }
        else if (type == CreatureTemplateType.Polliwog)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            breedParams.terrainSpeeds[3] = new(1.1f, 1f, 1f, 1f);
            breedParams.bodySizeFac = .7f;
            breedParams.headSize = 1f;
            breedParams.limbSize = .7f;
            breedParams.tongue = true;
            breedParams.toughness = .5f;
            temp.name = "Polliwog";
            temp.type = type;
            temp.baseDamageResistance = .5f;
            temp.meatPoints = 5;
            temp.canSwim = true;
            temp.doPreBakedPathing = false;
            temp.requireAImap = true;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander);
            temp.throwAction = "Call";
            return temp;
        }
        if (type == CreatureTemplateType.WaterSpitter)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            temp.name = "WaterSpitter";
            temp.type = type;
            temp.canSwim = true;
            temp.requireAImap = true;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.DropToFloor] = new(40f, PathCost.Legality.Allowed);
            temp.pathingPreferencesConnections[(int)MovementConnection.MovementType.LizardTurn] = new(60f, PathCost.Legality.Allowed);
            temp.waterPathingResistance = .5f;
            temp.movementBasedVision = .6f;
            breedParams.biteChance = .4f;
            breedParams.biteDelay = 15;
            breedParams.biteInFront = 40f;
            breedParams.biteHomingSpeed = .7f;
            breedParams.attemptBiteRadius = 100f;
            breedParams.getFreeBiteChance = .65f;
            breedParams.biteDamage = 1.6f;
            breedParams.toughness = 2.5f;
            breedParams.stunToughness = 1.5f;
            breedParams.regainFootingCounter = 10;
            breedParams.baseSpeed = 4.5f;
            breedParams.bodySizeFac = 1.2f;
            breedParams.floorLeverage = 7f;
            breedParams.maxMusclePower = 16f;
            temp.dangerousToPlayer = breedParams.danger = .55f;
            breedParams.aggressionCurveExponent = .95f;
            breedParams.wiggleSpeed = .5f;
            breedParams.wiggleDelay = 25;
            breedParams.bodyStiffnes = .5f;
            breedParams.swimSpeed = .25f;
            breedParams.idleCounterSubtractWhenCloseToIdlePos = 0;
            breedParams.biteDominance = .8f;
            breedParams.headShieldAngle = 100f;
            breedParams.loungeTendensy = 0f;
            temp.visualRadius = 1200f;
            temp.waterVision = 1f;
            temp.throughSurfaceVision = 1f;
            breedParams.perfectVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 5f / 18f);
            breedParams.limbSize = 1.4f;
            breedParams.stepLength = .9f;
            breedParams.liftFeet = .5f;
            breedParams.feetDown = 1f;
            breedParams.noGripSpeed = .05f;
            breedParams.limbSpeed = 3f;
            breedParams.limbQuickness = .3f;
            breedParams.limbGripDelay = 1;
            breedParams.smoothenLegMovement = false;
            breedParams.legPairDisplacement = 1f;
            breedParams.standardColor = Color.white;
            breedParams.walkBob = 4f;
            breedParams.tailSegments = 8;
            breedParams.tailStiffness = 300f;
            breedParams.tailStiffnessDecline = .6f;
            breedParams.tailLengthFactor = 1.3f;
            breedParams.tailColorationStart = .05f;
            breedParams.tailColorationExponent = 4f;
            breedParams.headSize = 1.1f;
            breedParams.neckStiffness = 1f;
            breedParams.jawOpenAngle = 40f;
            breedParams.jawOpenLowerJawFac = .5f;
            breedParams.jawOpenMoveJawsApart = 14f;
            breedParams.framesBetweenLookFocusChange = 180;
            breedParams.tamingDifficulty = 3.8f;
            breedParams.tongue = false;
            breedParams.headGraphics = [1, 1, 1, 1, 2];
            temp.meatPoints = 7;
            temp.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
            temp.wormGrassImmune = true;
            temp.bodySize = 3f;
            temp.baseDamageResistance = 4f;
            temp.baseStunResistance = 2.5f;
            temp.damageRestistances[2, 0] = 2.5f;
            temp.damageRestistances[2, 1] = 3f;
            temp.throwAction = "Spit";
            temp.jumpAction = "N/A";
            return temp;
        }
        else if (type == CreatureTemplateType.HunterSeeker)
        {
            temp = orig(CreatureTemplate.Type.WhiteLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            temp.dangerousToPlayer = breedParams.danger = .65f;
            temp.type = type;
            temp.name = "HunterSeeker";
            temp.throwAction = "Camouflage/Launch";
            temp.requireAImap = true;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.WhiteLizard);
            return temp;
        }
        else if (type == CreatureTemplateType.MoleSalamander)
        {
            temp = orig(CreatureTemplate.Type.Salamander, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
            breedParams = (LizardBreedParams)temp.breedParameters;
            breedParams.template = type;
            breedParams.headSize = 1.06f;
            breedParams.biteInFront = 26f;
            breedParams.biteDamage = 1f;
            breedParams.biteDamageChance = 5f / 14f;
            breedParams.stunToughness = 1f;
            breedParams.regainFootingCounter = 8;
            breedParams.baseSpeed = 4f;
            breedParams.bodySizeFac = .76f;
            breedParams.bodyMass = 1.5f;
            breedParams.floorLeverage = .35f;
            breedParams.maxMusclePower = 4.8f;
            breedParams.danger = .45f;
            breedParams.periferalVisionAngle = Mathf.Lerp(1f, -1f, 0f);
            breedParams.biteDominance = .5f;
            breedParams.limbSize = .7f;
            breedParams.stepLength = .6f;
            breedParams.liftFeet = .25f;
            breedParams.limbQuickness = .6f;
            breedParams.standardColor = new(.1f, .1f, .1f);
            breedParams.walkBob = 3f;
            breedParams.tailSegments = 12;
            breedParams.tailStiffness = 50f;
            breedParams.tailStiffnessDecline = .6f;
            breedParams.tailLengthFactor = 2f;
            breedParams.tailColorationStart = .5f;
            breedParams.jawOpenMoveJawsApart = 14f;
            breedParams.framesBetweenLookFocusChange = 100;
            breedParams.tamingDifficulty = 4f;
            breedParams.jawOpenAngle = 40f;
            breedParams.jawOpenLowerJawFac = .3f;
            temp.dangerousToPlayer = breedParams.danger;
            temp.visualRadius = .001f;
            temp.waterVision = 40000f;
            temp.throughSurfaceVision = 0f;
            temp.doPreBakedPathing = false;
            temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Salamander);
            temp.requireAImap = true;
            temp.name = "Mole Salamander";
            temp.type = CreatureTemplateType.MoleSalamander;
            temp.BlizzardWanderer = true;
            temp.BlizzardAdapted = true;
            temp.throughSurfaceVision = 0f;
            return temp;
        }
        return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
    }

    internal static void On_LizardBubble_ctor(On.LizardBubble.orig_ctor orig, LizardBubble self, LizardGraphics lizardGraphics, float intensity, float stickiness, float extraSpeed)
    {
        orig(self, lizardGraphics, intensity, stickiness, extraSpeed);
        if (lizardGraphics.lizard?.Template.type == CreatureTemplateType.NoodleEater)
            self.lifeTime /= 2;
    }

    internal static void On_LizardBubble_DrawSprites(On.LizardBubble.orig_DrawSprites orig, LizardBubble self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.slatedForDeletetion && self.room == rCam.room && self.lizardGraphics?.lizard?.Template.type == CreatureTemplateType.NoodleEater)
        {
            sLeaser.sprites[0].color = self.lizardGraphics.effectColor;
            if (self.lizardGraphics.lizard.dead)
                sLeaser.sprites[0].isVisible = false;
        }
    }

    internal static Color On_LizardGraphics_get_effectColor(Func<LizardGraphics, Color> orig, LizardGraphics self)
    {
        var color = orig(self);
        if (self.lizard.Template.type == CreatureTemplateType.WaterSpitter)
            color = Color.Lerp(self.palette.waterSurfaceColor1, Color.white, .1f);
        else if (self.lizard.Template.type == CreatureTemplateType.MoleSalamander && self.blackSalamander)
            color = self.palette.blackColor;
        return color;
    }

    internal static Color On_LizardGraphics_get_HeadColor1(Func<LizardGraphics, Color> orig, LizardGraphics self)
    {
        if (self.lizard.Template.type == CreatureTemplateType.MoleSalamander)
            return self.blackSalamander ? Color.Lerp(self.palette.blackColor, new(.5f, .5f, .5f), self.blackLizardLightUpHead) : self.SalamanderColor;
        if (self.lizard.Template.type == CreatureTemplateType.HunterSeeker)
            return Color.Lerp(Color.white, self.whiteCamoColor, self.whiteCamoColorAmount);
        return orig(self);
    }

    internal static Color On_LizardGraphics_get_HeadColor2(Func<LizardGraphics, Color> orig, LizardGraphics self)
    {
        if (self.lizard.Template.type == CreatureTemplateType.HunterSeeker)
            return Color.Lerp(self.palette.blackColor, self.whiteCamoColor, self.whiteCamoColorAmount);
        if (self.lizard.Template.type == CreatureTemplateType.MoleSalamander)
            return self.blackSalamander ? Color.Lerp(self.palette.blackColor, new(.5f, .5f, .5f), self.blackLizardLightUpHead) : self.SalamanderColor;
        return orig(self);
    }

    internal static void On_LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        var tpl = self.lizard.Template.type;
        Random.State state;
        int spriteIndex;
        if (tpl == CreatureTemplateType.SilverLizard)
        {
            state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            spriteIndex = self.startOfExtraSprites + self.extraSprites;
            spriteIndex = self.AddCosmetic(spriteIndex, new AxolotlGills(self, spriteIndex));
            if (Random.value < .2f)
                spriteIndex = self.AddCosmetic(spriteIndex, new LongHeadScales(self, spriteIndex));
            if (Random.value < .3f)
                self.AddCosmetic(spriteIndex, new TailGeckoScales(self, spriteIndex));
            Random.state = state;
        }
        else if (tpl == CreatureTemplateType.NoodleEater)
        {
            state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            self.overrideHeadGraphic = -1;
            self.bodyLength *= .75f;
            self.iVars.fatness = .65f + Random.value * .1f;
            self.iVars.tailColor = 4f;
            Random.state = state;
        }
        else if (tpl == CreatureTemplateType.Polliwog)
        {
            state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            spriteIndex = self.startOfExtraSprites + self.extraSprites;
            spriteIndex = self.AddCosmetic(spriteIndex, new AxolotlGills(self, spriteIndex));
            self.AddCosmetic(spriteIndex, new TailFin(self, spriteIndex));
            Random.state = state;
            self.overrideHeadGraphic = -1;
        }
        else if (tpl == CreatureTemplateType.WaterSpitter)
        {
            state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            spriteIndex = self.startOfExtraSprites + self.extraSprites;
            spriteIndex = self.AddCosmetic(spriteIndex, new Whiskers(self, spriteIndex));
            spriteIndex = self.AddCosmetic(spriteIndex, new TailFin(self, spriteIndex));
            spriteIndex = self.AddCosmetic(spriteIndex, new AxolotlGills(self, spriteIndex));
            if (Random.value < .4)
                spriteIndex = self.AddCosmetic(spriteIndex, new LongShoulderScales(self, spriteIndex));
            if (Random.value < .4)
                self.AddCosmetic(spriteIndex, new ShortBodyScales(self, spriteIndex));
            Random.state = state;
            self.overrideHeadGraphic = -1;
        }
        else if (tpl == CreatureTemplateType.HunterSeeker)
        {
            state = Random.state;
            Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
            spriteIndex = self.startOfExtraSprites + self.extraSprites;
            if (Random.value < .5f)
                spriteIndex = self.AddCosmetic(spriteIndex, new TailGeckoScales(self, spriteIndex));
            for (var k = 0; k < self.lizard.lizardParams.tailSegments; k++)
            {
                var num3 = Mathf.InverseLerp(0f, self.lizard.lizardParams.tailSegments - 1, k);
                var tk = self.tail[k];
                tk.rad += Mathf.Sin(Mathf.Pow(num3, .7f) * Mathf.PI) * 2.5f;
                tk.rad *= 1f - Mathf.Sin(Mathf.InverseLerp(0f, .4f, num3) * Mathf.PI) * .5f;
            }
            spriteIndex = self.AddCosmetic(spriteIndex, new WingScales(self, spriteIndex));
            spriteIndex = self.AddCosmetic(spriteIndex, new WingScales(self, spriteIndex));
            spriteIndex = (Random.value >= .5f || self.iVars.tailColor != 0f) ? self.AddCosmetic(spriteIndex, new TailGeckoScales(self, spriteIndex)) : self.AddCosmetic(spriteIndex, new TailTuft(self, spriteIndex));
            spriteIndex = self.AddCosmetic(spriteIndex, new JumpRings(self, spriteIndex));
            if (Random.value < .4f)
                spriteIndex = self.AddCosmetic(spriteIndex, new BumpHawk(self, spriteIndex));
            else if (Random.value < .4f)
                spriteIndex = self.AddCosmetic(spriteIndex, new ShortBodyScales(self, spriteIndex));
            else if (Random.value < .2f)
                spriteIndex = self.AddCosmetic(spriteIndex, new LongShoulderScales(self, spriteIndex));
            else if (Random.value < .2f)
                spriteIndex = self.AddCosmetic(spriteIndex, new LongHeadScales(self, spriteIndex));
            if (Random.value < .5f)
                self.AddCosmetic(spriteIndex, new TailTuft(self, spriteIndex));
            Random.state = state;
        }
        else if (tpl == CreatureTemplateType.MoleSalamander)
        {
            spriteIndex = self.startOfExtraSprites + self.extraSprites;
            spriteIndex = self.AddCosmetic(spriteIndex, new AxolotlGills(self, spriteIndex));
            spriteIndex = self.AddCosmetic(spriteIndex, new TailFin(self, spriteIndex));
            self.AddCosmetic(spriteIndex, new Whiskers(self, spriteIndex));
            self.overrideHeadGraphic = -1;
        }
    }

    internal static int On_LizardGraphics_AddCosmetic(On.LizardGraphics.orig_AddCosmetic orig, LizardGraphics self, int spriteIndex, Template cosmetic)
    {
        if (self.lizard?.Template.type == CreatureTemplateType.NoodleEater)
            return spriteIndex;
        return orig(self, spriteIndex, cosmetic);
    }

    internal static void On_LizardGraphics_ApplyPalette(On.LizardGraphics.orig_ApplyPalette orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.lizard is Lizard l)
        {
            if (!self.debugVisualization && l.Template.type == CreatureTemplateType.HunterSeeker)
                self.ColorBody(sLeaser, Color.white);
            else if (l.Template.type == CreatureTemplateType.MoleSalamander && !self.blackSalamander)
                self.ColorBody(sLeaser, self.SalamanderColor);
        }
    }

    internal static Color On_LizardGraphics_BodyColor(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
    {
        var res = orig(self, f);
        if (self.lizard is Lizard l)
        {
            if (l.Template.type == CreatureTemplateType.MoleSalamander && !self.blackSalamander)
                res = self.SalamanderColor;
            else if (l.Template.type == CreatureTemplateType.HunterSeeker)
                res = self.DynamicBodyColor(f);
        }
        return res;
    }

    internal static void On_LizardGraphics_CreatureSpotted(On.LizardGraphics.orig_CreatureSpotted orig, LizardGraphics self, bool firstSpot, Tracker.CreatureRepresentation crit)
    {
        if (self.lizard.Template.type == CreatureTemplateType.MoleSalamander)
            self.blackLizardLightUpHead = Mathf.Min(self.blackLizardLightUpHead + .5f, 1f);
        orig(self, firstSpot, crit);
    }

    internal static void On_LizardGraphics_DrawSprites(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var flag = !self.culled && !self.debugVisualization;
        if (flag && self.lizard is Lizard liz1 && liz1.Template.type == CreatureTemplateType.HunterSeeker)
        {
            self.ColorBody(sLeaser, self.DynamicBodyColor(0f));
            Color color = rCam.PixelColorAtCoordinate(liz1.mainBodyChunk.pos),
                color2 = rCam.PixelColorAtCoordinate(liz1.bodyChunks[1].pos),
                color3 = rCam.PixelColorAtCoordinate(liz1.bodyChunks[2].pos);
            if (color == color2)
                self.whitePickUpColor = color;
            else if (color2 == color3)
                self.whitePickUpColor = color2;
            else if (color3 == color)
                self.whitePickUpColor = color3;
            else
                self.whitePickUpColor = (color + color2 + color3) / 3f;
            if (self.whiteCamoColorAmount == -1f)
            {
                self.whiteCamoColor = self.whitePickUpColor;
                self.whiteCamoColorAmount = 1f;
            }
        }
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (flag && self.lizard is Lizard liz)
        {
            var sprites = sLeaser.sprites;
            var tpl = liz.Template.type;
            if (tpl == CreatureTemplateType.NoodleEater)
            {
                var eye = sprites[self.SpriteHeadStart + 4];
                eye.element = Futile.atlasManager.GetElementWithName((liz.Consious ? "NoodleEaterEye" : "NoodleEaterEyeDead") + (3 - (int)(Mathf.Abs(Mathf.Lerp(self.lastHeadDepthRotation, self.headDepthRotation, timeStacker)) * 3.9f)).ToString());
                eye.color = liz.Consious ? self.effectColor : rCam.currentPalette.blackColor;
                var num8 = self.SpriteLimbsColorStart - self.SpriteLimbsStart;
                for (var l = self.SpriteLimbsStart; l < self.SpriteLimbsEnd; l++)
                    sprites[l + num8].color = self.palette.blackColor;
                if (liz.tongue is LizardTongue t && t.Out)
                {
                    sprites[self.SpriteTongueStart + 1].color = self.effectColor;
                    var verts = (sprites[self.SpriteTongueStart] as TriangleMesh)!.verticeColors;
                    for (var num18 = 0; num18 < verts.Length; num18++)
                        verts[num18] = self.effectColor;
                }
            }
            else if (tpl == CreatureTemplateType.Polliwog)
            {
                for (int num8 = self.SpriteLimbsColorStart - self.SpriteLimbsStart, l = self.SpriteLimbsStart + 2; l < self.SpriteLimbsEnd; l++)
                {
                    sprites[l].isVisible = false;
                    sprites[l + num8].isVisible = false;
                }
            }
            else if (tpl == CreatureTemplateType.WaterSpitter)
            {
                for (var i = 7; i < 11; i++)
                    sprites[i + 9].color = Color.Lerp(self.palette.waterSurfaceColor1, Color.white, .1f);
            }
            else if (tpl == CreatureTemplateType.HunterSeeker)
            {
                var num8 = self.SpriteLimbsColorStart - self.SpriteLimbsStart;
                for (var m = self.SpriteLimbsStart; m < self.SpriteLimbsEnd; m++)
                {
                    var s = sprites[m + num8];
                    s.alpha = Mathf.Sin(self.whiteCamoColorAmount * Mathf.PI) * .3f;
                    s.color = self.palette.blackColor;
                }
            }
            else if (tpl == CreatureTemplateType.MoleSalamander)
            {
                var num2 = Mathf.Lerp(liz.lastJawOpen, liz.JawOpen, timeStacker);
                if (liz.JawReadyForBite && liz.Consious)
                    num2 += Random.value * .2f;
                num2 = Mathf.Lerp(num2, Mathf.Lerp(self.lastVoiceVisualization, self.voiceVisualization, timeStacker) + .2f, Mathf.Lerp(self.lastVoiceVisualizationIntensity, self.voiceVisualizationIntensity, timeStacker) * .8f);
                num2 = Mathf.Clamp(num2, 0f, 1f);
                for (var m = 7; m < 11; m++)
                {
                    sprites[m + 9].color = !self.blackSalamander ? self.effectColor : self.palette.blackColor;
                    sprites[m + 9].alpha = !self.blackSalamander ? (m % 2 != 1 ? .3f : Mathf.Lerp(.3f, .1f, Mathf.Abs(Mathf.Lerp(self.lastDepthRotation, self.depthRotation, timeStacker)))) : Mathf.Sin(self.whiteCamoColorAmount * Mathf.PI) * .3f;
                }
                if (self.blackSalamander)
                    sprites[13].color = Color.Lerp(self.palette.blackColor, new(.5f, .5f, .5f), Mathf.Pow(self.blackLizardLightUpHead, 1f - .95f * num2));
                sprites[15].isVisible = false;
            }
        }
    }

    internal static Color On_LizardGraphics_DynamicBodyColor(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
    {
        var tpl = self.lizard.Template.type;
        if (tpl == CreatureTemplateType.NoodleEater)
            return self.palette.blackColor;
        if (tpl == CreatureTemplateType.HunterSeeker)
            return Color.Lerp(Color.white, self.whiteCamoColor, self.whiteCamoColorAmount);
        if (tpl == CreatureTemplateType.MoleSalamander && !self.blackSalamander)
            return self.SalamanderColor;
        return orig(self, f);
    }

    internal static LizardGraphics.IndividualVariations On_LizardGraphics_GenerateIvars(On.LizardGraphics.orig_GenerateIvars orig, LizardGraphics self)
    {
        var res = orig(self);
        if (self.lizard.Template.type == CreatureTemplateType.HunterSeeker)
            res.tailColor = Random.value > .5f ? Random.value : 0f;
        else if (self.lizard.Template.type == CreatureTemplateType.MoleSalamander)
            res.fatness = Custom.ClampedRandomVariation(.45f, .06f, .5f) * 2f;
        return res;
    }

    internal static Color On_LizardGraphics_HeadColor(On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
    {
        if (self.lizard.Template.type == CreatureTemplateType.NoodleEater)
            return self.palette.blackColor;
        var color = orig(self, timeStacker);
        if (self.lizard.AI?.yellowAI is PolliwogCommunication c && c.PackLeader)
        {
            var flicker = Mathf.Lerp(c.LastFlicker, c.CurrentFlicker, timeStacker);
            if (!self.lizard.Consious)
                flicker = 0f;
            color = Color.Lerp(color, new(1f, .007843137254902f, .3529411764705882f), flicker);
        }
        return color;
    }

    internal static void On_LizardGraphics_InitiateSprites(On.LizardGraphics.orig_InitiateSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.lizard?.Template.type == CreatureTemplateType.NoodleEater && self.tongue is GenericBodyPart[] t)
        {
            var tg = sLeaser.sprites[self.SpriteTongueStart];
            tg.isVisible = false;
            var cont = tg.container;
            var tlm1 = t.Length - 1;
            var array = new TriangleMesh.Triangle[tlm1 * 4 + 1];
            for (var n = 0; n < tlm1; n++)
            {
                var num = n * 4;
                for (var num2 = 0; num2 < 4; num2++)
                    array[num + num2] = new(num + num2, num + num2 + 1, num + num2 + 2);
            }
            array[tlm1 * 4] = new(tlm1 * 4, tlm1 * 4 + 1, tlm1 * 4 + 2);
            cont.AddChild(sLeaser.sprites[self.SpriteTongueStart] = new TriangleMesh("Futile_White", array, true));
            sLeaser.sprites[self.SpriteTongueStart].MoveBehindOtherNode(tg);
            tg.RemoveFromContainer();
        }
    }

    internal static void IL_LizardGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        c.Emit(OpCodes.Ldarg_0)
         .EmitDelegate((LizardGraphics self) =>
         {
             if (self.lizard?.Template.type == CreatureTemplateType.MoleSalamander)
             {
                 if (self.lizard.bubble > 0)
                     self.blackLizardLightUpHead = Mathf.Min(self.blackLizardLightUpHead + .1f, 1f);
                 else
                     self.blackLizardLightUpHead *= .9f;
             }
         });
        for (var i = 1; i <= 2; i++)
        {
            ILLabel? label = null;
            if (c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<LizardGraphics>("lizard"),
                x => x.MatchCallOrCallvirt<Creature>("get_Template"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdsfld<CreatureTemplate.Type>("Salamander"),
                x => x.MatchCall(out _),
                x => x.MatchBrtrue(out label))
            && label is not null)
            {
                c.Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((LizardGraphics self) => self.lizard is Lizard l && (l.IsPolliwog() || l.IsMoleSala()));
                c.Emit(OpCodes.Brtrue, label);
                c.Index += 7; // arbitrary num
            }
            else
                LBMergedModsPlugin.s_logger.LogError($"Couldn't ILHook LizardGraphics.Update! (part {i})");
        }
        var label2 = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallOrCallvirt<Creature>("get_Template"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Salamander"),
            x => x.MatchCall(out _),
            x => x.MatchBrfalse(out _))
        && label2 is not null)
        {
            label2.Target = c.Next;
            c.Index -= 6;
            c.EmitDelegate((LizardGraphics self) => self.lizard is Lizard l && (l.IsPolliwog() || l.IsMoleSala()));
            c.Emit(OpCodes.Brtrue, label2)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardGraphics.Update! (part 3)");
        c.Index = 0;
        for (var i = 0; i < 2; i++)
        {
            var label = il.DefineLabel();
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<LizardGraphics>("lizard"),
                x => x.MatchCallOrCallvirt<Creature>("get_Template"),
                x => x.MatchLdfld<CreatureTemplate>("type"),
                x => x.MatchLdsfld<CreatureTemplate.Type>("WhiteLizard"),
                x => x.MatchCall(out _),
                x => x.MatchBrfalse(out _)))
            {
                label.Target = c.Next;
                c.Index -= 6;
                c.EmitDelegate((LizardGraphics self) => self.lizard?.Template.type == CreatureTemplateType.HunterSeeker);
                c.Emit(OpCodes.Brtrue, label)
                 .Emit(OpCodes.Ldarg_0);
                if (i == 0)
                    c.Index += 12;
            }
            else
                LBMergedModsPlugin.s_logger.LogError($"Couldn't ILHook LizardGraphics.Update! (part {i + 4})");
        }
        c.Index = il.Instrs.Count - 1;
        c.Next.OpCode = OpCodes.Ldarg_0;
        ++c.Index;
        c.EmitDelegate((LizardGraphics self) =>
        {
            if (self.lightSource is LightSource l && self.lizard?.Template.type == CreatureTemplateType.MoleSalamander)
            {
                l.color = Color.white;
                l.setAlpha = .35f * self.blackLizardLightUpHead;
            }
        });
        c.Emit(OpCodes.Ret);
    }

    internal static void On_LizardGraphics_Update(On.LizardGraphics.orig_Update orig, LizardGraphics self)
    {
        orig(self);
        if (self.lizard is Lizard l && l.Template.type == CreatureTemplateType.NoodleEater)
        {
            if (!l.Consious)
            {
                l.bubble = 0;
                l.bubbleIntensity = 0f;
            }
            else
                l.bubbleIntensity /= 2f;
            if (self.lightSource is LightSource ls)
                ls.setAlpha = 0f;
        }
    }

    internal static void IL_LizardGraphics_UpdateTailSegment(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardGraphics>("lizard"),
            x => x.MatchCallOrCallvirt<Creature>("get_Template"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Salamander"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label))
        && label is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((LizardGraphics self) => self.lizard is Lizard l && (l.IsPolliwog() || l.IsMoleSala()));
            c.Emit(OpCodes.Brtrue, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardGraphics.UpdateTailSegment!");
    }

    internal static void On_LizardGraphics_WhiteFlicker(On.LizardGraphics.orig_WhiteFlicker orig, LizardGraphics self, int fl)
    {
        if (self.lizard.Template.type == CreatureTemplateType.NoodleEater)
            self.whiteFlicker = 0;
        else
            orig(self, fl);
    }

    internal static bool On_LizardJumpModule_get_canChainJump(Func<LizardJumpModule, bool> orig, LizardJumpModule self) => (self.lizard is Lizard l && l.Template.type == CreatureTemplateType.HunterSeeker && l.grasps[0] is null) || orig(self);

    internal static void On_LizardLimb_ctor(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
    {
        orig(self, owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness, otherLimbInPair);
        if (owner is LizardGraphics l && l.lizard is Lizard liz)
        {
            var tpl = liz.Template.type;
            if (tpl == CreatureTemplateType.NoodleEater)
            {
                self.grabSound = SoundID.Lizard_BlueWhite_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_BlueWhite_Foot_Grab;
            }
            else if (tpl == CreatureTemplateType.SilverLizard)
            {
                self.grabSound = SoundID.Lizard_Green_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_Green_Foot_Release;
            }
        }
    }

    internal static PathCost On_LizardPather_HeuristicForCell(On.LizardPather.orig_HeuristicForCell orig, LizardPather self, PathFinder.PathingCell cell, PathCost costToGoal)
    {
        if (self.creature.creatureTemplate.type == CreatureTemplateType.Polliwog)
            return costToGoal;
        else if ((self.creature.creatureTemplate.type == CreatureTemplateType.WaterSpitter || self.creature.creatureTemplate.type == CreatureTemplateType.MoleSalamander) && self.InThisRealizedRoom(cell.worldCoordinate) && self.creature.Room.realizedRoom.aimap.getAItile(cell.worldCoordinate).AnyWater)
            return new(cell.worldCoordinate.Tile.FloatDist(self.creaturePos.Tile), costToGoal.legality);
        return orig(self, cell, costToGoal);
    }

    internal static void On_LizardSpit_AddToContainer(On.LizardSpit.orig_AddToContainer orig, LizardSpit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
    {
        orig(self, sLeaser, rCam, newContainer);
        if (self is LizardWaterSpit)
        {
            newContainer = rCam.ReturnFContainer("Background");
            var sprs = sLeaser.sprites;
            for (var i = 0; i < sprs.Length; i++)
            {
                var s = sprs[i];
                s.RemoveFromContainer();
                newContainer.AddChild(s);
            }
        }
    }

    internal static void On_LizardSpit_ApplyPalette(On.LizardSpit.orig_ApplyPalette orig, LizardSpit self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self is LizardWaterSpit)
        {
            var sprites = sLeaser.sprites;
            sprites[self.JaggedSprite].color = palette.waterColor1;
            sprites[self.DotSprite].color = palette.waterColor1;
            for (var i = 0; i < self.slime.GetLength(0); i++)
                sprites[self.SlimeSprite(i)].color = palette.waterColor1;
        }
    }

    internal static void IL_LizardSpit_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var instr = ins[i];
            if (instr.MatchLdsfld<SoundID>("Red_Lizard_Spit_Hit_NPC") || instr.MatchLdsfld<SoundID>("Red_Lizard_Spit_Hit_Player") || instr.MatchLdsfld<SoundID>("Red_Lizard_Spit_Hit_Wall"))
            {
                c.Goto(instr, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((SoundID ID, LizardSpit self) => self is LizardWaterSpit ? SoundID.Swollen_Water_Nut_Terrain_Impact : ID);
            }
        }
    }

    internal static void IL_LizardSpitTracker_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<LizardAI.LizardSpitTracker>("get_lizardAI"),
            x => x.MatchCallOrCallvirt<LizardAI>("get_lizard"),
            x => x.MatchLdflda<Creature>("lastInputWithDiagonals"),
            x => x.MatchCallOrCallvirt(out _),
            x => x.MatchLdfld<Player.InputPackage>("thrw")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, LizardAI.LizardSpitTracker self) => flag && self.lizardAI?.lizard?.Template.type != CreatureTemplateType.WaterSpitter);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.LizardSpitTracker.Update!");
    }

    internal static void On_LizardTongue_ctor(On.LizardTongue.orig_ctor orig, LizardTongue self, Lizard lizard)
    {
        orig(self, lizard);
        var tpl = lizard.Template.type;
        if (tpl == CreatureTemplateType.NoodleEater)
        {
            self.range = 400f;
            self.elasticRange = 0f;
            self.lashOutSpeed = 30f;
            self.reelInSpeed = 0f;
            self.chunkDrag = 0f;
            self.terrainDrag = 0f;
            self.dragElasticity = 0f;
            self.emptyElasticity = .07f;
            self.involuntaryReleaseChance = .0333333341f;
            self.voluntaryReleaseChance = 1f;
            self.baseDragOnly = true;
            self.totR = self.range * 1.1f;
        }
        else if (tpl == CreatureTemplateType.Polliwog)
        {
            self.range = 140f;
            self.lashOutSpeed = 16f;
            self.reelInSpeed = .000625f;
            self.terrainDrag = self.chunkDrag = .01f;
            self.dragElasticity = .1f;
            self.emptyElasticity = .8f;
            self.involuntaryReleaseChance = 1f / 400f;
            self.voluntaryReleaseChance = .0125f;
            self.elasticRange = .55f;
            self.baseDragOnly = true;
            self.totR = self.range * 1.1f;
        }
        else if (tpl == CreatureTemplateType.HunterSeeker)
        {
            self.range = 140f;
            self.lashOutSpeed = 16f;
            self.reelInSpeed = .002f;
            self.chunkDrag = .01f;
            self.terrainDrag = .01f;
            self.dragElasticity = .1f;
            self.emptyElasticity = .8f;
            self.involuntaryReleaseChance = .005f;
            self.voluntaryReleaseChance = .02f;
            self.elasticRange = .55f;
            self.totR = self.range * 1.1f;
        }
        else if (tpl == CreatureTemplateType.MoleSalamander)
        {
            self.range = 140f;
            self.lashOutSpeed = 16f;
            self.reelInSpeed = .000625f;
            self.chunkDrag = .01f;
            self.terrainDrag = .01f;
            self.dragElasticity = .1f;
            self.emptyElasticity = .8f;
            self.involuntaryReleaseChance = .0025f;
            self.voluntaryReleaseChance = .0125f;
            self.elasticRange = .55f;
            self.totR = self.range * 1.1f;
        }
    }

    internal static void On_LizardVoice_ctor(On.LizardVoice.orig_ctor orig, LizardVoice self, Lizard lizard)
    {
        orig(self, lizard);
        if (lizard.Template.type == CreatureTemplateType.NoodleEater)
            self.myPitch *= 1.5f;
    }

    internal static SoundID On_LizardVoice_GetMyVoiceTrigger(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
    {
        var res = orig(self);
        List<SoundID> list;
        SoundID soundID;
        if (self.lizard is Lizard l)
        {
            var tpl = l.Template.type;
            if (tpl == CreatureTemplateType.SilverLizard || tpl == CreatureTemplateType.HunterSeeker)
            {
                var array = new[]
                {
                    SoundID.Lizard_Voice_Pink_A,
                    SoundID.Lizard_Voice_Pink_B,
                    SoundID.Lizard_Voice_Pink_C,
                    SoundID.Lizard_Voice_Pink_D,
                    SoundID.Lizard_Voice_Pink_E
                };
                list = [];
                for (var i = 0; i < array.Length; i++)
                {
                    soundID = array[i];
                    if (soundID.Index != -1 && l.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            else if (tpl == CreatureTemplateType.NoodleEater)
            {
                soundID = SoundID.Lizard_Voice_Blue_A;
                if (soundID.Index != -1 && l.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    res = soundID;
                else
                    res = SoundID.None;
            }
            else if (tpl == CreatureTemplateType.Polliwog)
            {
                var array = new[] { "A", "B" };
                list = [];
                for (var i = 0; i < array.Length; i++)
                {
                    soundID = new("Lizard_Voice_Salamander_" + array[i]);
                    if (soundID.Index != -1 && l.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                        list.Add(soundID);
                }
                if (list.Count == 0)
                    res = SoundID.None;
                else
                    res = list[Random.Range(0, list.Count)];
            }
            else if (tpl == CreatureTemplateType.WaterSpitter)
            {
                soundID = SoundID.Lizard_Voice_Green_A;
                if (soundID.Index != -1 && l.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    res = soundID;
                else
                    res = SoundID.None;
            }
            else if (tpl == CreatureTemplateType.WaterSpitter)
            {
                soundID = MMFEnums.MMFSoundID.Lizard_Voice_Black_A;
                if (soundID is not null && soundID.Index != -1 && l.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    res = soundID;
                else
                    res = SoundID.None;
            }
        }
        return res;
    }

    internal static void On_LongBodyScales_DrawSprites(On.LizardCosmetics.LongBodyScales.orig_DrawSprites orig, LongBodyScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is LizardGraphics l && l.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            Color c = l.BodyColor(1f), c2 = l.HeadColor(timeStacker);
            var sprs = sLeaser.sprites;
            for (var num = self.startSprite + self.scalesPositions.Length - 1; num >= self.startSprite; num--)
            {
                sprs[num].color = c;
                if (self.colored)
                    sprs[num + self.scalesPositions.Length].color = c2;
            }
        }
    }

    internal static void On_LongHeadScales_ctor(On.LizardCosmetics.LongHeadScales.orig_ctor orig, LongHeadScales self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics.lizard.Template.type == CreatureTemplateType.MoleSalamander)
        {
            self.colored = !lGraphics.blackSalamander;
            self.numberOfSprites = !self.colored ? self.scalesPositions.Length : (self.scalesPositions.Length * 2);
        }
    }

    internal static void On_LongHeadScales_DrawSprites(On.LizardCosmetics.LongHeadScales.orig_DrawSprites orig, LongHeadScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.colored && self.lGraphics is LizardGraphics l && l.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            var c = l.HeadColor(timeStacker);
            var sprs = sLeaser.sprites;
            for (var num = self.startSprite + self.scalesPositions.Length - 1; num >= self.startSprite; num--)
                sprs[num + self.scalesPositions.Length].color = c;
        }
    }

    internal static void IL_LurkTracker_LurkPosScore(ILContext il)
    {
        var c = new ILCursor(il);
        ILLabel? label = il.DefineLabel();
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardAI.LurkTracker>("lizard"),
            x => x.MatchCallOrCallvirt<Creature>("get_Template"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("WhiteLizard"),
            x => x.MatchCall(out _),
            x => x.MatchBrfalse(out _)))
        {
            label.Target = c.Next;
            c.Index -= 6;
            c.EmitDelegate((LizardAI.LurkTracker self) => self.lizard?.Template.type == CreatureTemplateType.HunterSeeker);
            c.Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.LurkTracker.LurkPosScore! (part 1)");
        label = null;
        if (c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<LizardAI.LurkTracker>("lizard"),
            x => x.MatchCallOrCallvirt<Creature>("get_Template"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("Salamander"),
            x => x.MatchCall(out _),
            x => x.MatchBrtrue(out label))
        && label is not null)
        {
            ++c.Index;
            c.EmitDelegate((LizardAI.LurkTracker self) => self.lizard is Lizard l && (l.IsPolliwog() || l.IsMoleSala()));
            c.Emit(OpCodes.Brtrue, label)
             .Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook LizardAI.LurkTracker.LurkPosScore! (part 2)");
    }

    internal static float On_LurkTracker_Utility(On.LizardAI.LurkTracker.orig_Utility orig, LizardAI.LurkTracker self)
    {
        var res = orig(self);
        if (self.lizard is Lizard l)
        {
            var tpl = l.Template.type;
            if (tpl == CreatureTemplateType.Polliwog || tpl == CreatureTemplateType.MoleSalamander)
            {
                if (self.LurkPosScore(self.lurkPosition) > 0f)
                    res = l.room?.GetTile(self.lurkPosition).AnyWater is true ? .5f : .2f;
            }
            else if (tpl == CreatureTemplateType.HunterSeeker)
                res = .5f;
        }
        return res;
    }

    internal static void On_SpineSpikes_DrawSprites(On.LizardCosmetics.SpineSpikes.orig_DrawSprites orig, SpineSpikes self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is LizardGraphics g && g.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            Color bc = g.BodyColor(1f), hc = g.HeadColor(timeStacker);
            var sprs = sLeaser.sprites;
            for (var i = self.startSprite; i < self.startSprite + self.bumps; i++)
            {
                sprs[i].color = bc;
                if (self.colored == 1)
                    sprs[i + self.bumps].color = hc;
                else if (self.colored == 2)
                {
                    var f2 = Mathf.InverseLerp(self.startSprite, self.startSprite + self.bumps - 1, i);
                    sprs[i + self.bumps].color = Color.Lerp(hc, bc, Mathf.Pow(f2, .5f));
                }
            }
        }
    }

    internal static void On_TailFin_DrawSprites(On.LizardCosmetics.TailFin.orig_DrawSprites orig, TailFin self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.colored && self.lGraphics is LizardGraphics g && g.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            var sprs = sLeaser.sprites;
            for (var i = 0; i < 2; i++)
            {
                var num = i * self.bumps * 2;
                var col = g.HeadColor(timeStacker);
                for (var j = self.startSprite; j < self.startSprite + self.bumps; j++)
                    sprs[j + self.bumps + num].color = col;
            }
        }
    }

    internal static void IL_TailGeckoScales_DrawSprites(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCall<Color>("Lerp"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .Emit(OpCodes.Ldarg_3)
                 .EmitDelegate((Color color, TailGeckoScales self, float timeStacker) => self.lGraphics is LizardGraphics g && g.lizard?.Template.type == CreatureTemplateType.HunterSeeker ? Color.Lerp(color, g.HeadColor(timeStacker), g.whiteCamoColorAmount) : color);
            }
        }
    }

    internal static void On_Whiskers_ctor(On.LizardCosmetics.Whiskers.orig_ctor orig, Whiskers self, LizardGraphics lGraphics, int startSprite)
    {
        orig(self, lGraphics, startSprite);
        if (lGraphics.lizard?.IsWaterSpitter() is true)
            self.spritesOverlap = Template.SpritesOverlap.BehindHead;
    }

    internal static Vector2 On_Whiskers_AnchorPoint(On.LizardCosmetics.Whiskers.orig_AnchorPoint orig, Whiskers self, int side, int m, float timeStacker)
    {
        var vec = orig(self, side, m, timeStacker);
        if (self.lGraphics is LizardGraphics g && g.lizard is Lizard l)
        {
            if (l.IsWaterSpitter())
                vec = Vector2.Lerp(g.head.lastPos, g.head.pos, timeStacker) + Custom.DegToVec(g.HeadRotation(l, timeStacker)) * 7f * g.iVars.headSize + self.whiskerDir(side, m, timeStacker);
            else if (l.IsMoleSala())
                vec = Vector2.Lerp(g.head.lastPos, g.head.pos, timeStacker) + Custom.DegToVec(g.HeadRotation(l, timeStacker)) * 10.9f * g.iVars.headSize + self.whiskerDir(side, m, timeStacker);
        }
        return vec;
    }

    internal static void On_WingScales_DrawSprites(On.LizardCosmetics.WingScales.orig_DrawSprites orig, WingScales self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.lGraphics is LizardGraphics g && g.lizard?.Template.type == CreatureTemplateType.HunterSeeker)
        {
            var c = g.BodyColor(1f);
            var sprs = sLeaser.sprites;
            for (var i = 0; i < self.numberOfSprites; i++)
                sprs[self.startSprite + i].color = c;
        }
    }

    internal static YellowAI.YellowPack On_YellowAI_Pack(On.YellowAI.orig_Pack orig, YellowAI self, Creature liz)
    {
        if (liz?.abstractCreature?.abstractAI?.RealAI is not LizardAI ai || ai.yellowAI?.pack is null)
            return self.pack;
        return orig(self, liz);
    }

    internal static void IL_YellowAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<YellowAI>("lizard"),
            x => x.MatchLdflda<Creature>("inputWithDiagonals"),
            x => x.MatchCallOrCallvirt(out _),
            x => x.MatchLdfld<Player.InputPackage>("jmp")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, YellowAI self) => self is PolliwogCommunication && self.lizard is Lizard l ? l.inputWithDiagonals!.Value.thrw && self.pack?.members.Count > 1 : flag);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook YellowAI.Update! (part 1)");
        if (c.TryGotoNext(MoveType.After,
           x => x.MatchCall<Mathf>("Max"),
           x => x.MatchStfld<YellowAI>("commFlicker")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((YellowAI self) =>
             {
                 if (self is PolliwogCommunication co && self.lizard is Lizard l)
                 {
                     co.LastFlicker = co.CurrentFlicker;
                     co.CurrentFlicker = Mathf.Clamp(co.Increase ? co.CurrentFlicker + .25f : co.CurrentFlicker - .2f, -.5f, 1f);
                     if (co.CurrentFlicker >= 1f || !l.Consious || self.communicating <= 0)
                         co.Increase = false;
                     else if (self.communicating > 0 && co.CurrentFlicker <= -.5f && self.pack?.members.Count > 1)
                         co.Increase = true;
                 }
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook YellowAI.Update! (part 2)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdsfld<CreatureTemplate.Type>("YellowLizard")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CreatureTemplate.Type type, YellowAI self) => self is PolliwogCommunication ? CreatureTemplateType.Polliwog : type);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook YellowAI.Update! (part 3)");
    }

    internal static void On_YellowPack_FindLeader(On.YellowAI.YellowPack.orig_FindLeader orig, YellowAI.YellowPack self)
    {
        orig(self);
        var mems = self.members;
        for (var i = 0; i < mems.Count; i++)
        {
            var mem = mems[i];
            if (mem?.lizard?.realizedCreature is Lizard l && l.AI?.yellowAI is PolliwogCommunication c)
                c.PackLeader = mem.role == YellowAI.YellowPack.Role.Leader;
        }
    }

    internal static void On_YellowPack_RemoveLizard_AbstractCreature(On.YellowAI.YellowPack.orig_RemoveLizard_AbstractCreature orig, YellowAI.YellowPack self, AbstractCreature removeLizard)
    {
        var mems = self.members;
        for (var num = mems.Count - 1; num >= 0; num--)
        {
            if (mems[num]?.lizard == removeLizard && removeLizard?.realizedCreature is Lizard l && l.AI?.yellowAI is PolliwogCommunication c && c.PackLeader)
                c.PackLeader = false;
        }
        orig(self, removeLizard);
    }

    internal static void On_YellowPack_RemoveLizard_int(On.YellowAI.YellowPack.orig_RemoveLizard_int orig, YellowAI.YellowPack self, int index)
    {
        if (self.members[index]?.lizard?.realizedCreature is Lizard l && l.AI?.yellowAI is PolliwogCommunication c && c.PackLeader)
            c.PackLeader = false;
        orig(self, index);
    }

    internal static float HeadRotation(this LizardGraphics self, Lizard lizard, float timeStacker)
    {
        var num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(self.drawPositions[0, 1], self.drawPositions[0, 0], timeStacker), Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker));
        var num2 = Mathf.Lerp(self.lastHeadDepthRotation, self.headDepthRotation, timeStacker);
        var num3 = Mathf.Clamp(Mathf.Lerp(lizard.lastJawOpen, lizard.JawOpen, timeStacker), 0f, 1f);
        return num + lizard.lizardParams.jawOpenAngle * (1.5f - (lizard.lizardParams.jawOpenLowerJawFac / 3f)) * num3 * num2;
    }

    public static bool IsPolliwog(this Lizard self) => self.Template.type == CreatureTemplateType.Polliwog;

    public static bool IsWaterSpitter(this Lizard self) => self.Template.type == CreatureTemplateType.WaterSpitter;

    public static bool IsMoleSala(this Lizard self) => self.Template.type == CreatureTemplateType.MoleSalamander;

    public static void SpitWater(this Lizard self)
    {
        if (self.Submersion >= 1f || (self.grasps?.Length > 0 && self.grasps[0] is not null) || self.room is not Room rm || self.AI?.redSpitAI is not LizardAI.LizardSpitTracker a)
            return;
        self.bodyWiggleCounter = 0;
        self.JawOpen = Mathf.Clamp(self.JawOpen + .2f, 0f, 1f);
        var ctr = self.safariControlled;
        if (!a.spitting && !ctr)
            self.EnterAnimation(Lizard.Animation.Standard, true);
        else
        {
            var vector = a.AimPos();
            if (vector is Vector2 value)
            {
                BodyChunk mc = self.mainBodyChunk, b1 = self.bodyChunks[1], b0 = self.bodyChunks[0], b2 = self.bodyChunks[2];
                if (a.AtSpitPos)
                {
                    var vector2 = rm.MiddleOfTile(a.spitFromPos);
                    mc.vel += Vector2.ClampMagnitude(vector2 - Custom.DirVec(vector2, value) * self.bodyChunkConnections[0].distance - mc.pos, 10f) / 500f;
                    b1.vel += Vector2.ClampMagnitude(vector2 - b1.pos, 10f) / 500f;
                }
                if (!self.AI.UnpleasantFallRisk(rm.GetTilePosition(mc.pos)))
                {
                    var ltr = Custom.DirVec(mc.pos, value) * self.LegsGripping * .02f;
                    mc.vel += ltr * 2f;
                    b1.vel -= ltr;
                    b2.vel -= ltr;
                }
                if (a.delay < 1)
                {
                    var fl = Custom.DirVec(b1.pos, b0.pos);
                    Vector2 vector3 = b0.pos + fl * 10f, vector4 = Custom.DirVec(vector3, value);
                    if (Vector2.Dot(vector4, fl) > .3f || ctr)
                    {
                        if (ctr)
                        {
                            self.EnterAnimation(Lizard.Animation.Standard, true);
                            self.LoseAllGrasps();
                        }
                        rm.PlaySound(SoundID.Splashing_Water_Into_Terrain, vector3, 1.6f, 1.2f);
                        rm.AddObject(new LizardWaterSpit(vector3, vector4 * 28f, self));
                        a.delay = 0;
                        b2.pos -= vector4 * .01f;
                        b1.pos -= vector4 * .005f;
                        b2.vel -= vector4 * .0025f;
                        b1.vel -= vector4 * .00125f;
                        self.JawOpen = 1f;
                    }
                }
            }
        }
    }
}