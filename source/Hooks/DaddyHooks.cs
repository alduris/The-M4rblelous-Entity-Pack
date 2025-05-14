global using static LBMergedMods.Hooks.DaddyHooks;
using System.Runtime.CompilerServices;
using UnityEngine;
using RWCustom;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Noise;
using System.Collections.Generic;

namespace LBMergedMods.Hooks;
//CHK
public static class DaddyHooks
{
    public static Dictionary<string, Color> JLLRooms = [];

    internal static CreatureTemplate.Relationship On_DaddyAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.DaddyAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DaddyAI self, RelationshipTracker.DynamicRelationship dRelation)
    {
        var rel = orig(self, dRelation);
        if (Jelly.TryGetValue(self.creature, out var j) && j.IsJelly && dRelation.trackerRep?.representedCreature?.realizedCreature is DaddyLongLegs)
            rel = new(CreatureTemplate.Relationship.Type.Ignores, 0f);
        return rel;
    }

    internal static void IL_DaddyGraphics_ReactToNoise(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchNewobj_DaddyBubble))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_2)
             .EmitDelegate((DaddyBubble bubble, DaddyGraphics self, InGameNoise noise) =>
             {
                 if (self is JellyGraphics j && j.daddy?.firstChunk is BodyChunk b)
                 {
                     bubble.owner = j;
                     var dir = Custom.DirVec(b.pos, noise.pos) * 12f / (1f + Random.value / 4f);
                     bubble.direction = dir.normalized;
                     bubble.vel = dir + Custom.DegToVec(Random.value * 360f) * (Random.value * Random.value * 12f);
                 }
                 return bubble;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook DaddyGraphics.ReactToNoise! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchNewobj_DaddyRipple))
        {
            c.Emit(OpCodes.Ldarg_0)
             .EmitDelegate((DaddyRipple ripple, DaddyGraphics self) =>
             {
                 if (self is JellyGraphics j)
                     ripple.owner = j;
                 return ripple;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook DaddyGraphics.ReactToNoise! (part 2)");
    }

    internal static void On_Eye_RenderSlits(On.DaddyGraphics.Eye.orig_RenderSlits orig, DaddyGraphics.Eye self, Vector2 pos, Vector2 middleOfBody, float rotation, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (self.rotOwner is JellyGraphics j)
        {
            var eye = j.Eyes[self.index];
            var b = Vector2.Lerp(eye.lastDir, eye.dir, timeStacker);
            eye.centerRenderPos = pos + Vector2.Lerp(Custom.DirVec(middleOfBody, pos) * Mathf.Lerp(Mathf.InverseLerp(0f, Mathf.Lerp(30f, 50f, self.rotats[1]), Vector2.Distance(middleOfBody, pos + Custom.DirVec(middleOfBody, pos) * self.rad)) * .9f, 1f, .5f * Mathf.Max(Mathf.Lerp(eye.lastFocus, eye.focus, timeStacker) * Mathf.Pow(Mathf.InverseLerp(-1f, 1f, Vector2.Dot(Custom.DirVec(middleOfBody, pos), b.normalized)), .7f), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(eye.lastClosed, eye.closed, timeStacker)), .6f))), b, b.magnitude * .5f) * self.rad;
        }
        else
            orig(self, pos, middleOfBody, rotation, sLeaser, rCam, timeStacker, camPos);
    }

    internal static void On_DaddyLongLegs_InitiateGraphicsModule(On.DaddyLongLegs.orig_InitiateGraphicsModule orig, DaddyLongLegs self)
    {
        if (Jelly.TryGetValue(self.abstractCreature, out var j) && j.IsJelly)
        {
            if (self.graphicsModule is not JellyGraphics)
                self.graphicsModule = new JellyGraphics(self);
        }
        else
            orig(self);
    }

    internal static void IL_DaddyLongLegs_ctor(ILContext il)
    {
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            s_MatchNewarr_BodyChunk))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .Emit(OpCodes.Ldarg_2)
             .EmitDelegate((BodyChunk[] array, DaddyLongLegs self, AbstractCreature abstractCreature, World world) =>
             {
                 if (!Jelly.TryGetValue(abstractCreature, out var j))
                     Jelly.Add(abstractCreature, j = new()
                     {
                         Born = true,
                         IsJelly = abstractCreature.Room.JellyRooms() && !self.HDmode
                     });
                 return j.IsJelly ? new BodyChunk[1] : array;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook DaddyLongLegs.ctor! (part 1)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchNewobj_BodyChunk))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .Emit(OpCodes.Ldarg_2)
             .EmitDelegate((BodyChunk b, DaddyLongLegs self, AbstractCreature abstractCreature, World world) =>
             {
                 if (Jelly.TryGetValue(abstractCreature, out var j) && j.IsJelly)
                 {
                     self.colorClass = true;
                     self.JellyColor(abstractCreature, world);
                     j.Color = self.effectColor;
                     if (j.DevSpawnColor is Color clr)
                         j.Color = self.effectColor = self.eyeColor = clr;
                     var num = self.SizeClass ? 12f : 8f;
                     if (ModManager.DLCShared && abstractCreature.superSizeMe)
                         num = 18f;
                     var num5 = Mathf.Lerp(num * .9f, num, Random.value);
                     j.IconRadBonus = (num5 * 3.5f + 3.5f) * .005f;
                     return new(self, 0, default, num5 * 3.5f + 3.5f, num5);
                 }
                 return b;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook DaddyLongLegs.ctor! (part 2)");
        if (c.TryGotoNext(MoveType.After,
            s_MatchNewarr_PhysicalObject_BodyChunkConnection))
        {
            c.Emit(OpCodes.Ldarg_0)
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((PhysicalObject.BodyChunkConnection[] ar, DaddyLongLegs self, AbstractCreature abstractCreature) =>
             {
                 if (Jelly.TryGetValue(abstractCreature, out var j) && j.IsJelly)
                     return [];
                 return ar;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook DaddyLongLegs.ctor! (part 3)");
    }

    internal static Color On_DaddyLongLegs_ShortCutColor(On.DaddyLongLegs.orig_ShortCutColor orig, DaddyLongLegs self) => Jelly.TryGetValue(self.abstractCreature, out var j) && j.IsJelly ? self.eyeColor : orig(self);

    internal static void On_DaddyTentacle_CollideWithCreature(On.DaddyTentacle.orig_CollideWithCreature orig, DaddyTentacle self, int tChunk, BodyChunk creatureChunk)
    {
        if (self.daddy is DaddyLongLegs d && Jelly.TryGetValue(d.abstractCreature, out var j) && j.IsJelly && creatureChunk?.owner is DaddyLongLegs)
            return;
        orig(self, tChunk, creatureChunk);
    }

    [SuppressMessage(null, "IDE0060"), MethodImpl(MethodImplOptions.NoInlining)]
    public static void JellyColor(this DaddyLongLegs self, AbstractCreature crit, World world)
    {
        var rm = crit.Room;
        if (JLLRooms.TryGetValue(rm.name, out var col) && col.r >= 0f)
        {
            if (col.g < 0f)
            {
                var f1 = AssetManager.ResolveFilePath("levels/" + rm.FileName + "_jellylonglegs.txt");
                if (!File.Exists(f1))
                {
                    if (self.world?.name is string s)
                    {
                        f1 = AssetManager.ResolveFilePath("world/" + s.ToLower() + "-rooms/" + rm.FileName + "_jellylonglegs.txt");
                        if (!File.Exists(f1))
                            f1 = null;
                    }
                    else
                        f1 = null;
                }
                if (f1 is not null)
                {
                    var colorParams = File.ReadAllText(f1).Replace(" ", string.Empty).Split(',');
                    if (colorParams.Length >= 3)
                    {
                        float.TryParse(colorParams[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var r);
                        float.TryParse(colorParams[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var g);
                        float.TryParse(colorParams[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var b);
                        JLLRooms[rm.name] = self.eyeColor = self.effectColor = new(r, g, b);
                        return;
                    }
                }
                JLLRooms[rm.name] = new(0f, 0f, -1f);
                self.eyeColor = self.effectColor = Color.Lerp(new(146f / 255f, 33f / 255f, 191f / 255f), Color.blue, ModManager.DLCShared && crit.superSizeMe ? .3f : (self.SizeClass ? .15f : 0f));
            }
            else if (col.b < 0f)
                self.eyeColor = self.effectColor = Color.Lerp(new(146f / 255f, 33f / 255f, 191f / 255f), Color.blue, ModManager.DLCShared && crit.superSizeMe ? .3f : (self.SizeClass ? .15f : 0f));
            else
                self.eyeColor = self.effectColor = col;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool JellyRooms(this AbstractRoom self)
    {
        var nm = self.FileName;
        if (JLLRooms.TryGetValue(nm, out var col))
            return col.r >= 0f;
        var res = self.name is "reef" or "cavity" or "pump" ||
            File.Exists(AssetManager.ResolveFilePath("levels/" + nm + "_jellylonglegs.txt")) ||
            (self.world?.name is string s && (s == "RF" || File.Exists(AssetManager.ResolveFilePath("world/" + s.ToLower() + "-rooms/" + nm + "_jellylonglegs.txt"))));
        JLLRooms.Add(self.name, new(res ? 0f : -1f, -1f, 0f));
        return res;
    }
}