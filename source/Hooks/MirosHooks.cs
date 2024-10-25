global using static LBMergedMods.Hooks.MirosHooks;
using Random = UnityEngine.Random;
using static System.Reflection.BindingFlags;
using UnityEngine;
using MonoMod.Cil;
using RWCustom;
using Mono.Cecil;
using System;
using Mono.Cecil.Cil;

namespace LBMergedMods.Hooks;

public static class MirosHooks
{
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

    internal static void On_BirdLeg_RunMode(On.MirosBird.BirdLeg.orig_RunMode orig, MirosBird.BirdLeg self)
    {
        orig(self);
        if (self.bird?.Template.type == CreatureTemplateType.Blizzor)
        {
            self.springPower = Mathf.Lerp(self.springPower, 0f, .5f);
            self.forwardPower = Mathf.Lerp(self.forwardPower, 0f, .5f);
        }
    }

    internal static void On_MirosBird_ctor(On.MirosBird.orig_ctor orig, MirosBird self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Template.type == CreatureTemplateType.Blizzor)
        {
            for (var i = 0; i < self.bodyChunks.Length; i++)
            {
                var chunk = self.bodyChunks[i];
                chunk.rad *= 1.4f;
                chunk.mass *= 1.1f;
            }
            var chunk4 = self.bodyChunks[4];
            chunk4.rad = 10f;
            chunk4.mass *= 1.05f;
            self.abstractCreature.HypothermiaImmune = true;
        }
    }

    internal static void On_MirosBird_Act(On.MirosBird.orig_Act orig, MirosBird self)
    {
        orig(self);
        if (self.Template.type == CreatureTemplateType.Blizzor)
        {
            self.forwardPower = Mathf.Lerp(self.forwardPower, 0f, .5f);
            var bs = self.bodyChunks;
            for (var i = 0; i < bs.Length; i++)
                bs[i].vel.x = Mathf.Lerp(bs[i].vel.x, 0f, .125f);
        }
    }

    internal static void On_MirosBirdAbstractAI_ctor(On.MirosBirdAbstractAI.orig_ctor orig, MirosBirdAbstractAI self, World world, AbstractCreature parent)
    {
        orig(self, world, parent);
        if (parent.creatureTemplate.type == CreatureTemplateType.Blizzor)
        {
            self.allowedNodes.Clear();
            for (var i = self.world.firstRoomIndex; i < self.world.firstRoomIndex + (self.world.NumberOfRooms - 1); i++)
            {
                var rm = self.world.GetAbstractRoom(i);
                var attr = rm.AttractionForCreature(self.parent.creatureTemplate.type);
                if (attr == AbstractRoom.CreatureRoomAttraction.Like || attr == AbstractRoom.CreatureRoomAttraction.Stay || Array.IndexOf(MirosBirdAbstractAI.UGLYHARDCODEDALLOWEDROOMS, rm.name) >= 0)
                {
                    var nodes = rm.nodes;
                    for (var k = 0; k < nodes.Length; k++)
                    {
                        if (nodes[k].type == AbstractRoomNode.Type.SideExit && nodes[k].entranceWidth >= 5)
                        {
                            var nd = new WorldCoordinate(rm.index, -1, -1, k);
                            if (!self.allowedNodes.Contains(nd))
                                self.allowedNodes.Add(nd);
                        }
                    }
                }
            }
        }
    }

    internal static bool On_MirosBirdAI_DoIWantToBiteCreature(On.MirosBirdAI.orig_DoIWantToBiteCreature orig, MirosBirdAI self, AbstractCreature creature) => orig(self, creature) && creature.creatureTemplate.type != CreatureTemplateType.FatFireFly && creature.creatureTemplate.type != CreatureTemplateType.Blizzor;

    internal static void On_MirosBirdGraphics_ctor(On.MirosBirdGraphics.orig_ctor orig, MirosBirdGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.bird.Template.type == CreatureTemplateType.Blizzor)
        {
            var state = Random.state;
            Random.InitState(self.bird.abstractCreature.ID.RandomSeed);
            self.eyeCol = Custom.HSL2RGB(Mathf.Lerp(.65f, .69f, Random.value), .9f, .5f);
            self.eyeSize *= 1.2f;
            if (self.eyeSize < 1f)
                self.eyeSize = 1f;
            self.tighSize *= 1.2f;
            self.plumageLength *= 1.1f;
            self.plumageDensity *= 2f;
            self.plumageWidth *= 1.2f;
            self.neckFatness *= 2f;
            self.beakFatness *= 1.1f;
            Random.state = state;
        }
    }

    internal static void On_MirosBirdGraphics_ApplyPalette(On.MirosBirdGraphics.orig_ApplyPalette orig, MirosBirdGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        orig(self, sLeaser, rCam, palette);
        if (self.bird?.Template.type == CreatureTemplateType.Blizzor)
        {
            var clr = Color.Lerp(Color.white, palette.fogColor, Mathf.Lerp(palette.fogAmount, 0f, .75f) + .1f);
            for (var i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (i != self.EyeTrailSprite && (i < self.FirstBeakSprite || i > self.LastBeakSprite) && (i < self.FirstLegSprite || i > self.LastLegSprite))
                    sLeaser.sprites[i].color = clr;
            }
            for (var i = 0; i < self.legs.Length; i++)
                sLeaser.sprites[self.legs[i].firstSprite].color = clr;
        }
    }

    internal static void On_MirosBirdGraphics_InitiateSprites(On.MirosBirdGraphics.orig_InitiateSprites orig, MirosBirdGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);
        if (self.bird?.Template.type == CreatureTemplateType.Blizzor)
        {
            sLeaser.sprites[self.HeadSprite].element = Futile.atlasManager.GetElementWithName("BlizzorHead");
            sLeaser.sprites[self.NeckSprite].element = Futile.atlasManager.GetElementWithName("BlizzorNeck");
            sLeaser.sprites[self.BodySprite].element = Futile.atlasManager.GetElementWithName("BlizzorBody");
            var legs = self.legs;
            for (var i = 0; i < legs.Length; i++)
                sLeaser.sprites[legs[i].firstSprite].element = Futile.atlasManager.GetElementWithName("BlizzorTigh");
        }
    }
}