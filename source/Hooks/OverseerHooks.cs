global using static LBMergedMods.Hooks.OverseerHooks;
using MonoMod.Cil;
using UnityEngine;
using Mono.Cecil.Cil;
using RWCustom;
using OverseerHolograms;

namespace LBMergedMods.Hooks;

public static class OverseerHooks
{
    internal static bool On_OverseerAbstractAI_AllowSwarmTarget(On.OverseerAbstractAI.orig_AllowSwarmTarget orig, OverseerAbstractAI self, AbstractCreature evalTarget, AbstractRoom roomCheck)
    {
        if (self.targetCreature.creatureTemplate.type == CreatureTemplateType.MiniBlackLeech && self.SafariSwarmCap > 0)
        {
            --self.SafariSwarmCap;
            if (Custom.Dist(self.targetCreature.pos.Tile.ToVector2(), evalTarget.pos.Tile.ToVector2()) > 6f)
                return true;
            if (self.targetCreature.creatureTemplate.type == evalTarget.creatureTemplate.type)
                return false;
        }
        return orig(self, evalTarget, roomCheck);
    }

    internal static void IL_OverseerAbstractAI_HowInterestingIsCreature(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            s_MatchLdarg_1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_BlackLizard,
            s_MatchCall_Any,
            s_MatchBrtrue_OutLabel))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((AbstractCreature testCrit) => testCrit.creatureTemplate.type == CreatureTemplateType.SilverLizard || testCrit.creatureTemplate.type == CreatureTemplateType.Polliwog || testCrit.creatureTemplate.type == CreatureTemplateType.WaterSpitter || testCrit.creatureTemplate.type == CreatureTemplateType.HunterSeeker || testCrit.creatureTemplate.type == CreatureTemplateType.MoleSalamander || testCrit.creatureTemplate.type == CreatureTemplateType.CommonEel);
            c.Emit(OpCodes.Brtrue, s_label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, AbstractCreature testCrit) => flag || testCrit.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerAbstractAI.HowInterestingIsCreature! (part 2)");
    }

    internal static void IL_OverseerAbstractAI_PlayerGuideUpdate(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Any,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, int i, OverseerAbstractAI self) => flag || self.RelevantPlayer.Room.creatures[i].creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerAbstractAI.PlayerGuideUpdate!");
    }

    internal static float On_OverseerAbstractAI_HowInterestingIsCreature(On.OverseerAbstractAI.orig_HowInterestingIsCreature orig, OverseerAbstractAI self, AbstractCreature testCrit)
    {
        var res = orig(self, testCrit);
        if (testCrit is not null)
        {
            var tpl = testCrit.creatureTemplate.type;
            if (tpl == CreatureTemplateType.FatFireFly)
            {
                if (ModManager.MSC && self.safariOwner)
                    return testCrit == self.targetCreature ? 1f : 0f;
                res = .25f;
                if (testCrit.state.dead)
                    res /= 10f;
                res *= testCrit.Room.AttractionValueForCreature(self.parent);
                res *= Mathf.Lerp(.5f, 1.5f, self.world.game.SeededRandom(self.parent.ID.RandomSeed + testCrit.ID.RandomSeed));
            }
            else if (tpl == CreatureTemplateType.FlyingBigEel)
            {
                if (ModManager.MSC && self.safariOwner)
                    return testCrit == self.targetCreature ? 1f : 0f;
                res = .55f;
                if (testCrit.state.dead)
                    res /= 10f;
                res *= testCrit.Room.AttractionValueForCreature(self.parent);
                res *= Mathf.Lerp(.5f, 1.5f, self.world.game.SeededRandom(self.parent.ID.RandomSeed + testCrit.ID.RandomSeed));
            }
        }
        return res;
    }

    internal static void IL_OverseerAI_FlyingWeapon(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, Weapon weapon) => flag || weapon.thrownBy.Template.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerAI.FlyingWeapon!");
    }

    internal static void IL_OverseerAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[s_loc1])
             .EmitDelegate((bool flag, AbstractCreature abstractCreature) => flag || abstractCreature.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerAI.Update!");
    }

    internal static void IL_CreaturePointer_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_CreatureTemplate_Type_PoleMimic,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, OverseerHologram.CreaturePointer self) => flag && self.pointAtCreature is not Denture);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerHologram.CreaturePointer.Update!");
    }

    internal static void IL_OverseerCommunicationModule_CreatureDangerScore(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchCallOrCallvirt_Room_ViewedByAnyCamera_Vector2_float))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, AbstractCreature creature) => flag && (creature.realizedCreature is not Denture dt || dt.JawOpen <= .5f));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerCommunicationModule.CreatureDangerScore! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdsfld_CreatureTemplate_Type_Scavenger,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, AbstractCreature creature) => flag || creature.creatureTemplate.type == CreatureTemplateType.ScavengerSentinel);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerCommunicationModule.CreatureDangerScore! (part 2)");
    }

    internal static float On_OverseerCommunicationModule_FoodDelicousScore(On.OverseerCommunicationModule.orig_FoodDelicousScore orig, OverseerCommunicationModule self, AbstractPhysicalObject foodObject, Player player)
    {
        if (foodObject?.realizedObject is PhysicalObject obj && (obj is ThornyStrawberry or BouncingMelon or LittleBalloon or Physalis or MarineEye or StarLemon or MiniFruit or DarkGrub) && !foodObject.slatedForDeletion && foodObject.Room == player.abstractCreature.Room)
        {
            var num = Mathf.InverseLerp(1100f, 400f, Vector2.Distance(obj.firstChunk.pos, player.DangerPos));
            if (num == 0f)
                return 0f;
            if (self.GuideState.itemTypes.Contains(foodObject.type))
            {
                if (num <= .2f || !self.room.ViewedByAnyCamera(obj.firstChunk.pos, 0f))
                    return 0f;
                num = .3f;
            }
            var objs = self.objectsAlreadyTalkedAbout;
            for (var i = 0; i < objs.Count; i++)
            {
                if (objs[i] == foodObject.ID)
                    return 0f;
            }
            if (foodObject == self.mostDeliciousFoodInRoom && self.currentConcern == OverseerCommunicationModule.PlayerConcern.FoodItemInRoom)
                num *= 1.1f;
            return num * Mathf.Lerp(self.GeneralPlayerFoodNeed(player), .6f, .5f);
        }
        else
            return orig(self, foodObject, player);
    }

    internal static void IL_OverseerCommunicationModule_FoodDelicousScore(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_1,
            s_MatchLdfld_AbstractPhysicalObject_type,
            s_MatchLdsfld_AbstractPhysicalObject_AbstractObjectType_JellyFish,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((bool flag, AbstractPhysicalObject foodObject) => flag && foodObject.type != AbstractObjectType.GummyAnther);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook OverseerCommunicationModule.FoodDelicousScore!");
    }
}