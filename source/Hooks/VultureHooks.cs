global using static LBMergedMods.Hooks.VultureHooks;

namespace LBMergedMods.Hooks;
//CHK
public static class VultureHooks
{
    internal static void On_Vulture_AirBrake(On.Vulture.orig_AirBrake orig, Vulture self, int frames)
    {
        if (self is not FatFireFly)
            orig(self, frames);
    }

    internal static bool On_VultureAI_DoIWantToBiteCreature(On.VultureAI.orig_DoIWantToBiteCreature orig, VultureAI self, AbstractCreature creature)
    {
        var tp = creature.creatureTemplate.type;
        var flag = true;
        if (self.creature.creatureTemplate.type == CreatureTemplateType.FatFireFly)
            flag = !creature.creatureTemplate.IsVulture && (!ModManager.DLCShared || tp != DLCSharedEnums.CreatureTemplateType.MirosVulture);
        return flag && tp != CreatureTemplateType.FatFireFly && tp != CreatureTemplateType.FlyingBigEel && tp != CreatureTemplateType.MiniFlyingBigEel && orig(self, creature);
    }

    internal static CreatureTemplate.Relationship On_VultureAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.VultureAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, VultureAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var repr = dRelation.trackerRep.representedCreature;
        var reprTempl = repr.creatureTemplate;
        var tpl = self.creature.creatureTemplate;
        if (tpl.type?.value == "Joust" && reprTempl.type == CreatureTemplateType.XyloWorm)
            return new(CreatureTemplate.Relationship.Type.Eats, 1f);
        if (reprTempl.type == CreatureTemplateType.FatFireFly)
            return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
        if (tpl.type == CreatureTemplateType.FatFireFly)
        {
            if (reprTempl.bodySize > tpl.bodySize / 2f || reprTempl.IsVulture || (ModManager.DLCShared && reprTempl.type == DLCSharedEnums.CreatureTemplateType.MirosVulture))
                return new(CreatureTemplate.Relationship.Type.Ignores, 0f);
            var staticRel = self.StaticRelationship(repr);
            staticRel.intensity *= .1f;
            return staticRel;
        }
        return orig(self, dRelation);
    }

    internal static void On_VultureThruster_Update(On.Vulture.VultureThruster.orig_Update orig, Vulture.VultureThruster self, bool eu)
    {
        if (self.vulture is not FatFireFly)
            orig(self, eu);
    }
}