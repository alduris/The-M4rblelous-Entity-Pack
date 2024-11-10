global using static LBMergedMods.Hooks.CreatureHooks;
using System.Collections.Generic;
using RWCustom;
using System;
using MoreSlugcats;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class CreatureHooks
{
    public static Action<ArtificialIntelligence, bool, FoodItemRepresentation>? OnFoodItemSpotted;
    public static Action<ArtificialIntelligence, AbstractPhysicalObject, FoodItemRepresentation>? OnResultAction;

    internal static float On_ArtificialIntelligence_VisualScore(On.ArtificialIntelligence.orig_VisualScore orig, ArtificialIntelligence self, Vector2 lookAtPoint, float bonus)
    {
        if (self.creature?.realizedCreature is Lizard l && l.Template.type == CreatureTemplateType.MoleSalamander && l.room is Room rm /*&& rm.water*/ && rm.GetTile(lookAtPoint).DeepWater && rm.GetTile(l.VisionPoint).DeepWater && Custom.DistLess(l.VisionPoint, lookAtPoint, 8000f * bonus))
            return 1f;
        return orig(self, lookAtPoint, bonus);
    }

    internal static bool On_BigJellyFish_ValidGrabCreature(On.MoreSlugcats.BigJellyFish.orig_ValidGrabCreature orig, BigJellyFish self, AbstractCreature abs) => abs.creatureTemplate.type != CreatureTemplateType.MiniLeviathan && abs.creatureTemplate.type != CreatureTemplateType.FlyingBigEel && abs.creatureTemplate.type != CreatureTemplateType.MiniFlyingBigEel && abs.creatureTemplate.type != CreatureTemplateType.MiniBlackLeech && abs.creatureTemplate.type != CreatureTemplateType.Denture && orig(self, abs);

    internal static void On_Creature_Abstractize(On.Creature.orig_Abstractize orig, Creature self)
    {
        orig(self);
        if (self is BigSpider && SporeMemory.TryGetValue(self.abstractCreature, out var mem))
            mem.Clear();
    }

    internal static bool On_CreatureTemplate_get_IsVulture(Func<CreatureTemplate, bool> orig, CreatureTemplate self) => self.type == CreatureTemplateType.FatFireFly || orig(self);

    internal static bool On_PathFinder_CoordinateReachableAndGetbackable(On.PathFinder.orig_CoordinateReachableAndGetbackable orig, PathFinder self, WorldCoordinate coord)
    {
        var res = orig(self, coord);
        if (coord.TileDefined && self.creature.realizedCreature is BigEel be && (be.Template.type == CreatureTemplateType.FlyingBigEel || be.Template.type == CreatureTemplateType.MiniFlyingBigEel) && be.antiStrandingZones is List<PlacedObject> list && list.Count > 0 && be.room is Room rm)
        {
            for (var j = 0; j < list.Count; j++)
            {
                if (Custom.DistLess(rm.MiddleOfTile(coord), list[j].pos, 100f))
                {
                    res = false;
                    break;
                }
            }
        }
        return res;
    }

    internal static bool On_SporePlant_SporePlantInterested(On.SporePlant.orig_SporePlantInterested orig, CreatureTemplate.Type tp) => tp != CreatureTemplateType.Denture && orig(tp);

    internal static bool On_StowawayBugAI_WantToEat(On.MoreSlugcats.StowawayBugAI.orig_WantToEat orig, StowawayBugAI self, CreatureTemplate.Type input) => input != CreatureTemplateType.Denture && input != CreatureTemplateType.MiniLeviathan && input != CreatureTemplateType.FatFireFly && input != CreatureTemplateType.FlyingBigEel && input != CreatureTemplateType.MiniFlyingBigEel && input != CreatureTemplateType.Blizzor && orig(self, input);

    public static FoodItemRepresentation CreateTrackerRepresentationForItem(this ArtificialIntelligence self, AbstractPhysicalObject item)
    {
        var res = new FoodItemRepresentation(self.tracker, item, 0f, true);
        OnResultAction?.Invoke(self, item, res);
        return res;
    }

    public static void FoodItemSpotted(this ArtificialIntelligence self, bool firstSpot, FoodItemRepresentation item) => OnFoodItemSpotted?.Invoke(self, firstSpot, item);
}