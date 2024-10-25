global using static LBMergedMods.Hooks.ArenaHooks;
using System.Reflection;
using System;
using ArenaBehaviors;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Random = UnityEngine.Random;

namespace LBMergedMods.Hooks;

public static class ArenaHooks
{
    internal static bool On_ArenaCreatureSpawner_IsMajorCreature(On.ArenaCreatureSpawner.orig_IsMajorCreature orig, CreatureTemplate.Type type) => type == CreatureTemplateType.RedHorrorCenti || type == CreatureTemplateType.FlyingBigEel || type == CreatureTemplateType.FatFireFly || type == CreatureTemplateType.Blizzor || orig(type);

    internal static void On_ArenaGameSession_SpawnItem(On.ArenaGameSession.orig_SpawnItem orig, ArenaGameSession self, Room room, PlacedObject placedObj)
    {
        var data = (placedObj.data as PlacedObject.MultiplayerItemData)!;
        if (data.type != MultiplayerItemType.ThornyStrawberry && data.type != MultiplayerItemType.LittleBalloon && data.type != MultiplayerItemType.BouncingMelon && data.type != MultiplayerItemType.Physalis && data.type != MultiplayerItemType.LimeMushroom && data.type != MultiplayerItemType.MarineEye)
            orig(self, room, placedObj);
        else if (!self.SpawnDefaultRoomItems || Random.value > data.chance)
        {
            var tp = new AbstractPhysicalObject.AbstractObjectType(data.type.value);
            if (tp.Index < 0)
                tp = AbstractPhysicalObject.AbstractObjectType.Spear;
            else if (self.arenaSitting.multiplayerUnlocks.ExoticItems < 1f)
            {
                var sandboxID = new MultiplayerUnlocks.SandboxUnlockID(data.type.value);
                if ((sandboxID.Index < 0 || !self.arenaSitting.multiplayerUnlocks.SandboxItemUnlocked(sandboxID)) && Random.value > self.arenaSitting.multiplayerUnlocks.ExoticItems)
                    tp = AbstractPhysicalObject.AbstractObjectType.Spear;
            }
            if (tp == AbstractPhysicalObject.AbstractObjectType.Spear)
                room.abstractRoom.entities.Add(new AbstractSpear(room.world, null, room.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), false));
            else
                room.abstractRoom.entities.Add(new AbstractConsumable(room.world, tp, null, room.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), -2, -2, null));
        }
    }

    internal static bool On_CollectToken_AvailableToPlayer(On.CollectToken.orig_AvailableToPlayer orig, CollectToken self)
    {
        if (self.placedObj?.data is CollectToken.CollectTokenData d)
        {
            if (d.SandboxUnlock is MultiplayerUnlocks.SandboxUnlockID id && id.Index < 0)
                return false;
        }
        return orig(self);
    }

    internal static void IL_MultiplayerMenu_ctor(ILContext il)
    {
        int loc0 = 0, loc2 = 0;
        ILLabel? label = null;
        MethodInfo? Substring = null;
        var c = new ILCursor(il);
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out loc0),
            x => x.MatchLdloc(out loc2),
            x => x.MatchLdelemRef(),
            x => x.MatchLdloc(loc0),
            x => x.MatchLdloc(loc2),
            x => x.MatchLdelemRef(),
            x => x.MatchCallOrCallvirt<string>("get_Length"),
            x => x.MatchLdcI4(4),
            x => x.MatchSub(),
            x => x.MatchLdcI4(4),
            x => x.MatchCallOrCallvirt(Substring = typeof(string).GetMethod(nameof(string.Substring), [typeof(int), typeof(int)])),
            x => x.MatchLdstr(".txt"),
            x => x.MatchCall(typeof(string).GetMethod("op_Equality", [typeof(string), typeof(string)])),
            x => x.MatchBrfalse(out label))
            && label is not null)
        {
            VariableDefinition var0 = il.Body.Variables[loc0],
                var2 = il.Body.Variables[loc2];
            c.Emit(OpCodes.Ldloc, var0)
             .Emit(OpCodes.Ldloc, var2)
             .Emit(OpCodes.Ldelem_Ref)
             .Emit(OpCodes.Ldloc, var0)
             .Emit(OpCodes.Ldloc, var2)
             .Emit(OpCodes.Ldelem_Ref)
             .Emit<string>(OpCodes.Callvirt, "get_Length")
             .Emit(OpCodes.Ldc_I4, 18)
             .Emit(OpCodes.Sub)
             .Emit(OpCodes.Ldc_I4, 18)
             .Emit(OpCodes.Callvirt, il.Import(Substring))
             .Emit(OpCodes.Ldstr, "_jellylonglegs.txt")
             .Emit(OpCodes.Call, il.Import(typeof(string).GetMethod("op_Inequality", [typeof(string), typeof(string)])))
             .Emit(OpCodes.Brfalse, label);
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook Menu.MultiplayerMenu.ctor!");
    }

    internal static MultiplayerUnlocks.SandboxUnlockID On_MultiplayerUnlocks_SandboxUnlockForSymbolData(On.MultiplayerUnlocks.orig_SandboxUnlockForSymbolData orig, IconSymbol.IconSymbolData data)
    {
        var res = orig(data);
        if (data.critType == CreatureTemplate.Type.Fly && data.intData == SEED_DATA)
            res = SandboxUnlockID.SeedBat;
        else if (data.critType == CreatureTemplate.Type.TubeWorm && data.intData == GRUB_DATA)
            res = SandboxUnlockID.Bigrub;
        return res;
    }

    internal static IconSymbol.IconSymbolData On_MultiplayerUnlocks_SymbolDataForSandboxUnlock(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
    {
        var res = orig(unlockID);
        if (unlockID == SandboxUnlockID.SeedBat)
            res = new(CreatureTemplate.Type.Fly, AbstractPhysicalObject.AbstractObjectType.Creature, SEED_DATA);
        else if (unlockID == SandboxUnlockID.Bigrub)
            res = new(CreatureTemplate.Type.TubeWorm, AbstractPhysicalObject.AbstractObjectType.Creature, GRUB_DATA);
        return res;
    }

    internal static void On_SandboxEditor_GetPerformanceEstimate(On.ArenaBehaviors.SandboxEditor.orig_GetPerformanceEstimate orig, SandboxEditor.PlacedIcon placedIcon, ref float exponentialPart, ref float linearPart)
    {
        if (placedIcon is SandboxEditor.CreatureOrItemIcon i && (i.iconData.itemType == AbstractObjectType.LittleBalloon || i.iconData.itemType == AbstractObjectType.Physalis))
            linearPart += .6f;
        else
            orig(placedIcon, ref exponentialPart, ref linearPart);
    }

    internal static void IL_SandboxGameSession_SpawnEntity(ILContext il)
    {
        var c = new ILCursor(il);
        var loc = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<GameSession>("game"),
            x => x.MatchCallOrCallvirt<RainWorldGame>("get_world"),
            x => x.MatchLdcI4(0),
            x => x.MatchCallOrCallvirt(typeof(World).GetMethod("GetAbstractRoom", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(int)], null)),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<GameSession>("game"),
            x => x.MatchCallOrCallvirt<RainWorldGame>("get_world"),
            x => x.MatchLdloc(out loc),
            x => x.MatchLdfld<IconSymbol.IconSymbolData>("critType"),
            x => x.MatchCall(typeof(StaticWorld).GetMethod("GetCreatureTemplate", LBMergedModsPlugin.ALL_FLAGS, Type.DefaultBinder, [typeof(CreatureTemplate.Type)], null)),
            x => x.MatchLdnull(),
            x => x.MatchLdloc(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchNewobj<AbstractCreature>()))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc])
             .EmitDelegate((AbstractCreature crit, IconSymbol.IconSymbolData data) =>
             {
                 if (data.critType == CreatureTemplate.Type.Fly && Seed.TryGetValue(crit, out var prop))
                 {
                     prop.Born = true;
                     prop.IsSeed = data.intData == SEED_DATA;
                 }
                 return crit;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SandboxGameSession.SpawnEntity! (part 1)");
        c.Index = 0;
        if (c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(out loc),
            x => x.MatchCallOrCallvirt<AbstractRoom>("AddEntity"))
        && c.TryGotoNext(MoveType.After,
            x => x.MatchLdloc(loc),
            x => x.MatchCallOrCallvirt<AbstractRoom>("AddEntity")))
        {
            c.Emit(OpCodes.Ldloc, il.Body.Variables[loc])
             .Emit(OpCodes.Ldarg_1)
             .EmitDelegate((AbstractCreature crit, SandboxEditor.PlacedIconData placedIconData) =>
             {
                 if (placedIconData.data.critType == CreatureTemplate.Type.TubeWorm && Big.TryGetValue(crit, out var prop))
                 {
                     prop.Born = true;
                     prop.IsBig = placedIconData.data.intData == GRUB_DATA;
                 }
                 else if (placedIconData.data.critType == CreatureTemplate.Type.Hazer && Albino.TryGetValue(crit, out var prop2))
                     prop2.Value = placedIconData.data.intData == HAZER_DATA;
             });
        }
        else
            LBMergedModsPlugin.s_logger.LogError("Couldn't ILHook SandboxGameSession.SpawnEntity! (part 2)");
    }

    internal static void On_SandboxSettingsInterface_DefaultKillScores(On.Menu.SandboxSettingsInterface.orig_DefaultKillScores orig, ref int[] killScores)
    {
        orig(ref killScores);
        killScores[(int)SandboxUnlockID.Bigrub] = 2;
    }

    internal static bool On_SandboxSettingsInterface_IsThisSandboxUnlockVisible(On.Menu.SandboxSettingsInterface.orig_IsThisSandboxUnlockVisible orig, MultiplayerUnlocks.SandboxUnlockID sandboxUnlockID) => orig(sandboxUnlockID) && sandboxUnlockID != SandboxUnlockID.SeedBat && sandboxUnlockID != SandboxUnlockID.Bigrub;
}