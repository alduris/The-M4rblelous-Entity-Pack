global using static LBMergedMods.Hooks.MirosHooks;
using static System.Reflection.BindingFlags;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil;
using System;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class MirosHooks
{
    internal static void On_BirdLeg_RunMode(On.MirosBird.BirdLeg.orig_RunMode orig, MirosBird.BirdLeg self)
    {
        orig(self);
        if (self.bird is Blizzor)
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
            self.forwardPower = Mathf.Lerp(self.forwardPower, 0f, .5f);
            var bs = self.bodyChunks;
            for (var i = 0; i < bs.Length; i++)
            {
                var b = bs[i];
                b.vel.x = Mathf.Lerp(b.vel.x, 0f, .125f);
            }
        }
    }

    internal static void IL_MirosBirdAbstractAI_Raid(ILContext il)
    {
        var c = new ILCursor(il);
        var abstractRoom = typeof(World).GetMethod("GetAbstractRoom", Static | Public | Instance | NonPublic, Type.DefaultBinder, [typeof(int)], null);
        MethodReference? meth = null;
        int loc = 0, loc2 = 0, loc3 = 0, loc4 = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("parent"),
            x => x.MatchCallOrCallvirt<AbstractWorldEntity>("get_Room"),
            x => x.MatchLdfld<AbstractRoom>("creatures"),
            x => x.MatchLdloc(out loc),
            x => x.MatchCallOrCallvirt(out meth),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("MirosBird"),
            x => x.MatchCall(out _))
        && meth is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Callvirt, "get_Room")
             .Emit<AbstractRoom>(OpCodes.Ldfld, "creatures")
             .Emit(OpCodes.Ldloc, il.Body.Variables[loc])
             .Emit(OpCodes.Callvirt, meth)
             .Emit<AbstractCreature>(OpCodes.Ldfld, "creatureTemplate")
             .Emit<CreatureTemplate>(OpCodes.Ldfld, "type")
             .EmitDelegate((bool flag, CreatureTemplate.Type tp) => flag || tp == CreatureTemplateType.Blizzor);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MirosBirdAbstractAI.Raid (part 1)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("parent"),
            x => x.MatchLdfld<AbstractWorldEntity>("world"),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("parent"),
            x => x.MatchLdfld<AbstractWorldEntity>("world"),
            x => x.MatchCallOrCallvirt<World>("get_firstRoomIndex"),
            x => x.MatchLdloc(out loc2),
            x => x.MatchAdd(),
            x => x.MatchCallOrCallvirt(abstractRoom),
            x => x.MatchLdfld<AbstractRoom>("creatures"),
            x => x.MatchLdloc(out loc3),
            x => x.MatchCallOrCallvirt(out meth),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("MirosBird"),
            x => x.MatchCall(out _))
        && meth is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Ldfld, "world")
             .Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Ldfld, "world")
             .Emit<World>(OpCodes.Callvirt, "get_firstRoomIndex")
             .Emit(OpCodes.Ldloc, il.Body.Variables[loc2])
             .Emit(OpCodes.Add)
             .Emit(OpCodes.Callvirt, abstractRoom)
             .Emit<AbstractRoom>(OpCodes.Ldfld, "creatures")
             .Emit(OpCodes.Ldloc, il.Body.Variables[loc3])
             .Emit(OpCodes.Callvirt, meth)
             .Emit<AbstractCreature>(OpCodes.Ldfld, "creatureTemplate")
             .Emit<CreatureTemplate>(OpCodes.Ldfld, "type")
             .EmitDelegate((bool flag, CreatureTemplate.Type tp) => flag || tp == CreatureTemplateType.Blizzor);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook MirosBirdAbstractAI.Raid (part 2)!");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<AbstractCreatureAI>("parent"),
            x => x.MatchCallOrCallvirt<AbstractWorldEntity>("get_Room"),
            x => x.MatchLdfld<AbstractRoom>("creatures"),
            x => x.MatchLdloc(out loc4),
            x => x.MatchCallOrCallvirt(out meth),
            x => x.MatchLdfld<AbstractCreature>("creatureTemplate"),
            x => x.MatchLdfld<CreatureTemplate>("type"),
            x => x.MatchLdsfld<CreatureTemplate.Type>("MirosBird"),
            x => x.MatchCall(out _))
        && meth is not null)
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<AbstractCreatureAI>(OpCodes.Ldfld, "parent")
             .Emit<AbstractWorldEntity>(OpCodes.Callvirt, "get_Room")
             .Emit<AbstractRoom>(OpCodes.Ldfld, "creatures")
             .Emit(OpCodes.Ldloc, il.Body.Variables[loc4])
             .Emit(OpCodes.Callvirt, meth)
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