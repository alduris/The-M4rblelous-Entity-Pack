global using static LBMergedMods.Hooks.CreatureHooks;
using System.Collections.Generic;
using RWCustom;
using System;
using MoreSlugcats;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class CreatureHooks
{
    public static Action<ArtificialIntelligence, bool, FoodItemRepresentation>? OnFoodItemSpotted;
    public static Action<ArtificialIntelligence, AbstractPhysicalObject, FoodItemRepresentation>? OnResultAction;

    internal static bool On_BigJellyFish_ValidGrabCreature(On.MoreSlugcats.BigJellyFish.orig_ValidGrabCreature orig, BigJellyFish self, AbstractCreature abs) => abs.creatureTemplate.type != CreatureTemplateType.MiniLeviathan && abs.creatureTemplate.type != CreatureTemplateType.FlyingBigEel && abs.creatureTemplate.type != CreatureTemplateType.MiniFlyingBigEel && abs.creatureTemplate.type != CreatureTemplateType.MiniBlackLeech && abs.creatureTemplate.type != CreatureTemplateType.Denture && orig(self, abs);

    internal static void On_Creature_Abstractize(On.Creature.orig_Abstractize orig, Creature self)
    {
        orig(self);
        if (self is BigSpider && SporeMemory.TryGetValue(self.abstractCreature, out var mem))
            mem.Clear();
    }

    internal static void IL_InspectorAI_IUseARelationshipTracker_UpdateDynamicRelationship(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_Creature_get_grasps,
            s_MatchLdloc_OutLoc2,
            s_MatchLdelemRef,
            s_MatchBrfalse_Any))
        {
            var vars = il.Body.Variables;
            VariableDefinition? curRelVar = null;
            for (var i = 0; i < vars.Count; i++)
            {
                var vari = vars[i];
                if (vari.VariableType.Name.Contains("Relationship"))
                {
                    curRelVar = vari;
                    break;
                }
            }
            if (curRelVar is null)
            {
                LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook InspectorAI.IUseARelationshipTracker.UpdateDynamicRelationship!");
                return;
            }
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .Emit(OpCodes.Ldloc, curRelVar)
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .EmitDelegate((InspectorAI self, RelationshipTracker.DynamicRelationship dRelation, CreatureTemplate.Relationship currentRelationship, Creature realizedCreature, int i) =>
             {
                 if (realizedCreature.grasps[i].grabbed is DendriticNeuron swarmer && swarmer.Bites < 5)
                 {
                     currentRelationship.type = CreatureTemplate.Relationship.Type.Eats;
                     currentRelationship.intensity = 1f;
                     self.preyTracker.AddPrey(dRelation.trackerRep);
                 }
                 return currentRelationship;
             });
            c.Emit(OpCodes.Stloc, curRelVar);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook InspectorAI.IUseARelationshipTracker.UpdateDynamicRelationship!");
    }

    internal static bool On_CreatureTemplate_get_IsVulture(Func<CreatureTemplate, bool> orig, CreatureTemplate self) => self.type == CreatureTemplateType.FatFireFly || orig(self);

    internal static bool On_PathFinder_CoordinateReachableAndGetbackable(On.PathFinder.orig_CoordinateReachableAndGetbackable orig, PathFinder self, WorldCoordinate coord)
    {
        var res = orig(self, coord);
        if (coord.TileDefined && self.creature.realizedCreature is BigEel be && be is FlyingBigEel or MiniFlyingBigEel && be.antiStrandingZones is List<PlacedObject> list && list.Count > 0 && be.room is Room rm)
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

    internal static float On_ThreatDetermination_ThreatOfCreature(On.ThreatDetermination.orig_ThreatOfCreature orig, ThreatDetermination self, Creature creature, Player player)
    {
        if ((creature is Sporantula spore && (spore.dead || (spore.AI is SporantulaAI sporeAI && !sporeAI.DoIWantToKill(player.abstractCreature)))) || (creature is Scutigera centi && (centi.dead || (centi.AI is ScutigeraAI centiAI && !centiAI.DoIWantToShockCreature(player.abstractCreature)))))
            return 0f;
        return orig(self, creature, player);
    }

    public static void FoodItemSpotted(this ArtificialIntelligence self, bool firstSpot, FoodItemRepresentation item) => OnFoodItemSpotted?.Invoke(self, firstSpot, item);
}