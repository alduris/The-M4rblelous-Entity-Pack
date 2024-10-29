global using static LBMergedMods.Hooks.CreatureHooks;
using MonoMod.Cil;
using Mono.Cecil.Cil;
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

    internal static bool On_BigJellyFish_ValidGrabCreature(On.MoreSlugcats.BigJellyFish.orig_ValidGrabCreature orig, BigJellyFish self, AbstractCreature abs) => abs.creatureTemplate.type != CreatureTemplateType.MiniLeviathan && abs.creatureTemplate.type != CreatureTemplateType.FlyingBigEel && abs.creatureTemplate.type != CreatureTemplateType.MiniFlyingBigEel && abs.creatureTemplate.type != CreatureTemplateType.MiniBlackLeech && orig(self, abs);

    internal static CreatureTemplate.Relationship On_DropBugAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.DropBugAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DropBugAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c)
        {
            var grs = c.grasps;
            if (grs is not null)
            {
                for (var i = 0; i < grs.Length; i++)
                {
                    if (grs[i]?.grabbed is LimeMushroom)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Afraid;
                        result.intensity = 1f;
                        break;
                    }
                }
            }
        }
        return result;
    }

    internal static CreatureTemplate.Relationship On_SmallNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.SmallNeedleWormAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, SmallNeedleWormAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c)
        {
            var grs = c.grasps;
            if (grs is not null)
            {
                for (var i = 0; i < grs.Length; i++)
                {
                    if (grs[i]?.grabbed is LimeMushroom)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Afraid;
                        result.intensity = 1f;
                        break;
                    }
                }
            }
        }
        return result;
    }

    internal static CreatureTemplate.Relationship On_BigNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.BigNeedleWormAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigNeedleWormAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c)
        {
            var grs = c.grasps;
            if (grs is not null)
            {
                for (var i = 0; i < grs.Length; i++)
                {
                    if (grs[i]?.grabbed is LimeMushroom)
                    {
                        result.type = CreatureTemplate.Relationship.Type.Afraid;
                        result.intensity = 1f;
                        break;
                    }
                }
            }
        }
        return result;
    }

    internal static void On_Creature_Abstractize(On.Creature.orig_Abstractize orig, Creature self)
    {
        orig(self);
        if (self is BigSpider && SporeMemory.TryGetValue(self.abstractCreature, out var mem))
            mem.Clear();
    }

    internal static void On_Creature_Blind(On.Creature.orig_Blind orig, Creature self, int blnd)
    {
        orig(self, blnd);
        if (self.Template.type == CreatureTemplateType.Blizzor)
            self.blind = 0;
    }

    internal static bool On_CreatureTemplate_get_IsVulture(Func<CreatureTemplate, bool> orig, CreatureTemplate self) => orig(self) || self.type == CreatureTemplateType.FatFireFly;

    internal static Tracker.CreatureRepresentation? On_GarbageWormAI_CreateTrackerRepresentationForCreature(On.GarbageWormAI.orig_CreateTrackerRepresentationForCreature orig, GarbageWormAI self, AbstractCreature otherCreature)
    {
        if (otherCreature.creatureTemplate.type == CreatureTemplateType.MiniBlackLeech)
            return null;
        return orig(self, otherCreature);
    }

    internal static void IL_GarbageWormAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        int loc1 = 0, loc2 = 0;
        if (c.TryGotoNext(
            x => x.MatchLdloc(out loc1),
            x => x.MatchLdfld<GarbageWormAI.CreatureInterest>("crit"),
            x => x.MatchLdfld<Tracker.CreatureRepresentation>("representedCreature"),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchCallOrCallvirt<CreatureTemplate>("get_IsVulture"),
            x => x.MatchBrfalse(out _),
            x => x.MatchLdcR4(1000f),
            x => x.MatchStloc(out loc2)))
        {
            var l2 = il.Body.Variables[loc2];
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc1])
             .Emit(OpCodes.Ldloc, l2)
             .EmitDelegate((GarbageWormAI.CreatureInterest interest, float num) =>
             {
                 if (interest.crit.representedCreature.creatureTemplate.type == CreatureTemplateType.FlyingBigEel)
                     return 1000f;
                 return num;
             });
            c.Emit(OpCodes.Stloc, l2);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook GarbageWormAI.Update!");
    }

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

    internal static bool On_StowawayBugAI_WantToEat(On.MoreSlugcats.StowawayBugAI.orig_WantToEat orig, StowawayBugAI self, CreatureTemplate.Type input) => input != CreatureTemplateType.MiniLeviathan && input != CreatureTemplateType.FatFireFly && input != CreatureTemplateType.FlyingBigEel && input != CreatureTemplateType.MiniFlyingBigEel && input != CreatureTemplateType.Blizzor && orig(self, input);

    public static FoodItemRepresentation CreateTrackerRepresentationForItem(this ArtificialIntelligence self, AbstractPhysicalObject item)
    {
        var res = new FoodItemRepresentation(self.tracker, item, 0f, true);
        OnResultAction?.Invoke(self, item, res);
        return res;
    }

    public static void FoodItemSpotted(this ArtificialIntelligence self, bool firstSpot, FoodItemRepresentation item) => OnFoodItemSpotted?.Invoke(self, firstSpot, item);
}