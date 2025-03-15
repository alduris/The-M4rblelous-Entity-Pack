global using static LBMergedMods.Hooks.MirosHooks;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class MirosHooks
{
    internal static void On_BirdLeg_RunMode(On.MirosBird.BirdLeg.orig_RunMode orig, MirosBird.BirdLeg self)
    {
        orig(self);
        if (self.bird is Blizzor b && (b.AI is not MirosBirdAI ai || ai.behavior != MirosBirdAI.Behavior.Hunt || ai.focusCreature is not Tracker.CreatureRepresentation rep || ai.DynamicRelationship(rep).type != CreatureTemplate.Relationship.Type.Eats || !rep.VisualContact))
        {
            self.springPower = Mathf.Lerp(self.springPower, 0f, .5f);
            self.forwardPower = Mathf.Lerp(self.forwardPower, 0f, .5f);
        }
    }

    internal static void On_MirosBird_Act(On.MirosBird.orig_Act orig, MirosBird self)
    {
        orig(self);
        if (self is Blizzor)
        {
            var bs = self.bodyChunks;
            if (self.AI is not MirosBirdAI ai || ai.behavior != MirosBirdAI.Behavior.Hunt || ai.focusCreature is not Tracker.CreatureRepresentation rep || ai.DynamicRelationship(rep).type != CreatureTemplate.Relationship.Type.Eats || !rep.VisualContact)
            {
                self.forwardPower = Mathf.Lerp(self.forwardPower, 0f, .5f);
                for (var i = 0; i < bs.Length; i++)
                {
                    var b = bs[i];
                    b.vel.x = Mathf.Lerp(b.vel.x, 0f, .125f);
                }
            }
            else
            {
                if (self.forwardPower < .5f)
                    self.forwardPower = .5f;
                self.forwardPower = Mathf.Lerp(self.forwardPower, 0f, .5f);
                for (var i = 0; i < bs.Length; i++)
                {
                    var b = bs[i];
                    if (b.vel.x < 10f)
                        b.vel.x += .001f;
                }
            }
        }
    }

    internal static void IL_MirosBirdAbstractAI_Raid(ILContext il)
    {
        var c = new ILCursor(il);
        var vars = il.Body.Variables;
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_parent,
            s_MatchCallOrCallvirt_AbstractWorldEntity_get_Room,
            s_MatchLdfld_AbstractRoom_creatures,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_OutRef,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_MirosBird,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Callvirt, "get_Room")
             .Emit<AbstractRoom>(OpCodes.Ldfld, "creatures")
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Callvirt, s_ref)
             .Emit<AbstractCreature>(OpCodes.Ldfld, "creatureTemplate")
             .Emit<CreatureTemplate>(OpCodes.Ldfld, "type")
             .EmitDelegate((bool flag, CreatureTemplate.Type tp) => flag || tp == CreatureTemplateType.Blizzor);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MirosBirdAbstractAI.Raid (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_parent,
            s_MatchLdfld_AbstractWorldEntity_world,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_parent,
            s_MatchLdfld_AbstractWorldEntity_world,
            s_MatchCallOrCallvirt_World_get_firstRoomIndex,
            s_MatchLdloc_OutLoc1,
            s_MatchAdd,
            s_MatchCallOrCallvirt_World_GetAbstractRoom_int,
            s_MatchLdfld_AbstractRoom_creatures,
            s_MatchLdloc_OutLoc2,
            s_MatchCallOrCallvirt_OutRef,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_MirosBird,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Ldfld, "world")
             .Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Ldfld, "world")
             .Emit<World>(OpCodes.Callvirt, "get_firstRoomIndex")
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Add)
             .Emit(OpCodes.Callvirt, il.Import(s_World_GetAbstractRoom_int))
             .Emit<AbstractRoom>(OpCodes.Ldfld, "creatures")
             .Emit(OpCodes.Ldloc, vars[s_loc2])
             .Emit(OpCodes.Callvirt, s_ref)
             .Emit<AbstractCreature>(OpCodes.Ldfld, "creatureTemplate")
             .Emit<CreatureTemplate>(OpCodes.Ldfld, "type")
             .EmitDelegate((bool flag, CreatureTemplate.Type tp) => flag || tp == CreatureTemplateType.Blizzor);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MirosBirdAbstractAI.Raid (part 2)!");
        if (c.TryGotoNext(MoveType.After,
            s_MatchLdarg_0,
            s_MatchLdfld_AbstractCreatureAI_parent,
            s_MatchCallOrCallvirt_AbstractWorldEntity_get_Room,
            s_MatchLdfld_AbstractRoom_creatures,
            s_MatchLdloc_OutLoc1,
            s_MatchCallOrCallvirt_OutRef,
            s_MatchLdfld_AbstractCreature_creatureTemplate,
            s_MatchLdfld_CreatureTemplate_type,
            s_MatchLdsfld_CreatureTemplate_Type_MirosBird,
            s_MatchCall_Any))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Callvirt, "get_Room")
             .Emit<AbstractRoom>(OpCodes.Ldfld, "creatures")
             .Emit(OpCodes.Ldloc, vars[s_loc1])
             .Emit(OpCodes.Callvirt, s_ref)
             .Emit<AbstractCreature>(OpCodes.Ldfld, "creatureTemplate")
             .Emit<CreatureTemplate>(OpCodes.Ldfld, "type")
             .EmitDelegate((bool flag, CreatureTemplate.Type tp) => flag || tp == CreatureTemplateType.Blizzor);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MirosBirdAbstractAI.Raid (part 3)!");
    }

    internal static bool On_MirosBirdAI_DoIWantToBiteCreature(On.MirosBirdAI.orig_DoIWantToBiteCreature orig, MirosBirdAI self, AbstractCreature creature)
    {
        var tp = creature.creatureTemplate.type;
        return tp != CreatureTemplateType.FatFireFly && tp != CreatureTemplateType.Blizzor && orig(self, creature);
    }
}