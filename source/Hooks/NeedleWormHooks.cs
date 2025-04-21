global using static LBMergedMods.Hooks.NeedleWormHooks;

namespace LBMergedMods.Hooks;
//CHK
public static class NeedleWormHooks
{
    internal static CreatureTemplate.Relationship On_BigNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.BigNeedleWormAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigNeedleWormAI self, RelationshipTracker.DynamicRelationship dRelation)
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

    internal static CreatureTemplate.Relationship On_SmallNeedleWormAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.SmallNeedleWormAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, SmallNeedleWormAI self, RelationshipTracker.DynamicRelationship dRelation)
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
}