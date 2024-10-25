global using static LBMergedMods.Hooks.CentipedeHooks;
using MonoMod.Cil;
using System;
using UnityEngine;
using Mono.Cecil.Cil;
using RWCustom;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class CentipedeHooks
{
    internal static bool On_Centipede_get_Centiwing(Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.RedHorrorCenti || orig(self);

    internal static bool On_Centipede_get_Red(Func<Centipede, bool> orig, Centipede self) => self.Template.type == CreatureTemplateType.RedHorrorCenti || orig(self);

    internal static void IL_Centipede_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<AbstractCreature>("state"),
            x => x.MatchLdcR4(2.3f),
            x => x.MatchLdcR4(7f),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<Centipede>("size"),
            x => x.MatchCall<Mathf>("Lerp"),
            x => x.MatchCall<Mathf>("RoundToInt"),
            x => x.MatchStfld<CreatureState>("meatLeft")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((Centipede self, AbstractCreature abstractCreature) =>
             {
                 if (self.Scutigera())
                     abstractCreature.state.meatLeft = 5;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.ctor!");
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && !self.RedHorror());
            }
        }
    }

    internal static void On_Centipede_ctor(On.Centipede.orig_ctor orig, Centipede self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        if (self.Scutigera())
        {
            var chs = self.bodyChunks;
            var le = self.bodyChunks.Length;
            var sz = self.size;
            for (var i = 0; i < chs.Length; i++)
            {
                var num = (float)i / (le - 1);
                var num2 = Mathf.Lerp(Mathf.Lerp(2f, 3.5f, sz), Mathf.Lerp(4f, 6.5f, sz), Mathf.Pow(Mathf.Clamp(Mathf.Sin(Mathf.PI * num), 0f, 1f), Mathf.Lerp(.7f, .3f, sz)));
                num2 = Mathf.Lerp(num2, Mathf.Lerp(2f, 3.5f, sz), .4f);
                chs[i].rad = num2;
            }
            var num3 = 0;
            for (var l = 0; l < chs.Length; l++)
            {
                for (var m = l + 1; m < chs.Length; m++)
                {
                    self.bodyChunkConnections[num3].distance = chs[l].rad + chs[m].rad;
                    num3++;
                }
            }
        }
        else if (self.RedHorror())
            self.flying = true;
    }

    internal static void IL_Centipede_Crawl(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchCallOrCallvirt<HealthState>("get_ClampedHealth"),
            x => x.MatchMul(),
            x => x.MatchCall<Mathf>("Lerp"),
            x => x.MatchCall<Vector2>("op_Multiply"),
            x => x.MatchLdarg(0),
            x => x.MatchCallOrCallvirt<Centipede>("get_Red")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, Centipede self) => flag || self.Scutigera());
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Crawl!");
    }

    internal static float On_Centipede_GenerateSize(On.Centipede.orig_GenerateSize orig, AbstractCreature abstrCrit) => abstrCrit.creatureTemplate.type == CreatureTemplateType.Scutigera || abstrCrit.creatureTemplate.type == CreatureTemplateType.RedHorrorCenti ? 1f : orig(abstrCrit);

    internal static void IL_Centipede_Shock(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdcR4(.7f),
            x => x.MatchLdcR4(.7f),
            x => x.MatchLdcR4(1f),
            x => x.MatchNewobj<Color>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Color color, Centipede self) => self.Scutigera() ? new(color.r, color.b, color.g) : color);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Shock! (part 1)");
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchStfld<CentipedeAI>("annoyingCollisions")))
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Shock! (from 1 to 2)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdcR4(.7f),
            x => x.MatchLdcR4(.7f),
            x => x.MatchLdcR4(1f),
            x => x.MatchNewobj<Color>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Color color, Centipede self) => self.Scutigera() ? new(color.r, color.b, color.g) : color);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Shock! (part 2)");
    }

    internal static void IL_Centipede_ShortCutColor(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && !self.RedHorror());
            }
        }
    }

    internal static Color On_Centipede_ShortCutColor(On.Centipede.orig_ShortCutColor orig, Centipede self) => self.Scutigera() ? Custom.HSL2RGB(Mathf.Lerp(.1527777777777778f, .1861111111111111f, .5f), Mathf.Lerp(.294f, .339f, .5f), .5f) : orig(self);

    internal static bool On_Centipede_SpearStick(On.Centipede.orig_SpearStick orig, Centipede self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appPos, Vector2 direction)
    {
        if (self.Scutigera() && self.CentiState is Centipede.CentipedeState s && Random.value < .25f && chunk is not null && chunk.index >= 0 && chunk.index < s.shells.Length && (chunk.index == self.shellJustFellOff || s.shells[chunk.index]))
        {
            if (chunk.index == self.shellJustFellOff)
                self.shellJustFellOff = -1;
            return false;
        }
        return orig(self, source, dmg, chunk, appPos, direction);
    }

    internal static void IL_Centipede_Stun(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && !self.RedHorror());
            }
        }
    }

    internal static void On_Centipede_Update(On.Centipede.orig_Update orig, Centipede self, bool eu)
    {
        orig(self, eu);
        if (self.RedHorror() && self.Consious)
        {
            var chs = self.bodyChunks;
            if (self.flying)
            {
                for (var i = 0; i < chs.Length; i++)
                    chs[i].vel *= 1.04f;
            }
            else
            {
                for (var i = 0; i < chs.Length; i++)
                    chs[i].vel *= 1.02f;
            }
        }
    }

    internal static void IL_Centipede_Violence(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchNewobj<CentipedeShell>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CentipedeShell shell, Centipede self) => self.Scutigera() ? new ScutigeraShell(shell.pos, shell.vel, shell.hue, shell.saturation, shell.scaleX, shell.scaleY) : (self.RedHorror() ? new RedHorrorShell(shell.pos, shell.vel, shell.hue, shell.saturation, shell.scaleX, shell.scaleY) : shell));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Violence! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchCall<Creature>("Violence")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((Centipede self) =>
             {
                 if (self.Scutigera())
                     self.stun = 0;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Centipede.Violence! (part 2)");
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            var instr = ins[i];
            if (instr.MatchCallOrCallvirt<Centipede>("get_Red"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag || self.Scutigera());
            }
            else if (instr.MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, Centipede self) => flag && !self.RedHorror());
            }
        }
    }

    internal static void IL_CentipedeAI_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Centiwing"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, CentipedeAI self) => flag && self.centipede?.RedHorror() is false);
            }
        }
    }

    internal static void On_CentipedeAI_ctor(On.CentipedeAI.orig_ctor orig, CentipedeAI self, AbstractCreature creature, World world)
    {
        orig(self, creature, world);
        if (self.centipede?.Scutigera() is true)
            self.pathFinder.stepsPerFrame = 15;
    }

    internal static void On_CentipedeAI_CreatureSpotted(On.CentipedeAI.orig_CreatureSpotted orig, CentipedeAI self, bool firstSpot, Tracker.CreatureRepresentation creatureRep)
    {
        orig(self, firstSpot, creatureRep);
        if (creatureRep.representedCreature is AbstractCreature acrit && acrit.realizedCreature is Creature c)
        {
            var tp = self.StaticRelationship(acrit).type;
            if (!c.dead && self.centipede is Centipede cent && cent.room is Room rm && !cent.dead && self.DoIWantToShockCreature(acrit) && (tp == CreatureTemplate.Relationship.Type.Eats || tp == CreatureTemplate.Relationship.Type.Attacks) && cent.bodyChunks is BodyChunk[] cAr)
            {
                if (cent.Scutigera())
                {
                    for (var i = 0; i < cAr.Length; i++)
                    {
                        var b = cAr[i];
                        if (Random.value < .1f)
                            rm.AddObject(new ScutigeraFlash(b.pos, b.rad / (b.rad * 30f), cent));
                    }
                }
                else if (cent.RedHorror())
                {
                    for (var i = 0; i < cAr.Length; i++)
                    {
                        var b = cAr[i];
                        if (Random.value < .1f)
                            rm.AddObject(new RedHorrorFlash(b.pos, b.rad / (b.rad * 30f)));
                    }
                }
            }
        }
    }

    internal static bool On_CentipedeAI_DoIWantToShockCreature(On.CentipedeAI.orig_DoIWantToShockCreature orig, CentipedeAI self, AbstractCreature critter)
    {
        var result = orig(self, critter);
        if (self.centipede is Centipede ce && ce.Scutigera() && critter.realizedCreature is Creature c && c is not Centipede)
        {
            if (c.dead || c.grasps is not Creature.Grasp[] ar)
                result = false;
            else if (ce.CentiState?.health < .4f || self.StaticRelationship(critter).type == CreatureTemplate.Relationship.Type.Afraid)
                result = true;
            else
            {
                for (var i = 0; i < ar.Length; i++)
                {
                    if (ar[i]?.grabbed is Weapon)
                    {
                        result = true;
                        break;
                    }
                    else
                        result = false;
                }
            }
        }
        return result;
    }

    internal static void IL_CentipedeAI_Update(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchStfld<NoiseTracker>("hearingSkill")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((CentipedeAI self) =>
             {
                 if (self.centipede?.Scutigera() is true && self.noiseTracker is NoiseTracker t)
                     t.hearingSkill = 1.5f;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeAI.Update! (part 1)");
        if (!c.TryGotoNext(MoveType.After,
            x => x.MatchLdcR4(.1f),
            x => x.MatchCall<Mathf>("Lerp"),
            x => x.MatchStfld<CentipedeAI>("excitement")))
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeAI.Update! (from 1 to 2)");
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeAI>("centipede"),
            x => x.MatchCallOrCallvirt<Centipede>("get_Centiwing")))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((bool flag, CentipedeAI self) => flag || (self.centipede?.Scutigera() is true));
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeAI.Update! (part 2)");
    }

    internal static void IL_CentipedeAI_VisualScore(ILContext il)
    {
        var c = new ILCursor(il);
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Red"))
            {
                c.Goto(i, MoveType.After)
                 .Emit(OpCodes.Ldarg_0)
                 .EmitDelegate((bool flag, CentipedeAI self) => flag || (self.centipede?.Scutigera() is true));
            }
        }
    }

    internal static CreatureTemplate.Relationship On_CentipedeAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var result = orig(self, dRelation);
        if (self.centipede is Centipede c && c.Scutigera())
        {
            if (dRelation?.trackerRep is Tracker.CreatureRepresentation rep && rep.representedCreature is AbstractCreature acrit && (result.type == CreatureTemplate.Relationship.Type.Attacks || result.type == CreatureTemplate.Relationship.Type.Eats))
            {
                if (self.DoIWantToShockCreature(acrit))
                {
                    result.type = CreatureTemplate.Relationship.Type.Eats;
                    result.intensity = 1f;
                    if (self.preyTracker is PreyTracker tr)
                        tr.currentPrey = new(tr, rep);
                }
                else
                {
                    result.type = CreatureTemplate.Relationship.Type.Ignores;
                    result.intensity = 0f;
                }
            }
        }
        return result;
    }

    internal static Color On_CentipedeGraphics_get_SecondaryShellColor(Func<CentipedeGraphics, Color> orig, CentipedeGraphics self)
    {
        var res = orig(self);
        if (self.centipede is Centipede c && c.Scutigera())
        {
            var state = Random.state;
            Random.InitState(c.abstractCreature.ID.RandomSeed);
            res = Color.Lerp(res, new(res.r + .2f, res.g + .2f, res.b + .2f), Mathf.Lerp(.1f, .2f, Random.value));
            Random.state = state;
        }
        return res;
    }

    internal static void On_CentipedeGraphics_ctor(On.CentipedeGraphics.orig_ctor orig, CentipedeGraphics self, PhysicalObject ow)
    {
        orig(self, ow);
        if (self.centipede is Centipede c)
        {
            if (c.Scutigera())
            {
                var state = Random.state;
                Random.InitState(c.abstractCreature.ID.RandomSeed);
                self.hue = Mathf.Lerp(.1527777777777778f, .1861111111111111f, Random.value);
                self.saturation = Mathf.Lerp(.294f, .339f, Random.value);
                self.wingPairs = c.bodyChunks.Length;
                var tot = self.totSegs;
                var wl = self.wingLengths = new float[tot];
                for (var j = 0; j < wl.Length; j++)
                {
                    var num = j / (tot - 1f);
                    var num2 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(.5f, 0f, num), .75f) * Mathf.PI);
                    num2 *= 1f - num;
                    var num3 = Mathf.Sin(Mathf.Pow(Mathf.InverseLerp(1f, .5f, num), .75f) * Mathf.PI);
                    num3 *= num;
                    num2 = .5f + .5f * num2;
                    num3 = .5f + .5f * num3;
                    wl[j] = Mathf.Lerp(3f, Custom.LerpMap(c.size, .5f, 1f, 60f, 80f), Mathf.Max(num2, num3) - Mathf.Sin(num * Mathf.PI) * .25f);
                }
                Random.state = state;
            }
            else if (c.RedHorror())
            {
                self.hue = Mathf.Lerp(-.02f, .01f, Random.value);
                self.saturation = .9f + .1f * Random.value;
            }
        }
    }

    internal static void IL_CentipedeGraphics_DrawSprites(ILContext il)
    {
        var c = new ILCursor(il);
        var cnt = 0;
        var ins = il.Instrs;
        for (var i = 0; i < ins.Count; i++)
        {
            if (ins[i].MatchCallOrCallvirt<Centipede>("get_Red"))
            {
                ++cnt;
                if (cnt == 2)
                {
                    c.Goto(i, MoveType.After)
                     .Emit(OpCodes.Ldarg_0)
                     .EmitDelegate((bool flag, CentipedeGraphics self) => flag && self.centipede?.RedHorror() is false);
                    break;
                }
            }
        }
    }

    internal static void On_CentipedeGraphics_DrawSprites(On.CentipedeGraphics.orig_DrawSprites orig, CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.centipede is Centipede c)
        {
            if (c.Scutigera())
            {
                for (var i = 0; i < c.bodyChunks.Length; i++)
                {
                    var spr = sLeaser.sprites[self.ShellSprite(i, 0)];
                    if (spr.element.name is "CentipedeBackShell")
                        spr.element = Futile.atlasManager.GetElementWithName("ScutigeraBackShell");
                    else if (spr.element.name is "CentipedeBellyShell")
                        spr.element = Futile.atlasManager.GetElementWithName("ScutigeraBellyShell");
                }
                for (var k = 0; k < 2; k++)
                {
                    for (var num15 = 0; num15 < self.wingPairs; num15++)
                    {
                        if (sLeaser.sprites[self.WingSprite(k, num15)] is CustomFSprite cSpr)
                        {
                            var vector1 = (num15 != 0) ? Custom.DirVec(self.ChunkDrawPos(num15 - 1, timeStacker), self.ChunkDrawPos(num15, timeStacker)) : Custom.DirVec(self.ChunkDrawPos(0, timeStacker), self.ChunkDrawPos(1, timeStacker));
                            var vector2 = Custom.PerpendicularVector(vector1);
                            var vector3 = self.RotatAtChunk(num15, timeStacker);
                            var vector4 = self.WingPos(k, num15, vector1, vector2, vector3, timeStacker);
                            var vector5 = self.ChunkDrawPos(num15, timeStacker) + c.bodyChunks[num15].rad * (k != 0 ? 1f : -1f) * vector2 * vector3.y;
                            cSpr.MoveVertice(1, vector4 + vector1 * 2f - camPos);
                            cSpr.MoveVertice(0, vector4 - vector1 * 2f - camPos);
                            cSpr.MoveVertice(2, vector5 + vector1 * 2f - camPos);
                            cSpr.MoveVertice(3, vector5 - vector1 * 2f - camPos);
                            cSpr.verticeColors[1] = cSpr.verticeColors[0] = self.SecondaryShellColor;
                            cSpr.verticeColors[3] = cSpr.verticeColors[2] = self.blackColor;
                        }
                    }
                }
            }
            else if (c.RedHorror())
            {
                for (var i = 0; i < c.bodyChunks.Length; i++)
                {
                    var spr = sLeaser.sprites[self.ShellSprite(i, 0)];
                    if (spr.element.name is "CentipedeBackShell")
                        spr.element = Futile.atlasManager.GetElementWithName("RedHorrorBackShell");
                    else if (spr.element.name is "CentipedeBellyShell")
                        spr.element = Futile.atlasManager.GetElementWithName("RedHorrorBellyShell");
                }
                for (var k = 0; k < 2; k++)
                {
                    for (var num15 = 0; num15 < self.wingPairs; num15++)
                    {
                        if (sLeaser.sprites[self.WingSprite(k, num15)] is CustomFSprite cSpr)
                        {
                            cSpr.verticeColors[1] = cSpr.verticeColors[0] = PinkToBlue(cSpr.verticeColors[0]);
                            cSpr.verticeColors[2] = cSpr.verticeColors[3] = PinkToBlue2(cSpr.verticeColors[3]);
                        }
                    }
                }
            }
        }
    }

    internal static void IL_CentipedeGraphics_InitiateSprites(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(
            x => x.MatchLdarg(0),
            x => x.MatchLdarg(1),
            x => x.MatchLdarg(2),
            x => x.MatchLdnull(),
            x => x.MatchCallOrCallvirt<GraphicsModule>("AddToContainer")))
        {
            ++c.Index;
            c.Emit(OpCodes.Ldarg_1)
             .EmitDelegate((CentipedeGraphics self, RoomCamera.SpriteLeaser sLeaser) =>
             {
                 if (self.centipede is Centipede c)
                 {
                     if (c.Scutigera())
                     {
                         for (var l = 0; l < 2; l++)
                         {
                             for (var num = 0; num < self.wingPairs; num++)
                                 sLeaser.sprites[self.WingSprite(l, num)] = new CustomFSprite("ScutigeraWing");
                         }
                         for (var i = 0; i < c.bodyChunks.Length; i++)
                         {
                             sLeaser.sprites[self.SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("ScutigeraSegment");
                             for (var j = 0; j < 2; j++)
                                 sLeaser.sprites[self.LegSprite(i, j, 1)].element = Futile.atlasManager.GetElementWithName("ScutigeraLegB");
                         }
                     }
                     else if (c.RedHorror())
                     {
                         for (var l = 0; l < 2; l++)
                         {
                             for (var num = 0; num < self.wingPairs; num++)
                                 sLeaser.sprites[self.WingSprite(l, num)] = new CustomFSprite("CentipedeWing") { shader = Custom.rainWorld.Shaders["CicadaWing"] };
                         }
                         for (var i = 0; i < c.bodyChunks.Length; i++)
                             sLeaser.sprites[self.SegmentSprite(i)].element = Futile.atlasManager.GetElementWithName("RedHorrorSegment");
                     }
                 }
             });
            c.Emit(OpCodes.Ldarg_0);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeGraphics.InitiatesSprites!");
    }

    internal static void IL_CentipedeGraphics_Update(ILContext il)
    {
        var c = new ILCursor(il);
        var loc = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<CentipedeGraphics>("lightSource"),
            x => x.MatchLdloc(out loc),
            x => x.MatchLdloc(loc),
            x => x.MatchLdcR4(1f),
            x => x.MatchNewobj<Color>()))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit<CentipedeGraphics>(OpCodes.Ldfld, "centipede")
             .EmitDelegate((Color color, Centipede self) => self?.Scutigera() is true ? new(color.r, color.b, color.g) : color);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook CentipedeGraphics.Update!");
    }

    internal static float On_CentipedeGraphics_WhiskerLength(On.CentipedeGraphics.orig_WhiskerLength orig, CentipedeGraphics self, int part) => self.centipede?.Scutigera() is true ? (part != 0 ? 48f : 44f) : orig(self, part);

    public static Color PinkToBlue(Color clr) => Color.Lerp(new(clr.b, clr.g, clr.r), Color.white, .35f);

    public static Color PinkToBlue2(Color clr) => Color.Lerp(Color.Lerp(new(clr.b, clr.g, clr.r), Color.white, .35f), Color.blue, .2f);

    public static bool RedHorror(this Centipede self) => self.Template.type == CreatureTemplateType.RedHorrorCenti;

    public static bool Scutigera(this Centipede self) => self.Template.type == CreatureTemplateType.Scutigera;

    public static Color ShockColorIfScut(float nr, float ng, float nb, Centipede self) => self.Scutigera() ? new(nr, nb, ng) : new(nr, ng, nb);
}