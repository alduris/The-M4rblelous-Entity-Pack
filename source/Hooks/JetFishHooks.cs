global using static LBMergedMods.Hooks.JetFishHooks;
using RWCustom;
using UnityEngine;

namespace LBMergedMods.Hooks;

public static class JetFishHooks
{
    internal static void On_JetFish_ctor(On.JetFish.orig_ctor orig, JetFish self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        self.albino = self.albino || (Albino.TryGetValue(abstractCreature, out var props) && props.Value);
        if (self.albino)
            self.iVars.eyeColor = Color.red;
    }

    internal static void On_JetFishAI_SocialEvent(On.JetFishAI.orig_SocialEvent orig, JetFishAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
    {
        if (involvedItem is BlobPiece or MarineEye)
        {
            if (self.tracker.RepresentationForObject(subjectCrit, false) is not Tracker.CreatureRepresentation repr)
                return;
            Tracker.CreatureRepresentation? repr2 = null;
            var flag = objectCrit == self.fish;
            if (!flag)
            {
                repr2 = self.tracker.RepresentationForObject(objectCrit, false);
                if (repr2 is null)
                    return;
            }
            if ((repr2 is null || repr.TicksSinceSeen <= 40 || repr2.TicksSinceSeen <= 40) && ID == SocialEventRecognizer.EventID.ItemOffering && flag)
                self.GiftRecieved(involvedItem.room.socialEventRecognizer.ItemOwnership(involvedItem));
        }
        else
            orig(self, ID, subjectCrit, objectCrit, involvedItem);
    }

    internal static bool On_JetFishAI_WantToEatObject(On.JetFishAI.orig_WantToEatObject orig, JetFishAI self, PhysicalObject obj)
    {
        var res = orig(self, obj);
        if (obj is BlobPiece or MarineEye && self.fish?.room is Room rmj && self.pathFinder is PathFinder p && obj.room == rmj && obj.grabbedBy?.Count == 0 && !obj.slatedForDeletetion)
        {
            var coord = rmj.GetWorldCoordinate(obj.firstChunk.pos);
            if (p.CoordinateReachableAndGetbackable(coord) || p.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -1)) || p.CoordinateReachableAndGetbackable(coord + new IntVector2(0, -2)) && self.threatTracker is ThreatTracker t && t.ThreatOfArea(coord, true) < .55f)
                res = true;
        }
        return res;
    }
}