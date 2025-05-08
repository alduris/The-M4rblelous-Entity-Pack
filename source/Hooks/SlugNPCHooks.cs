global using static LBMergedMods.Hooks.SlugNPCHooks;
using MonoMod.Cil;
using MoreSlugcats;
using UnityEngine;
using Mono.Cecil.Cil;
using System;

namespace LBMergedMods.Hooks;
//CHK
public static class SlugNPCHooks
{
    internal static void On_SlugNPCAI_AteFood(On.MoreSlugcats.SlugNPCAI.orig_AteFood orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is ThornyStrawberry or GummyAnther or MiniFruit)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.DangleFruit.index]);
        else if (food is BlobPiece or MarineEye)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.WaterNut.index]);
        else if (food is LittleBalloon)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.Popcorn.index]);
        else if (food is Physalis)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.SlimeMold.index]);
        else if (food is StarLemon)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.GlowWeed.index]);
        else if (food is DendriticNeuron)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.Neuron.index]);
        else if (food is MiniScutigera)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.SmallCentipede.index]);
        else if (food is XyloWorm)
            self.DefaultFoodReaction(self.foodPreference[SlugNPCAI.Food.VultureGrub.index]);
        else
            orig(self, food);
    }

    internal static SlugNPCAI.Food On_SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is ThornyStrawberry)
            return SlugFood.ThornyStrawberry!;
        if (food is BlobPiece)
            return SlugFood.BlobPiece!;
        if (food is LittleBalloon)
            return SlugFood.LittleBalloon!;
        if (food is Physalis)
            return SlugFood.Physalis!;
        if (food is GummyAnther)
            return SlugFood.GummyAnther!;
        if (food is MarineEye)
            return SlugFood.MarineEye!;
        if (food is StarLemon)
            return SlugFood.StarLemon!;
        if (food is DendriticNeuron)
            return SlugFood.DendriticNeuron!;
        if (food is MiniFruit)
            return SlugFood.MiniBlueFruit!;
        if (food is MiniScutigera)
            return SlugFood.MiniScutigera!;
        if (food is XyloWorm)
            return SlugFood.XyloWorm!;
        return orig(self, food);
    }

    internal static float On_SlugNPCAI_LethalWeaponScore(On.MoreSlugcats.SlugNPCAI.orig_LethalWeaponScore orig, SlugNPCAI self, PhysicalObject obj, Creature target)
    {
        if (obj is PuffBall or FlareBomb && target is Sporantula)
            return 0f;
        if (obj is SmallPuffBall)
            return target is InsectoidCreature ? (target is Sporantula ? 0f : 5.3f) : .3f;
        if (obj is ThornyStrawberry st)
            return st.SpikesRemoved() ? 0f : .75f;
        if (obj is LittleBalloon)
            return self.WantsToEatThis(obj) ? 0f : .35f;
        return orig(self, obj, target);
    }

    internal static void IL_SlugNPCAI_PassingGrab(ILContext il)
    {
        var c = new ILCursor(il);
        for (var i = 1; i <= 2; i++)
        {
            if (c.TryGotoNext(MoveType.After,
                s_MatchLdloc_OutLoc1,
                s_MatchIsinst_PuffBall,
                s_MatchBrtrue_OutLabel))
            {
                c.Emit(OpCodes.Ldloc, s_loc1)
                 .EmitDelegate((PhysicalObject realizedObject) => realizedObject is SmallPuffBall || realizedObject is LittleBalloon || (realizedObject is ThornyStrawberry st && !st.SpikesRemoved()));
                c.Emit(OpCodes.Brtrue, s_label);
            }
            else
                LBMergedModsPlugin.s_logger.LogError($"Couldn't ILHook SlugNPCAI.PassingGrab (part {i})!");
        }
    }

    internal static bool On_SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject obj) => (obj is BouncingMelon && !self.IsFull) || orig(self, obj);

    public static void DefaultFoodReaction(this SlugNPCAI self, float preference)
    {
        var abs = Math.Abs(preference);
        if (abs > .4f)
            self.foodReaction += (int)(preference * 120f);
        if (abs > .85f && self.FunStuff)
            self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(.85f, 1f, abs)));
    }
}