global using static LBMergedMods.Hooks.DropBugHooks;

namespace LBMergedMods.Hooks;
//CHK
public static class DropBugHooks
{
    internal static CreatureTemplate.Relationship On_DropBugAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.DropBugAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DropBugAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (dRelation.trackerRep?.representedCreature?.realizedCreature is Creature c && c.abstractPhysicalObject.SameRippleLayer(self.creature))
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

    internal static bool On_DropBugAI_IUseItemTracker_TrackItem(On.DropBugAI.orig_IUseItemTracker_TrackItem orig, DropBugAI self, AbstractPhysicalObject obj)
    {
        var res = orig(self, obj);
        if (self.bug.safariControlled || self.creature.world.game.SeededRandom(obj.ID.RandomSeed + 5) <= self.creature.personality.dominance)
        {
            if (obj.type == AbstractObjectType.Physalis || obj.type == AbstractObjectType.BouncingMelon || obj.type == AbstractObjectType.BlobPiece || obj.type == AbstractObjectType.MarineEye || obj.type == AbstractObjectType.SporeProjectile || obj.type == AbstractObjectType.ThornyStrawberry || obj.type == AbstractObjectType.LittleBalloon || obj.type == AbstractObjectType.MiniBlueFruit || obj.type == AbstractObjectType.FumeFruit || obj.type == AbstractObjectType.Durian)
                res = true;
        }
        return res;
    }
}