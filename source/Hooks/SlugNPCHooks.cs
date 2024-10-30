global using static LBMergedMods.Hooks.SlugNPCHooks;
using MoreSlugcats;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class SlugNPCHooks
{
    internal static void On_SlugNPCAI_AteFood(On.MoreSlugcats.SlugNPCAI.orig_AteFood orig, SlugNPCAI self, PhysicalObject food)
    {
        if (food is ThornyStrawberry or GummyAnther)
        {
            var num = self.foodPreference[SlugNPCAI.Food.DangleFruit.index];
            if (Mathf.Abs(num) > .4f)
                self.foodReaction += (int)(num * 120f);
            if (Mathf.Abs(num) > .85f && self.FunStuff)
                self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(.85f, 1f, Mathf.Abs(num))));
        }
        else if (food is BlobPiece or MarineEye)
        {
            var num = self.foodPreference[SlugNPCAI.Food.WaterNut.index];
            if (Mathf.Abs(num) > .4f)
                self.foodReaction += (int)(num * 120f);
            if (Mathf.Abs(num) > .85f && self.FunStuff)
                self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(.85f, 1f, Mathf.Abs(num))));
        }
        else if (food is LittleBalloon)
        {
            var num = self.foodPreference[SlugNPCAI.Food.Popcorn.index];
            if (Mathf.Abs(num) > .4f)
                self.foodReaction += (int)(num * 120f);
            if (Mathf.Abs(num) > .85f && self.FunStuff)
                self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(.85f, 1f, Mathf.Abs(num))));
        }
        else if (food is Physalis)
        {
            var num = self.foodPreference[SlugNPCAI.Food.SlimeMold.index];
            if (Mathf.Abs(num) > .4f)
                self.foodReaction += (int)(num * 120f);
            if (Mathf.Abs(num) > .85f && self.FunStuff)
                self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(.85f, 1f, Mathf.Abs(num))));
        }
        else if (food is StarLemon)
        {
            var num = self.foodPreference[SlugNPCAI.Food.GlowWeed.index];
            if (Mathf.Abs(num) > .4f)
                self.foodReaction += (int)(num * 120f);
            if (Mathf.Abs(num) > .85f && self.FunStuff)
                self.cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(.85f, 1f, Mathf.Abs(num))));
        }
        else
            orig(self, food);
    }

    internal static SlugNPCAI.Food On_SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
    {
        var res = orig(self, food);
        if (food is ThornyStrawberry)
            res = SlugFood.ThornyStrawberry!;
        else if (food is BlobPiece)
            res = SlugFood.BlobPiece!;
        else if (food is LittleBalloon)
            res = SlugFood.LittleBalloon!;
        else if (food is Physalis)
            res = SlugFood.Physalis!;
        else if (food is GummyAnther)
            res = SlugFood.GummyAnther!;
        else if (food is MarineEye)
            res = SlugFood.MarineEye!;
        else if (food is StarLemon)
            res = SlugFood.StarLemon!;
        return res;
    }

    internal static bool On_SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject obj) => (obj is BouncingMelon && !self.IsFull) || orig(self, obj);
}