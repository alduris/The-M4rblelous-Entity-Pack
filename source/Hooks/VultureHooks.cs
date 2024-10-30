global using static LBMergedMods.Hooks.VultureHooks;
using MoreSlugcats;

namespace LBMergedMods.Hooks;

public static class VultureHooks
{
    internal static void On_Vulture_AirBrake(On.Vulture.orig_AirBrake orig, Vulture self, int frames)
    {
        if (self is not FatFireFly)
            orig(self, frames);
    }

    internal static bool On_VultureAI_DoIWantToBiteCreature(On.VultureAI.orig_DoIWantToBiteCreature orig, VultureAI self, AbstractCreature creature) => orig(self, creature) && creature.creatureTemplate.type != CreatureTemplateType.FatFireFly && creature.creatureTemplate.type != CreatureTemplateType.FlyingBigEel && creature.creatureTemplate.type != CreatureTemplateType.MiniFlyingBigEel;

    internal static CreatureTemplate.Relationship On_VultureAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var res = orig(self, dRelation);
        if (dRelation.trackerRep.representedCreature.creatureTemplate is CreatureTemplate c)
        {
            if (self.vulture is FatFireFly f && (c.bodySize > f.Template.bodySize / 2f || c.IsVulture || (ModManager.MSC && c.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)))
                res = new(CreatureTemplate.Relationship.Type.Ignores, 0f);
            else if (c.type == CreatureTemplateType.FatFireFly)
                res = new(CreatureTemplate.Relationship.Type.Ignores, 0f);
        }
        return res;
    }

    internal static void On_VultureThruster_Update(On.Vulture.VultureThruster.orig_Update orig, Vulture.VultureThruster self, bool eu)
    {
        if (self.vulture is not FatFireFly)
            orig(self, eu);
    }
}